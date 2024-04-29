// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedThingViewModel.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The view-model representing a related thing for a <see cref="BinaryRelationship" />
    /// </summary>
    public class RelatedThingViewModel : ReactiveObject, IDropTarget, IDisposable
    {
        /// <summary>
        /// Backing field for <see cref="RelatedThing" />
        /// </summary>
        private Thing relatedThing;

        /// <summary>
        /// Backing field for <see cref="RelatedThingDenomination" />
        /// </summary>
        private string relatedThingDenomination;

        /// <summary>
        /// The collection of <see cref="IDisposable" />
        /// </summary>
        private IDisposable subscription;

        /// <summary name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </summary>
        private readonly ICDPMessageBus messageBus;

        /// <summary>
        /// Creates anew instance of the <see cref="RelatedThingViewModel"/> class
        /// </summary>
        /// <param name="messageBus">
        /// The <see cref="ICDPMessageBus"/>
        /// </param>
        public RelatedThingViewModel(ICDPMessageBus messageBus)
        {
            this.messageBus = messageBus;
        }

        /// <summary>
        /// Gets the source <see cref="Thing" /> for the <see cref="BinaryRelationship" /> to create
        /// </summary>
        public Thing RelatedThing
        {
            get => this.relatedThing;
            private set => this.RaiseAndSetIfChanged(ref this.relatedThing, value);
        }

        /// <summary>
        /// Gets the string to display associated with the <see cref="RelatedThing" />
        /// </summary>
        public string RelatedThingDenomination
        {
            get => this.relatedThingDenomination;
            set => this.RaiseAndSetIfChanged(ref this.relatedThingDenomination, value);
        }

        /// <summary>
        /// dispose of this view-model
        /// </summary>
        public void Dispose()
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
            }
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects" /> property on
        /// <paramref name="dropInfo" /> should be set to a value other than <see cref="DragDropEffects.None" />
        /// and <see cref="DropInfo.Data" /> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            var thing = dropInfo.Payload as Thing;

            if (thing != null && thing.TopContainer is EngineeringModel)
            {
                dropInfo.Effects = DragDropEffects.All;
            }
        }

        /// <summary>
        /// Performs a drop.
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var thing = dropInfo.Payload as Thing;

            if (thing == null)
            {
                this.RelatedThingDenomination = "";
                return;
            }

            this.Dispose();

            this.RelatedThing = thing;
            this.RelatedThingDenomination = string.Format("({0}) {1}", thing.ClassKind, thing.UserFriendlyName);

            this.subscription = this.messageBus.Listen<ObjectChangedEvent>(this.RelatedThing)
                .Where(msg => msg.EventKind == EventKind.Updated)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => this.RelatedThingDenomination = string.Format("({0}) {1}", thing.ClassKind, thing.UserFriendlyName));
        }

        /// <summary>
        /// Reset the control
        /// </summary>
        public void ResetControl()
        {
            this.RelatedThing = null;
            this.RelatedThingDenomination = "";
        }
    }
}
