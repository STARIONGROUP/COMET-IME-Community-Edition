// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonMenuItemOptionDependentViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition;    
    using CDP4Dal;
    using CDP4Dal.Events;
    using Navigation;
    using Navigation.Interfaces;
    using PluginSettingService;
    using ReactiveUI;

    /// <summary>
    /// Represents the view-model for the menu-item based on the <see cref="Option"/>s available which opens a <see cref="IPanelViewModel"/>
    /// </summary>
    public class RibbonMenuItemOptionDependentViewModel : RibbonMenuItemViewModelBase
    {
        /// <summary>
        /// The <see cref="Option"/> represented 
        /// </summary>
        public readonly Option Option;

        /// <summary>
        /// The Function returning an instance of a <see cref="IPanelViewModel"/>
        /// </summary>
        private readonly Func<Option, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonMenuItemOptionDependentViewModel"/> class
        /// </summary>
        /// <param name="option">
        /// The <see cref="Option"/> associated to this menu item view-model
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="instantiatePanelViewModelFunction">The function that creates an instance of the <see cref="IPanelViewModel"/> for this menu-item</param>
        public RibbonMenuItemOptionDependentViewModel(Option option, ISession session, Func<Option, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModelFunction)
            : base(option.Name, session)
        {
            this.Option = option;
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;

            this.InitializeSubscriptions();

            this.SetProperties();
        }

        /// <summary>
        /// Set the properties
        /// </summary>
        private void SetProperties()
        {
            this.RevisionNumber = this.Option.RevisionNumber;
            this.MenuItemContent = this.Option.Name;
            this.Description = string.Format("{1} > Iteration {0}", ((Iteration)this.Option.Container).IterationSetup.IterationNumber, ((EngineeringModel)this.Option.Container.Container).EngineeringModelSetup.Name);
        }

        /// <summary>
        /// Initialize the subscriptions.
        /// </summary>
        private void InitializeSubscriptions()
        {
            var engineeringModelSetupSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(
                ((EngineeringModel)this.Option.Container.Container).EngineeringModelSetup)
                .Where(
                    objectChange =>
                        objectChange.EventKind == EventKind.Updated &&
                        objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());
            this.Disposables.Add(engineeringModelSetupSubscription);

            var thingSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Option)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());
            this.Disposables.Add(thingSubscription);
        }

        /// <summary>
        /// Gets the description of this <see cref="RibbonMenuItemOptionDependentViewModel"/>
        /// </summary>
        public string Description
        {
            get { return this.description; }
            private set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }
        
        /// <summary>
        /// Gets or sets the revision number representing the current state of the IterationSetup
        /// </summary>
        public int RevisionNumber { get; set; }

        /// <summary>
        /// Returns an instance of <see cref="IPanelViewModel"/> which is <see cref="Iteration"/> dependent
        /// </summary>
        /// <returns>An instance of <see cref="IPanelViewModel"/></returns>
        protected override IPanelViewModel InstantiatePanelViewModel()
        {
            return this.InstantiatePanelViewModelFunction(this.Option, this.Session, this.ThingDialogNavigationService, this.PanelNavigationServive, this.DialogNavigationService, this.PluginSettingsService);
        }
    }
}