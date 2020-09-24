// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;    
    using Microsoft.Practices.ServiceLocation;
    using ReactiveUI;
    using ReqIFDal;
    using ReqIFSharp;

    /// <summary>
    /// The view-model for the ribbon controls related to importing and exporting ReqIF files
    /// </summary>
    public class ReqIfRibbonViewModel : ReactiveObject
    {
        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/>
        /// </summary>
        protected readonly IThingDialogNavigationService ThingDialogNavigationService;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        protected readonly IDialogNavigationService DialogNavigationService;
        
        /// <summary>
        /// The <see cref="IPluginSettingsService"/>
        /// </summary>
        protected readonly IPluginSettingsService PluginSettingsService;

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        protected readonly IOpenSaveFileDialogService OpenSaveDialogService;

        /// <summary>
        /// Backing field for <see cref="CanImportExport"/>
        /// </summary>
        private bool canImportExport;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfRibbonViewModel"/> class
        /// </summary>
        public ReqIfRibbonViewModel()
        {
            this.DialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            this.ThingDialogNavigationService = ServiceLocator.Current.GetInstance<IThingDialogNavigationService>();
            this.PluginSettingsService = ServiceLocator.Current.GetInstance<IPluginSettingsService>();
            this.OpenSaveDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

            this.Sessions = new ReactiveList<ISession>();
            this.Iterations = new ReactiveList<Iteration>();
            this.Iterations.ChangeTrackingEnabled = true;

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Iteration))
                .Subscribe(this.IterationEventHandler);

            this.Iterations.CountChanged.Subscribe(x => { this.CanImportExport = (x > 0); });
            var isImportExportEnable = this.WhenAnyValue(x => x.CanImportExport);

            this.ExportCommand = ReactiveCommand.Create(isImportExportEnable);
            this.ExportCommand.Subscribe(x => this.ExecuteExportCommand());

            this.ImportCommand = ReactiveCommand.Create(isImportExportEnable);
            this.ImportCommand.Subscribe(x => this.ExecuteImportCommand());
        }

        /// <summary>
        /// Gets or sets a value indicating whether the export command is enabled
        /// </summary>
        public bool CanImportExport
        {
            get { return this.canImportExport; }
            set { this.RaiseAndSetIfChanged(ref this.canImportExport, value); }
        }

        /// <summary>
        /// Gets the ReqIF export <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> ExportCommand { get; private set; }

        /// <summary>
        /// Gets the ReqIF import <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> ImportCommand { get; private set; }

        /// <summary>
        /// Gets the List of <see cref="ISession"/> that are open
        /// </summary>
        public ReactiveList<ISession> Sessions { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="Iteration"/>s that are open
        /// </summary>
        public ReactiveList<Iteration> Iterations { get; private set; } 

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Iteration"/>s added
        /// </summary>
        /// <param name="iterationEvent">the <see cref="ObjectChangedEvent"/></param>
        private void IterationEventHandler(ObjectChangedEvent iterationEvent)
        {
            if (iterationEvent.EventKind == EventKind.Added)
            {
                var iteration = (Iteration)iterationEvent.ChangedThing;
                var session = this.Sessions.SingleOrDefault(s => s.Assembler.Cache == iteration.Cache);

                if (session == null)
                {
                    throw new InvalidOperationException("There is no ISession associated with an Iteration.");
                }

                this.Iterations.Add(iteration);
            }
            else if (iterationEvent.EventKind == EventKind.Removed)
            {
                var iteration = (Iteration)iterationEvent.ChangedThing;
                this.Iterations.Remove(iteration);
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Session"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (sessionChange.Status == SessionStatus.Open)
            {
                this.Sessions.Add(sessionChange.Session);
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.Sessions.Remove(sessionChange.Session);
                var iterationsToRemove = this.Iterations.Where(x => x.Cache == sessionChange.Session.Assembler.Cache);

                foreach (var iterationToRemove in iterationsToRemove)
                {
                    this.Iterations.Remove(iterationToRemove);
                }
            }
        }

        /// <summary>
        /// Executes the <see cref="ExportCommand"/>
        /// </summary>
        private void ExecuteExportCommand()
        {
             var reqifExportDialogViewModel = new ReqIfExportDialogViewModel(this.Sessions, this.Iterations, this.OpenSaveDialogService, new ReqIFSerializer(false));
             this.DialogNavigationService.NavigateModal(reqifExportDialogViewModel);
        }

        /// <summary>
        /// Executes the <see cref="ImportCommand"/>
        /// </summary>
        private void ExecuteImportCommand()
        {
            var reqifImportDialogViewModel = new ReqIfImportDialogViewModel(this.Sessions, this.Iterations, this.OpenSaveDialogService, this.PluginSettingsService, new ReqIFDeserializer());
            var result = (ReqIfImportResult)this.DialogNavigationService.NavigateModal(reqifImportDialogViewModel);

            if (result?.Result == null || !result.Result.Value)
            {
                return;
            }

            // Start the mapping process, owner cant be null if this is reached
            var session = this.Sessions.Single(x => x.Assembler.Cache == result.Iteration.Cache);
            var model = (EngineeringModel)result.Iteration.Container;
            var activeDomain = model.GetActiveParticipant(session.ActivePerson).SelectedDomain;

            var reqifToThingMapper = new ReqIfImportMappingManager(result.ReqIfObject, session, result.Iteration, activeDomain, this.DialogNavigationService, this.ThingDialogNavigationService, result.MappingConfiguration);
            reqifToThingMapper.StartMapping();
        }
    }
}
