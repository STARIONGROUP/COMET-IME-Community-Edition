﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterOptionRowViewModel.cs" company="Starion Group S.A.">
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
    /// The row representing an <see cref="Option"/> of a <see cref="ParameterBase"/>
    /// </summary>
    public class ParameterOptionRowViewModel : ParameterValueBaseRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOptionRowViewModel"/> class
        /// </summary>
        /// <param name="parameterBase">The associated <see cref="ParameterBase"/></param>
        /// <param name="option">The associated <see cref="Option"/></param>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container row</param>
        /// <param name="isReadOnly">A value indicating whether the row is read-only</param>
        public ParameterOptionRowViewModel(ParameterBase parameterBase, Option option, ISession session, IViewModelBase<Thing> containerViewModel, bool isReadOnly)
            : base(parameterBase, session, option, null, containerViewModel, 0, isReadOnly)
        {
            this.Name = this.ActualOption.Name;
            this.Option = this.ActualOption;

            Func<ObjectChangedEvent, bool> discriminator = objectChange => objectChange.EventKind == EventKind.Updated;
            Action<ObjectChangedEvent> action = x => { this.Name = this.ActualOption.Name; };

            if (this.AllowMessageBusSubscriptions)
            {
                var optionListener = this.CDPMessageBus.Listen<ObjectChangedEvent>(this.Option)
                    .Where(discriminator)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(action);

                this.Disposables.Add(optionListener);
            }
            else
            {
                var optionObserver = this.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(Option));
                this.Disposables.Add(this.MessageBusHandler.GetHandler<ObjectChangedEvent>().RegisterEventHandler(optionObserver, new ObjectChangedMessageBusEventHandlerSubscription(this.Option, discriminator, action)));
            }
        }

        /// <summary>
        /// Setting values for this <see cref="ParameterOptionRowViewModel"/>
        /// </summary>
        public override void SetValues()
        {
            base.SetValues();

            if (this.Thing is ParameterSubscription)
            {
                this.Published = this.Computed;
            }
        }
    }
}
