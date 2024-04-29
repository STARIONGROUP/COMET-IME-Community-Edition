// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelMenuGroupViewModel.cs" company="Starion Group S.A.">
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

    using CDP4Composition.Mvvm.MenuItems;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Represents the selected <see cref="EngineeringModel"/> to open based on the selected <see cref="Iteration"/>s
    /// </summary>
    public class EngineeringModelMenuGroupViewModel : MenuGroupViewModelBase<EngineeringModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelMenuGroupViewModel"/> class
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> to add
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public EngineeringModelMenuGroupViewModel(Iteration iteration, ISession session)
            : base(iteration.Container as EngineeringModel, session)
        {
            var engineeringModelSetupSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.EngineeringModelSetup)
                .Where(
                    objectChange =>
                        objectChange.EventKind == EventKind.Updated &&
                        objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());

            this.Disposables.Add(engineeringModelSetupSubscription);

            this.SelectedIterations = new ReactiveList<RibbonMenuItemIterationDependentViewModel>();
        }

        /// <summary>
        /// Derives the name string based on containment
        /// </summary>
        /// <returns>The formatted name of the group.</returns>
        protected override string DeriveName()
        {
            return string.Format("{0} : {1}", this.Session.Name, this.Thing.EngineeringModelSetup.Name);
        }

        /// <summary>
        /// Gets the list of <see cref="RibbonMenuItemIterationDependentViewModel"/> based on the <see cref="Iteration"/>s available
        /// </summary>
        public ReactiveList<RibbonMenuItemIterationDependentViewModel> SelectedIterations { get; private set; }
    }
}
