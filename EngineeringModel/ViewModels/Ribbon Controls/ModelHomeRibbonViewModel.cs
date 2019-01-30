// -------------------------------------------------------------------------------------------------
// <copyright file="ModelHomeRibbonViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4EngineeringModel.Views;
    using Microsoft.Practices.ServiceLocation;
    using ReactiveUI;

    /// <summary>
    /// The View-Model for <see cref="ModelHomeRibbon"/> containing the controls in the "Home" Page for this module
    /// </summary>
    public class ModelHomeRibbonViewModel : ReactiveObject
    {
        #region Fields

        /// <summary>
        /// Backing field for <see cref="HasSession"/>
        /// </summary>
        private ObservableAsPropertyHelper<bool> hasSession;

        /// <summary>
        /// Backing field for <see cref="HasOpenIterations"/>
        /// </summary>
        private bool hasOpenIterations;

        #endregion

        #region constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelHomeRibbonViewModel"/> class
        /// </summary>
        public ModelHomeRibbonViewModel()
        {
            this.OpenSessions = new ReactiveList<ISession>();
            this.OpenSessions.ChangeTrackingEnabled = true;
            this.OpenSessions.CountChanged.Select(x => x != 0).ToProperty(this, x => x.HasSession, out this.hasSession);

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            this.OpenSelectIterationsCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.HasSession));
            this.OpenSelectIterationsCommand.Subscribe(_ => this.ExecuteOpenSelectIterationsCommand());

            this.CloseIterationsCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.HasOpenIterations));
            this.CloseIterationsCommand.Subscribe(_ => this.ExecuteCloseIterationsCommand());

            this.OpenDomainSwitchDialogCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.HasOpenIterations));
            this.OpenDomainSwitchDialogCommand.Subscribe(_ => this.ExecuteOpenDomainSwitchDialogCommand());
        }

        #endregion

        #region public properties
        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasSession
        {
            get { return this.hasSession.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether there are open <see cref="ModelReferenceDataLibrary"/> in any <see cref="ISession"/>
        /// </summary>
        public bool HasOpenIterations
        {
            get { return this.hasOpenIterations; }
            private set { this.RaiseAndSetIfChanged(ref this.hasOpenIterations, value); }
        }

        /// <summary>
        /// Gets a list of open <see cref="ISession"/>s
        /// </summary>
        public ReactiveList<ISession> OpenSessions { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to select and open a Iteration Selection Window
        /// </summary>
        public ReactiveCommand<object> OpenSelectIterationsCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to switch the domain for an iteration
        /// </summary>
        public ReactiveCommand<object> OpenDomainSwitchDialogCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to select and close Site RDLs
        /// </summary>
        public ReactiveCommand<object> CloseIterationsCommand { get; private set; }

        #endregion

        #region private method

        /// <summary>
        /// Executes the <see cref="OpenSelectIterationsCommand"/> command
        /// </summary>
        private void ExecuteOpenSelectIterationsCommand()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            var modelSelectionViewModel = new ModelOpeningDialogViewModel(this.OpenSessions);
            dialogService.NavigateModal(modelSelectionViewModel);
        }

        /// <summary>
        /// The execute close iterations command.
        /// </summary>
        private void ExecuteCloseIterationsCommand()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            var modelSelectionViewModel = new ModelClosingDialogViewModel(this.OpenSessions);
            dialogService.NavigateModal(modelSelectionViewModel);
        }

        /// <summary>
        /// The execute the <see cref="OpenDomainSwitchDialogCommand"/>
        /// </summary>
        private void ExecuteOpenDomainSwitchDialogCommand()
        {
            var dialogService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            var domainSwitchViewModel = new ModelIterationDomainSwitchDialogViewModel(this.OpenSessions);
            dialogService.NavigateModal(domainSwitchViewModel);
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
                this.OpenSessions.Add(sessionChange.Session);
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.OpenSessions.Remove(sessionChange.Session);
            }

            this.HasOpenIterations = this.OpenSessions.SelectMany(x => x.OpenIterations).Any();
        }

        #endregion
    }
}