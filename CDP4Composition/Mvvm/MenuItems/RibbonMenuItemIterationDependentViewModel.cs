﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonMenuItemIterationDependentViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Reactive.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

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
        private readonly Func<Iteration, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> InstantiatePanelViewModelFunction;

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
        public RibbonMenuItemIterationDependentViewModel(Iteration iteration, ISession session, Func<Iteration, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModelFunction)
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
                frozenString = string.Format(
                    "{1}Frozen On:{0}",
                    this.Iteration.IterationSetup.FrozenOn,
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
            var thingSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(this.Iteration.IterationSetup)
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
            get => this.description;
            private set => this.RaiseAndSetIfChanged(ref this.description, value);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="RibbonMenuItemIterationDependentViewModel"/> is showing
        /// a frozen IterationSetup
        /// </summary>
        public bool IsFrozen
        {
            get => this.isFrozen;
            private set => this.RaiseAndSetIfChanged(ref this.isFrozen, value);
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
            return this.InstantiatePanelViewModelFunction(this.Iteration, this.Session, this.ThingDialogNavigationService, this.PanelNavigationServive, this.DialogNavigationService, this.PluginSettingsService);
        }
    }
}
