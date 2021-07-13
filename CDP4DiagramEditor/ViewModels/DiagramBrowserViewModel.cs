// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramBrowserViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski.
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="DiagramBrowserViewModel"/> is to represent the view-model for <see cref="Diagram"/>s
    /// </summary>
    public class DiagramBrowserViewModel : BrowserViewModelBase<Iteration>, IPanelViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CanSetDefaultDiagram"/>
        /// </summary>
        private bool canSetDefaultDiagram;

        /// <summary>
        /// Backing field for <see cref="CanCreateDiagram"/>
        /// </summary>
        private bool canCreateDiagram;

        /// <summary>
        /// Backing field for <see cref="CurrentModel"/>
        /// </summary>
        private string currentModel;

        /// <summary>
        /// Backing field for <see cref="CurrentIteration"/>
        /// </summary>
        private int currentIteration;

        /// <summary>
        /// Backing field for <see cref="DomainOfExpertise"/>
        /// </summary>
        private string domainOfExpertise;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Diagrams";

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramBrowserViewModel"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="EngineeringModel"/></param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        public DiagramBrowserViewModel(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = string.Format("{0}, iteration_{1}", PanelCaption, this.Thing.IterationSetup.IterationNumber);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", ((EngineeringModel)this.Thing.Container).EngineeringModelSetup.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            this.UpdateDiagrams();

            this.AddSubscriptions();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the view model current <see cref="EngineeringModelSetup"/>
        /// </summary>
        public EngineeringModelSetup CurrentEngineeringModelSetup
        {
            get { return this.Thing.IterationSetup.GetContainerOfType<EngineeringModelSetup>(); }
        }

        /// <summary>
        /// Gets the current model caption to be displayed in the browser
        /// </summary>
        public string CurrentModel
        {
            get { return this.currentModel; }
            private set { this.RaiseAndSetIfChanged(ref this.currentModel, value); }
        }

        /// <summary>
        /// Gets the current iteration caption to be displayed in the browser
        /// </summary>
        public int CurrentIteration
        {
            get { return this.currentIteration; }
            private set { this.RaiseAndSetIfChanged(ref this.currentIteration, value); }
        }

        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise"/> name
        /// </summary>
        public string DomainOfExpertise
        {
            get { return this.domainOfExpertise; }
            private set { this.RaiseAndSetIfChanged(ref this.domainOfExpertise, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the set default Diagram command is enabled
        /// </summary>
        public bool CanSetDefaultDiagram
        {
            get { return this.canSetDefaultDiagram; }
            set { this.RaiseAndSetIfChanged(ref this.canSetDefaultDiagram, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the create Diagram command is enabled
        /// </summary>
        public bool CanCreateDiagram
        {
            get { return this.canCreateDiagram; }
            set { this.RaiseAndSetIfChanged(ref this.canCreateDiagram, value); }
        }

        /// <summary>
        /// Gets the rows representing <see cref="Diagram"/>s
        /// </summary>
        public ReactiveList<DiagramCanvasRowViewModel> Diagrams { get; private set; }

        /// <summary>
        /// Initializes the browser
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.Diagrams = new ReactiveList<DiagramCanvasRowViewModel>();
        }

        /// <summary>
        /// Initializes the <see cref="ICommand"/>s
        /// </summary>
        protected override void InitializeCommands()
        {
            var canDelete = this.WhenAnyValue(
               vm => vm.SelectedThing,
               vm => vm.CanWriteSelectedThing,
               (selection, canWrite) => selection != null && canWrite);

            this.CreateCommand = ReactiveCommand.Create(this.WhenAnyValue(vm => vm.CanCreateDiagram));
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<DiagramCanvas>(this.Thing));

            this.DeleteCommand = ReactiveCommand.Create(canDelete);
            this.DeleteCommand.Subscribe(_ => this.ExecuteDeleteCommand(this.SelectedThing.Thing));
            this.UpdateCommand = ReactiveCommand.Create(canDelete);
            this.UpdateCommand.Subscribe(_ => this.ExecuteUpdateCommand());
            this.InspectCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.SelectedThing).Select(x => x != null));
            this.InspectCommand.Subscribe(_ => this.ExecuteUpdateCommand());
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateDiagrams();
            this.UpdateProperties();
        }

        /// <summary>
        /// Compute the permissions
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();
            this.CanCreateDiagram = this.PermissionService.CanWrite(ClassKind.DiagramCanvas, this.Thing);
            this.CanWriteSelectedThing = true;
        }

        /// <summary>
        /// Populate the context menu
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();
            this.ContextMenu.Add(new ContextMenuItemViewModel("Create a Diagram", "", this.CreateCommand, MenuItemKind.Create, ClassKind.DiagramCanvas));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// a value indicating whether the class is being disposed of
        /// </param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var iteration in this.Diagrams)
            {
                iteration.Dispose();
            }
        }

        /// <summary>
        /// Update the <see cref="Diagrams"/> List
        /// </summary>
        private void UpdateDiagrams()
        {
            var newDiagrams = this.Thing.DiagramCanvas.Except(this.Diagrams.Select(x => x.Thing)).ToList();
            var oldDiagrams = this.Diagrams.Select(x => x.Thing).Except(this.Thing.DiagramCanvas).ToList();

            foreach (var diagram in newDiagrams)
            {
                var row = new DiagramCanvasRowViewModel(diagram, this.Session, this);
                row.Index = this.Thing.DiagramCanvas.IndexOf(diagram);

                this.Diagrams.Add(row);
            }

            foreach (var diagram in oldDiagrams)
            {
                var row = this.Diagrams.SingleOrDefault(x => x.Thing == diagram);
                if (row != null)
                {
                    this.Diagrams.Remove(row);
                }
            }

            this.Diagrams.Sort((o1, o2) => o1.Index.CompareTo(o2.Index));
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.CurrentEngineeringModelSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.IterationSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(iterationSetupSubscription);
        }

        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.CurrentEngineeringModelSetup.Name;
            this.CurrentIteration = this.Thing.IterationSetup.IterationNumber;

            var iterationDomainPair = this.Session.OpenIterations.SingleOrDefault(x => x.Key == this.Thing);
            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                this.DomainOfExpertise = "None";
            }
            else
            {
                this.DomainOfExpertise = (iterationDomainPair.Value == null || iterationDomainPair.Value.Item1 == null)
                                        ? "None"
                                        : string.Format("{0} [{1}]", iterationDomainPair.Value.Item1.Name, iterationDomainPair.Value.Item1.ShortName);
            }
        }

        /// <summary>
        /// Execute the <see cref="UpdateCommand"/> on the <see cref="SelectedThing"/>
        /// </summary>
        protected override void ExecuteUpdateCommand()
        {
            if (this.SelectedThing == null)
            {
                return;
            }

            var thing = this.SelectedThing.Thing;
            var vm = new DiagramEditorViewModel((DiagramCanvas)thing, this.Session, this.ThingDialogNavigationService, this.PanelNavigationService, this.DialogNavigationService, this.PluginSettingsService);
            this.PanelNavigationService.Open(vm, true);
        }
    }
}
