// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterStateRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The row representing an <see cref="ActualFiniteState"/>
    /// </summary>
    public class ParameterStateRowViewModel : ParameterValueBaseRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterStateRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">The associated <see cref="ParameterBase"/></param>
        /// <param name="option">The associated <see cref="Option"/></param>
        /// <param name="actualState">The associated <see cref="ActualFiniteState"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container row</param>
        /// <param name="isReadOnly">A value indicating whether the row is read-only</param>
        public ParameterStateRowViewModel(ParameterBase parameterBase, Option option, ActualFiniteState actualState, ISession session, IRowViewModelBase<Thing> containerViewModel, bool isReadOnly)
            : base(parameterBase, session, option, actualState, containerViewModel, 0, isReadOnly)
        {
            this.Name = this.ActualState.Name;
            this.State = this.ActualState.Name;
            this.IsDefault = this.ActualState.IsDefault;
            this.Option = this.ActualOption;

            this.InitializeOptionSubscriptions();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ActualFiniteState"/> associated with this row is the default value of the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public bool IsDefault
        {
            get => this.isDefault;
            set => this.RaiseAndSetIfChanged(ref this.isDefault, value);
        }

        /// <summary>
        /// Initializes the <see cref="Option"/> related subscriptions
        /// </summary>
        private void InitializeOptionSubscriptions()
        {
            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;

            Action<ObjectChangedEvent> action = x =>
            {
                this.Name = this.ActualState.Name;
                this.IsDefault = this.ActualState.IsDefault;
                this.UpdateModelCode();
            };

            if (this.AllowMessageBusSubscriptions)
            {
                foreach (var possibleFiniteState in this.ActualState.PossibleState)
                {
                    var stateListener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(possibleFiniteState)
                        .Where(discriminator)
                        .ObserveOn(RxApp.MainThreadScheduler)
                        .Subscribe(action);

                    this.Disposables.Add(stateListener);
                }
            }
            else
            {
                var possibleFiniteStateObserver = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(PossibleFiniteState));

                foreach (var possibleFiniteState in this.ActualState.PossibleState)
                {
                    this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(possibleFiniteStateObserver, new ObjectChangedMessageBusEventHandlerSubscription(possibleFiniteState, discriminator, action)));
                }
            }
        }
    }
}
