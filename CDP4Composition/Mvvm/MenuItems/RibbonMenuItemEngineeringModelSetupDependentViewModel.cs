// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonMenuItemEngineeringModelSetupDependentViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;    
    using CDP4Dal;
    using CDP4Dal.Events;
    using Navigation;
    using Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using ReactiveUI;

    /// <summary>
    /// Represents the view-model for the menu-item based on the <see cref="EngineeringModelSetup"/>s available which opens a <see cref="IPanelViewModel"/>
    /// </summary>
    public class RibbonMenuItemEngineeringModelSetupDependentViewModel : RibbonMenuItemViewModelBase
    {
        /// <summary>
        /// The <see cref="EngineeringModelSetup"/> represented 
        /// </summary>
        public readonly EngineeringModelSetup EngineeringModelSetup;

        /// <summary>
        /// The Function returning an instance of a <see cref="IPanelViewModel"/>
        /// </summary>
        private readonly Func<EngineeringModelSetup, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonMenuItemEngineeringModelSetupDependentViewModel"/> class
        /// </summary>
        /// <param name="engineeringModelSetup">
        /// The <see cref="EngineeringModelSetup"/> associated to this menu item view-model
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="instantiatePanelViewModelFunction">The function that creates an instance of the <see cref="IPanelViewModel"/> for this menu-item</param>
        public RibbonMenuItemEngineeringModelSetupDependentViewModel(EngineeringModelSetup engineeringModelSetup, ISession session, Func<EngineeringModelSetup, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModelFunction)
            : base(engineeringModelSetup.Name, session)
        {
            this.EngineeringModelSetup = engineeringModelSetup;

            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;

            this.InitializeSubscriptions();

            this.SetProperties();
        }

        /// <summary>
        /// Set the properties
        /// </summary>
        private void SetProperties()
        {
            this.RevisionNumber = this.EngineeringModelSetup.RevisionNumber;
        }

        /// <summary>
        /// Initialize the subscriptions.
        /// </summary>
        private void InitializeSubscriptions()
        {
            var thingSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.EngineeringModelSetup)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());
            this.Disposables.Add(thingSubscription);
        }

        /// <summary>
        /// Gets or sets the revision number representing the current state of the <see cref="EngineeringModelSetupSetup"/>
        /// </summary>
        public int RevisionNumber { get; set; }

        /// <summary>
        /// Returns an instance of <see cref="IPanelViewModel"/> which is <see cref="EngineeringModelSetup"/> dependent
        /// </summary>
        /// <returns>An instance of <see cref="IPanelViewModel"/></returns>
        protected override IPanelViewModel InstantiatePanelViewModel()
        {
            return this.InstantiatePanelViewModelFunction(this.EngineeringModelSetup, this.Session, this.ThingDialogNavigationService, this.PanelNavigationServive, this.DialogNavigationService, this.PluginSettingsService);
        }
    }
}