// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedThingRowViewModel.cs" company="Starion Group S.A.">
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
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The row-view-model used to represent related things for a <see cref="MultiRelationship"/> in the <see cref="MultiRelationshipCreatorViewModel"/> 
    /// </summary>
    public class RelatedThingRowViewModel : ReactiveObject, IDisposable
    {
        /// <summary>
        /// Backing field for <see cref="Denomination"/>
        /// </summary>
        private string denomination;

        /// <summary>
        /// Backing field for <see cref="ClassKind"/>
        /// </summary>
        private string classKind;

        /// <summary>
        /// The collection of <see cref="IDisposable"/>
        /// </summary>
        private readonly List<IDisposable> Subscriptions = new List<IDisposable>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RelatedThingRowViewModel"/> class
        /// </summary>
        /// <param name="thing">The associated <see cref="Thing"/></param>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        /// <param name="callBack">The <see cref="Action"/> to call back</param>
        public RelatedThingRowViewModel(Thing thing, ICDPMessageBus messageBus, Action<RelatedThingRowViewModel> callBack)
        {
            this.Thing = thing;

            var subscriber = messageBus.Listen<ObjectChangedEvent>(this.Thing)
                .Where(msg => msg.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.SetProperties());

            this.Subscriptions.Add(subscriber);

            this.RemoveRelatedThingCommand = ReactiveCommandCreator.Create(() => callBack(this));

            this.SetProperties();
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> represented by the view-model
        /// </summary>
        public Thing Thing { get; private set; }

        /// <summary>
        /// Gets the command to remove the current represented <see cref="Thing"/> from the related things
        /// </summary>
        public ReactiveCommand<Unit, Unit> RemoveRelatedThingCommand { get; private set; }

        /// <summary>
        /// Gets the human-readable string that represents the current <see cref="Thing"/>
        /// </summary>
        public string Denomination
        {
            get => this.denomination;
            set => this.RaiseAndSetIfChanged(ref this.denomination, value);
        }

        /// <summary>
        /// Gets the class-kind of the current <see cref="Thing"/>
        /// </summary>
        public string ClassKind
        {
            get => this.classKind;
            set => this.RaiseAndSetIfChanged(ref this.classKind, value);
        }

        /// <summary>
        /// Disposes of this view-model
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in this.Subscriptions)
            {
                subscription.Dispose();
            }
        }

        /// <summary>
        /// Set the properties of the current view-model
        /// </summary>
        private void SetProperties()
        {
            this.Denomination = this.Thing.UserFriendlyName;
            this.ClassKind = this.Thing.ClassKind.ToString();
        }
    }
}
