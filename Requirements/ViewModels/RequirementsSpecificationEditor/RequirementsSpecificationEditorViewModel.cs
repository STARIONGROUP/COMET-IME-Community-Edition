// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationEditorViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Requirements.ViewModels.RequirementsSpecificationEditor;
    using ReactiveUI;

    /// <summary>
    /// The <see cref="RequirementsSpecificationEditorViewModel"/> is the view-model of the <see cref="RequirementsSpecificationEditorViewModel"/>
    /// </summary>
    public class RequirementsSpecificationEditorViewModel : BrowserViewModelBase<RequirementsSpecification>, IPanelViewModel
    {
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
        /// The <see cref="EngineeringModelSetup"/> that is referenced by the <see cref="EngineeringModel"/> that contains the current <see cref="Option"/>
        /// </summary>
        private readonly EngineeringModelSetup modelSetup;

        /// <summary>
        /// The container <see cref="iterationSetup"/> that is referenced by the container <see cref="Iteration"/> of the current <see cref="Option"/>.
        /// </summary>
        private readonly IterationSetup iterationSetup;

        /// <summary>
        /// The Panel Caption
        /// </summary>
        private const string PanelCaption = "Requirements Specification Editor";

        /// <summary>
        /// The <see cref="IComparer{T}"/>
        /// </summary>
        protected static readonly IComparer<IRowViewModelBase<Thing>> ContainedRowsComparer = new RequirementSpecificationContentComparer();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationEditorViewModel"/> class
        /// </summary>
        /// <param name="thing">The <see cref="RequirementsSpecification"/> that is represented by the current view-model</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        public RequirementsSpecificationEditorViewModel(RequirementsSpecification thing, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(thing, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.Caption = string.Format("{0}: {1}", PanelCaption, this.Thing.ShortName);
            this.ToolTip = string.Format("{0}\n{1}\n{2}", this.Thing.Name, this.Thing.IDalUri, this.Session.ActivePerson.Name);

            var model = (EngineeringModel)this.Thing.TopContainer;
            this.modelSetup = model.EngineeringModelSetup;

            var iteration = (Iteration)thing.Container;
            this.iterationSetup = iteration.IterationSetup;
            
            this.ContainedRows = new ReactiveList<IRowViewModelBase<Thing>>();

            this.AddRequirementsSpecificationRow(thing);
            this.UpdateRequirementGroupRows();
            this.UpdateRequirementRows();

            this.AddSubscriptions();
            this.UpdateProperties();
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
        /// Gets the <see cref="DomainOfExpertise"/> of the active user
        /// </summary>
        public string DomainOfExpertise
        {
            get { return this.domainOfExpertise; }
            private set { this.RaiseAndSetIfChanged(ref this.domainOfExpertise, value); }
        }

        /// <summary>
        /// Gets or sets the Contained <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        public ReactiveList<IRowViewModelBase<Thing>> ContainedRows { get; protected set; }
        
        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.modelSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(engineeringModelSetupSubscription);

            var domainOfExpertiseSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(DomainOfExpertise))
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(domainOfExpertiseSubscription);

            var iterationSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.iterationSetup)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateProperties());
            this.Disposables.Add(iterationSetupSubscription);
            
            var requirementsGroupAddSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(RequirementsGroup))
                .Where(objectChangedEvent => objectChangedEvent.EventKind == EventKind.Added && objectChangedEvent.ChangedThing.Cache == this.Session.Assembler.Cache && objectChangedEvent.ChangedThing.GetContainerOfType(typeof(RequirementsSpecification)) == this.Thing)
                .Select(objectChangedEvent => objectChangedEvent.ChangedThing)
                .OfType<RequirementsGroup>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.AddRequirementsGroupRow);
            this.Disposables.Add(requirementsGroupAddSubscription);

            var requirementsGroupRemoveSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(RequirementsGroup))
                .Where(objectChangedEvent => objectChangedEvent.EventKind == EventKind.Removed && objectChangedEvent.ChangedThing.Cache == this.Session.Assembler.Cache && objectChangedEvent.ChangedThing.GetContainerOfType(typeof(RequirementsSpecification)) == this.Thing)
                .Select(objectChangedEvent => objectChangedEvent.ChangedThing)
                .OfType<RequirementsGroup>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.RemoveRequirementsGroupRow);
            this.Disposables.Add(requirementsGroupRemoveSubscription);
        }
        
        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateRequirementGroupRows();
            this.UpdateRequirementRows();
            this.UpdateProperties();
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

            foreach (var rowViewModelBase in this.ContainedRows)
            {
                rowViewModelBase.Dispose();
            }
        }

        /// <summary>
        /// Adds a <see cref="RequirementsSpecificationRowViewModel"/> to the <see cref="ContainedRows"/>
        /// </summary>
        /// <param name="requirementsSpecification"></param>
        private void AddRequirementsSpecificationRow(RequirementsSpecification requirementsSpecification)
        {
            var row = new CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementsSpecificationRowViewModel(requirementsSpecification, this.Session, this);
            this.ContainedRows.SortedInsert(row, ContainedRowsComparer);
        }
        
        /// <summary>
        /// Update the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            this.CurrentModel = this.modelSetup.Name;
            this.CurrentIteration = this.iterationSetup.IterationNumber;
            
            var iterationDomainPair = this.Session.OpenIterations.SingleOrDefault(x => x.Key == this.Thing.Container);
            if (iterationDomainPair.Equals(default(KeyValuePair<Iteration, Tuple<DomainOfExpertise, Participant>>)))
            {
                this.DomainOfExpertise = "None";
            }
            else
            {
                this.DomainOfExpertise = (iterationDomainPair.Value.Item1 == null)
                                        ? "None"
                                        : string.Format("{0} [{1}]", iterationDomainPair.Value.Item1.Name, iterationDomainPair.Value.Item1.ShortName);
            }
        }

        /// <summary>
        /// Update the current <see cref="RequirementsContainer"/> with its current <see cref="RequirementsGroup"/>s
        /// </summary>
        protected void UpdateRequirementGroupRows()
        {
            var current = this.ContainedRows.Select(x => x.Thing).OfType<RequirementsGroup>().ToList();
            var updated = this.Thing.GetAllContainedGroups().ToArray();

            var added = updated.Except(current).ToList();
            var removed = current.Except(updated).ToList();

            foreach (var requirementsGroup in added)
            {
                this.AddRequirementsGroupRow(requirementsGroup);
            }

            foreach (var requirementsGroup in removed)
            {
                this.RemoveRequirementsGroupRow(requirementsGroup);
            }
        }

        /// <summary>
        /// Update the current <see cref="RequirementsContainer"/> with its current <see cref="RequirementsGroup"/>s
        /// </summary>
        protected void UpdateRequirementRows()
        {
            var current = this.ContainedRows.Select(x => x.Thing).OfType<Requirement>().ToList();
            var updated = this.Thing.Requirement;

            var added = updated.Except(current).ToList();
            var removed = current.Except(updated).ToList();

            foreach (var requirement in added)
            {
                this.AddRequirementsRow(requirement);
            }

            foreach (var requirement in removed)
            {
                this.RemoveRequirementRow(requirement);
            }
        }

        /// <summary>
        /// Adds a <see cref="RequirementsGroup"/> to the <see cref="ContainedRows"/>
        /// </summary>
        /// <param name="requirementsGroup">
        /// The <see cref="RequirementsGroup"/> that is to be added
        /// </param>
        private void AddRequirementsGroupRow(RequirementsGroup requirementsGroup)
        {
            var row = new CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementsGroupRowViewModel(requirementsGroup, this.Session, this);
            this.ContainedRows.SortedInsert(row, ContainedRowsComparer);
        }

        /// <summary>
        /// Removes a <see cref="RequirementsGroup"/> from the <see cref="ContainedRows"/>
        /// </summary>
        /// <param name="requirementsGroup">
        /// The <see cref="RequirementsGroup"/> that is to be removed
        /// </param>
        private void RemoveRequirementsGroupRow(RequirementsGroup requirementsGroup)
        {
            var row = this.ContainedRows.SingleOrDefault(x => x.Thing == requirementsGroup);
            if (row != null)
            {
                this.ContainedRows.Remove(row);
                row.Dispose();
            }
        }

        /// <summary>
        /// Adds a <see cref="Requirement"/> to the <see cref="ContainedRows"/>
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement"/> that is to be added
        /// </param>
        private void AddRequirementsRow(Requirement requirement)
        {
            var row = new CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementRowViewModel(requirement, this.Session, this);
            this.ContainedRows.SortedInsert(row, ContainedRowsComparer);
        }

        /// <summary>
        /// Removes a <see cref="Requirement"/> from the <see cref="ContainedRows"/>
        /// </summary>
        /// <param name="requirement">
        /// The <see cref="Requirement"/> that is to be removed
        /// </param>
        private void RemoveRequirementRow(Requirement requirement)
        {
            var row = this.ContainedRows.SingleOrDefault(x => x.Thing == requirement);
            if (row != null)
            {
                this.ContainedRows.Remove(row);
                row.Dispose();
            }
        }
    }
}