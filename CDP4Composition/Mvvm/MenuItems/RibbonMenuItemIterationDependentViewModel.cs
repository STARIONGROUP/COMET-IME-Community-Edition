// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonMenuItemIterationDependentViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;    
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Navigation;
    using Navigation.Interfaces;
    using ReactiveUI;

    /// <summary>
    /// Represents the view-model for the menu-item based on the <see cref="Iteration"/>s available which opens a <see cref="IPanelViewModel"/>
    /// </summary>
    public class RibbonMenuItemIterationDependentViewModel : RibbonMenuItemViewModelBase
    {
        /// <summary>
        /// The <see cref="Iteration"/> represented 
        /// </summary>
        public readonly Iteration Iteration;

        /// <summary>
        /// The Function returning an instance of a <see cref="IPanelViewModel"/>
        /// </summary>
        private readonly Func<Iteration, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// The backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="IsFrozen"/>
        /// </summary>
        private bool isFrozen;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonMenuItemIterationDependentViewModel"/> class
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> associated to this menu item view-model
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="instantiatePanelViewModelFunction">The function that creates an instance of the <see cref="IPanelViewModel"/> for this menu-item</param>
        public RibbonMenuItemIterationDependentViewModel(Iteration iteration, ISession session, Func<Iteration, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPanelViewModel> instantiatePanelViewModelFunction)
            : base("Iteration " + iteration.IterationSetup.IterationNumber, session)
        {
            this.Iteration = iteration;
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;

            this.InitializeSubscriptions();

            this.SetProperties();
        }

        /// <summary>
        /// Set the properties
        /// </summary>
        private void SetProperties()
        {
            this.RevisionNumber = this.Iteration.IterationSetup.RevisionNumber;

            var frozenString = string.Empty;
            if (this.Iteration.IterationSetup.FrozenOn != null)
            {
                frozenString = string.Format("{1}Frozen On:{0}", this.Iteration.IterationSetup.FrozenOn,
                    Environment.NewLine);

                this.IsFrozen = true;
            }

            this.Description = string.Format("{0}{1}Created On: {2}{3}", this.Iteration.IterationSetup.Description, Environment.NewLine, this.Iteration.IterationSetup.CreatedOn, frozenString);
        }

        /// <summary>
        /// Initialize the subscriptions.
        /// </summary>
        private void InitializeSubscriptions()
        {
            var thingSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Iteration.IterationSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());
            this.Disposables.Add(thingSubscription);
        }

        /// <summary>
        /// Gets the description of this <see cref="RibbonMenuItemIterationDependentViewModel"/>
        /// </summary>
        public string Description
        {
            get { return this.description; }
            private set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="RibbonMenuItemIterationDependentViewModel"/> is showing
        /// a frozen IterationSetup
        /// </summary>
        public bool IsFrozen
        {
            get { return this.isFrozen; }
            private set { this.RaiseAndSetIfChanged(ref this.isFrozen, value); }
        }

        /// <summary>
        /// Gets or sets the revision number representing the current state of the <see cref="IterationSetup"/>
        /// </summary>
        public int RevisionNumber { get; set; }

        /// <summary>
        /// Returns an instance of <see cref="IPanelViewModel"/> which is <see cref="Iteration"/> dependent
        /// </summary>
        /// <returns>An instance of <see cref="IPanelViewModel"/></returns>
        protected override IPanelViewModel InstantiatePanelViewModel()
        {
            return this.InstantiatePanelViewModelFunction(this.Iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationServive, this.DialogNavigationService);
        }
    }
}