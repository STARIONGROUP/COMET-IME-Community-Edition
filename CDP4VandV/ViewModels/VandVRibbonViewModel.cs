// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVRibbonViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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

namespace CDP4VandV.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4VandV.Rdl;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the V&amp;V ribbon controls. Tracks the open <see cref="EngineeringModel"/>s and offers a
    /// "Set up V&amp;V" action per model, which checks and (after confirmation) seeds the V&amp;V reference data.
    /// </summary>
    public class VandVRibbonViewModel : ReactiveObject
    {
        /// <summary>
        /// The <see cref="VandVRdlService"/> shared by the menu items.
        /// </summary>
        private readonly VandVRdlService rdlService = new VandVRdlService();

        /// <summary>
        /// The open <see cref="ISession"/>s.
        /// </summary>
        private readonly List<ISession> sessions = new List<ISession>();

        /// <summary>
        /// Backing field for <see cref="HasModels"/>.
        /// </summary>
        private bool hasModels;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVRibbonViewModel"/> class.
        /// </summary>
        /// <param name="messageBus">The (MEF injected) <see cref="ICDPMessageBus"/>.</param>
        public VandVRibbonViewModel(ICDPMessageBus messageBus)
        {
            this.OpenModels = new ReactiveList<VandVModelMenuItemViewModel>();
            this.OpenModels.CountChanged.Subscribe(count => this.HasModels = count != 0);

            messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            messageBus.Listen<ObjectChangedEvent>(typeof(EngineeringModel))
                .Where(x => x.EventKind == EventKind.Added)
                .Select(x => x.ChangedThing as EngineeringModel)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.EngineeringModelAddedEventHandler);

            messageBus.Listen<ObjectChangedEvent>(typeof(EngineeringModel))
                .Where(x => x.EventKind == EventKind.Removed)
                .Select(x => x.ChangedThing as EngineeringModel)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.EngineeringModelRemovedEventHandler);
        }

        /// <summary>
        /// Gets a value indicating whether there is at least one open <see cref="EngineeringModel"/>.
        /// </summary>
        public bool HasModels
        {
            get => this.hasModels;
            private set => this.RaiseAndSetIfChanged(ref this.hasModels, value);
        }

        /// <summary>
        /// Gets the "Set up V&amp;V" menu items, one per open <see cref="EngineeringModel"/>.
        /// </summary>
        public ReactiveList<VandVModelMenuItemViewModel> OpenModels { get; }

        /// <summary>
        /// Adds a menu item for a newly opened <see cref="EngineeringModel"/>.
        /// </summary>
        /// <param name="engineeringModel">The <see cref="EngineeringModel"/> that was added.</param>
        private void EngineeringModelAddedEventHandler(EngineeringModel engineeringModel)
        {
            var session = this.sessions.SingleOrDefault(s => s.Assembler.Cache == engineeringModel.Cache);

            if (session == null || this.OpenModels.Any(x => x.Model == engineeringModel))
            {
                return;
            }

            this.OpenModels.Add(new VandVModelMenuItemViewModel(engineeringModel, session, this.rdlService));
        }

        /// <summary>
        /// Removes the menu item for a closed <see cref="EngineeringModel"/>.
        /// </summary>
        /// <param name="engineeringModel">The <see cref="EngineeringModel"/> that was removed.</param>
        private void EngineeringModelRemovedEventHandler(EngineeringModel engineeringModel)
        {
            var menuItem = this.OpenModels.SingleOrDefault(x => x.Model == engineeringModel);

            if (menuItem != null)
            {
                this.OpenModels.Remove(menuItem);
            }
        }

        /// <summary>
        /// Tracks open and closed <see cref="ISession"/>s.
        /// </summary>
        /// <param name="sessionChange">The <see cref="SessionEvent"/>.</param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (sessionChange.Status == SessionStatus.Open)
            {
                this.sessions.Add(sessionChange.Session);
            }
            else if (sessionChange.Status == SessionStatus.Closed)
            {
                this.sessions.Remove(sessionChange.Session);

                foreach (var menuItem in this.OpenModels.Where(x => x.Model.Cache == sessionChange.Session.Assembler.Cache).ToList())
                {
                    this.OpenModels.Remove(menuItem);
                }
            }
        }
    }
}
