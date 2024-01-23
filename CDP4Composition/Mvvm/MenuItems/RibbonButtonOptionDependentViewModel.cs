// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonButtonOptionDependentViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The base class representing Ribbon button that depends on open <see cref="Iteration"/> to show a <see cref="IPanelView"/>
    /// </summary>
    public abstract class RibbonButtonOptionDependentViewModel : ReactiveObject
    {
        /// <summary>
        /// The Function returning an instance of <see cref="IPanelViewModel"/>
        /// </summary>
        protected readonly Func<Option, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> InstantiatePanelViewModelFunction;

        /// <summary>
        /// Backing field for <see cref="HasModels"/>
        /// </summary>
        private bool hasModels;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonButtonOptionDependentViewModel"/> class. 
        /// </summary>
        /// <param name="instantiatePanelViewModelFunction">
        /// The instantiate Panel View Model Function.
        /// </param>
        protected RibbonButtonOptionDependentViewModel(Func<Option, ISession, IThingDialogNavigationService, IPanelNavigationService, IDialogNavigationService, IPluginSettingsService, IPanelViewModel> instantiatePanelViewModelFunction, ICDPMessageBus messageBus)
        {
            this.InstantiatePanelViewModelFunction = instantiatePanelViewModelFunction;

            this.Sessions = new List<ISession>();

            this.OpenIterations = new ReactiveList<IterationMenuGroupViewModel>();
            this.OpenIterations.CountChanged.Subscribe(x => this.HasModels = x != 0);

            messageBus.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            // react to iterations
            messageBus.Listen<ObjectChangedEvent>(typeof(Iteration))
                .Where(x => x.EventKind == EventKind.Added)
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationAddedEventHandler);

            messageBus.Listen<ObjectChangedEvent>(typeof(Iteration))
                .Where(x => x.EventKind == EventKind.Removed)
                .Select(x => x.ChangedThing as Iteration)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationRemovedEventHandler);

            // react to options
            messageBus.Listen<ObjectChangedEvent>(typeof(Option))
                .Where(x => x.EventKind == EventKind.Added)
                .Select(x => x.ChangedThing as Option)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.OptionAddedEventHandler);

            messageBus.Listen<ObjectChangedEvent>(typeof(Option))
                .Where(x => x.EventKind == EventKind.Removed)
                .Select(x => x.ChangedThing as Option)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.OptionRemovedEventHandler);
        }

        /// <summary>
        /// Gets a value indicating whether there are open sessions
        /// </summary>
        public bool HasModels
        {
            get => this.hasModels;
            set => this.RaiseAndSetIfChanged(ref this.hasModels, value);
        }

        /// <summary>
        /// Gets the list of groups of <see cref="Iteration"/> based on the ones available in the application
        /// </summary>
        public ReactiveList<IterationMenuGroupViewModel> OpenIterations { get; private set; }

        /// <summary>
        /// Gets the List of <see cref="ISession"/> that are opened
        /// </summary>
        public List<ISession> Sessions { get; private set; }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Iteration"/>s added
        /// </summary>
        /// <param name="iteration">the Iteration</param>
        private void IterationAddedEventHandler(Iteration iteration)
        {
            var session = this.Sessions.SingleOrDefault(s => s.Assembler.Cache == iteration.Cache);

            if (session == null)
            {
                throw new InvalidOperationException("There is no ISession associated with an Iteration.");
            }

            var modelGroupViewModel = this.OpenIterations.SingleOrDefault(x => x.Thing == iteration);

            if (modelGroupViewModel == null)
            {
                modelGroupViewModel = new IterationMenuGroupViewModel(iteration, session);
                this.OpenIterations.Add(modelGroupViewModel);
            }

            foreach (var option in iteration.Option)
            {
                var menuItem = new RibbonMenuItemOptionDependentViewModel((Option)option, session, this.InstantiatePanelViewModelFunction);
                modelGroupViewModel.SelectedOptions.Add(menuItem);
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Iteration"/>s removed
        /// </summary>
        /// <param name="iteration">the Iteration</param>
        private void IterationRemovedEventHandler(Iteration iteration)
        {
            // remove one by one the option panels, items and then remove the iteration group itself
            var groupViewModel = this.OpenIterations.SingleOrDefault(x => x.Thing == iteration);

            if (groupViewModel != null)
            {
                var menuItemsToRemove = groupViewModel.SelectedOptions.Where(x => x.Option.Container == iteration).ToList();

                foreach (var item in menuItemsToRemove)
                {
                    groupViewModel.SelectedOptions.Remove(item);
                    ((ICommand)item.ClosePanelsCommand).Execute(default);
                }

                this.OpenIterations.Remove(groupViewModel);
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Option"/>s added
        /// </summary>
        /// <param name="option">the Iteration</param>
        private void OptionAddedEventHandler(Option option)
        {
            var session = this.Sessions.SingleOrDefault(s => s.Assembler.Cache == option.Cache);

            if (session == null)
            {
                throw new InvalidOperationException("There is no ISession associated with an option.");
            }

            var groupViewModel = this.OpenIterations.SingleOrDefault(x => x.Thing == option.Container);

            if (groupViewModel != null)
            {
                // if there is no iteration group that this option depends on do nothing
                // only add if not present yet.
                if (groupViewModel.SelectedOptions.All(x => x.Option != option))
                {
                    var menuItem = new RibbonMenuItemOptionDependentViewModel(option, session, this.InstantiatePanelViewModelFunction);
                    groupViewModel.SelectedOptions.Add(menuItem);
                }
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for <see cref="Option"/>s removed
        /// </summary>
        /// <param name="option">the <see cref="Option"/></param>
        private void OptionRemovedEventHandler(Option option)
        {
            var groupViewModel = this.OpenIterations.SingleOrDefault(x => x.Thing == option.Container);

            if (groupViewModel != null)
            {
                var menuItemToRemove = groupViewModel.SelectedOptions.SingleOrDefault(x => x.Option == option);

                if (menuItemToRemove != null)
                {
                    groupViewModel.SelectedOptions.Remove(menuItemToRemove);
                    ((ICommand)menuItemToRemove.ClosePanelsCommand).Execute(default);
                }

                // removes the group if there are no more of its options opened
                if (groupViewModel.SelectedOptions.Count == 0)
                {
                    groupViewModel.Dispose();
                    this.OpenIterations.Remove(groupViewModel);
                }
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
            switch (sessionChange.Status)
            {
                case SessionStatus.Open:
                    this.Sessions.Add(sessionChange.Session);
                    break;
                case SessionStatus.Closed:
                    this.Sessions.Remove(sessionChange.Session);
                    break;
                default:
                    // do nothing
                    break;
            }
        }
    }
}
