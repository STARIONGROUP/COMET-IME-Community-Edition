// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="MultiRelationshipRowViewModel"/> row
    /// </summary>
    public class MultiRelationshipRowViewModel : CDP4CommonView.MultiRelationshipRowViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="Name"/> property.
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for the <see cref="Categories"/> property.
        /// </summary>
        private string categories;

        /// <summary>
        /// Backing field for the <see cref="RelatedThings"/> property.
        /// </summary>
        private string relatedThings;

        /// <summary>
        /// Backing field for the <see cref="RelatedThingsKinds"/> property.
        /// </summary>
        private string relatedThingsKinds;

        /// <summary>
        /// Disctionary to map the related things and the related observables to be able to dispose them
        /// </summary>
        private Dictionary<Thing, IDisposable> oldRelatedThingSubcriptions = new Dictionary<Thing, IDisposable>();

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiRelationshipRowViewModel"/> class
        /// </summary>
        /// <param name="relationship">The <see cref="MultiRelationship"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase<Thing>"/></param> container
        public MultiRelationshipRowViewModel(MultiRelationship relationship, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(relationship, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            // I look for the new and old elements
            var oldRelatedThings = this.oldRelatedThingSubcriptions.Keys.ToList();
            var newElements = this.Thing.RelatedThing.Except(oldRelatedThings);
            var oldElements = oldRelatedThings.Except(this.Thing.RelatedThing);

            //I remove old elements
            foreach (var element in oldElements)
            {
                this.Disposables.Remove(this.oldRelatedThingSubcriptions[element]);
                this.oldRelatedThingSubcriptions[element].Dispose();
                this.oldRelatedThingSubcriptions.Remove(element);
            }

            //In case there are new elements I create new name subcriptions and add them
            foreach (var element in newElements)
            {
                var elementSubscription = this.CDPMessageBus.Listen<ObjectChangedEvent>(element)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateRelatedThings());

                this.oldRelatedThingSubcriptions.Add(element, elementSubscription);
                this.Disposables.Add(elementSubscription);
            }

            this.Categories = string.Join(" ", this.Thing.Category.Select(x => x.ShortName));

            this.UpdateRelatedThings();
        }

        /// <summary>
        /// Updates the related-things columns and the name. Called both when the set of related things changes and
        /// when an individual related <see cref="Thing"/> is updated (e.g. its name changes).
        /// </summary>
        protected void UpdateRelatedThings()
        {
            this.RelatedThings = string.Join(", ", this.Thing.RelatedThing.Select(this.GetThingName));
            this.RelatedThingsKinds = string.Join(", ", this.Thing.RelatedThing.Select(thing => thing.ClassKind.ToString()));

            this.UpdateName();
        }

        /// <summary>
        /// Update the relationship name. When the <see cref="MultiRelationship"/> is named the name is shown; otherwise
        /// the related items are used as a fallback (the related items are also shown in the Related Things column).
        /// </summary>
        protected void UpdateName()
        {
            this.Name = string.IsNullOrWhiteSpace(this.Thing.Name)
                ? this.RelatedThings
                : this.Thing.Name;
        }

        /// <summary>
        /// Gets the visual name of the related <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">The related <see cref="Thing"/></param>
        /// <returns>The name of the <see cref="Thing"/>, or its <see cref="ClassKind"/> when it is not an <see cref="INamedThing"/></returns>
        private string GetThingName(Thing thing)
        {
            return thing is INamedThing namedThing ? namedThing.Name : thing.ClassKind.ToString();
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="MultiRelationship"/> that is represented by the current row-view-model
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the display names of the related <see cref="Thing"/>s of the <see cref="MultiRelationship"/>
        /// </summary>
        public string RelatedThings
        {
            get => this.relatedThings;
            set => this.RaiseAndSetIfChanged(ref this.relatedThings, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ClassKind"/>s of the related <see cref="Thing"/>s as a display string
        /// </summary>
        public string RelatedThingsKinds
        {
            get => this.relatedThingsKinds;
            set => this.RaiseAndSetIfChanged(ref this.relatedThingsKinds, value);
        }

        /// <summary>
        /// Gets or sets the short names of the <see cref="Category"/> instances that are directly applied to the <see cref="MultiRelationship"/>
        /// </summary>
        public string Categories
        {
            get => this.categories;
            set => this.RaiseAndSetIfChanged(ref this.categories, value);
        }
    }
}
