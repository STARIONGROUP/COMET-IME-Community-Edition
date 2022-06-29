// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// The view-model for the <see cref="BinaryRelationshipRowViewModel"/> row
    /// </summary>
    public class BinaryRelationshipRowViewModel : CDP4CommonView.BinaryRelationshipRowViewModel
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
        /// Source thing before any update
        /// </summary>
        private Thing oldSource;

        /// <summary>
        /// Source thing subscription
        /// </summary>
        private IDisposable sourceSubscription;

        /// <summary>
        /// Target thing before any update
        /// </summary>
        private Thing oldTarget;

        /// <summary>
        /// Target thing subscription
        /// </summary>
        private IDisposable targetSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRelationshipRowViewModel"/> class
        /// </summary>
        /// <param name="relationship">The <see cref="BinaryRelationship"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="permissionService">The <see cref="IPermissionService"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        public BinaryRelationshipRowViewModel(BinaryRelationship relationship, ISession session, IViewModelBase<Thing> containerViewModel)
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
            //In case Source or Target have been updated I dispose and create a new name sunbscription

            if (this.oldSource != this.Thing.Source)
            {
                if (this.sourceSubscription != null)
                {
                    this.Disposables.Remove(this.sourceSubscription);
                    this.sourceSubscription.Dispose();
                }

                this.oldSource = this.Thing.Source;

                this.sourceSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Source)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateName());

                this.Disposables.Add(this.sourceSubscription);
            }

            if (this.oldTarget != this.Thing.Target)
            {
                if (this.targetSubscription != null)
                {
                    this.Disposables.Remove(this.targetSubscription);
                    this.targetSubscription.Dispose();
                }

                this.oldTarget = this.Thing.Target;

                this.targetSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing.Target)
                    .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.UpdateName());

                this.Disposables.Add(this.targetSubscription);
            }

            this.UpdateName();
            this.UpdateCategories();
        }

        /// <summary>
        /// Updates the relationship name
        /// </summary>
        protected void UpdateName()
        {
            if (string.IsNullOrWhiteSpace(this.Thing.Name))
            {
                var source = this.FormatName(this.Source);
                var target = this.FormatName(this.Target);
                this.Name =$"{source} → {target}";
            }
            else
            {
                this.Name = this.Thing.Name;
            }
        }

        /// <summary>
        /// Updates the relationship categories
        /// </summary>
        protected void UpdateCategories()
        {
            this.Categories = string.Join(", ", this.Thing.Category.Select(c => c.Name));
        }

        /// <summary>
        /// Format the name string for display which will include the shortname if available.
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string FormatName(Thing thing)
        {
            var thingName = this.GetThingName(thing);

            var thingShortName = thing is IShortNamedThing shortNamedThing
                ? $" ({shortNamedThing.ShortName})"
                : string.Empty;

            return $"{thingName}{thingShortName}";
        }

        /// <summary>
        /// Get the visual Name of the <see cref="Thing"/>
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/></param>
        /// <returns>Name of the <see cref="Thing"/></returns>
        private string GetThingName(Thing thing)
        {
            if (thing is BooleanExpression booleanExpression)
            {
                return booleanExpression.StringValue;
            }

            return thing is INamedThing namedThing
                ? namedThing.Name
                : thing.GetType().GetProperty(nameof(CDP4Common.CommonData.Thing.UserFriendlyName))?.DeclaringType == typeof(Thing)
                    ? thing.ClassKind.ToString()
                    : thing.UserFriendlyName;
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="BinaryRelationship"/> that is represented by the current row-view-model
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Gets or sets the categories of the <see cref="BinaryRelationship"/> that is represented by the current row-view-model
        /// </summary>
        public string Categories
        {
            get => this.categories;
            set => this.RaiseAndSetIfChanged(ref this.categories, value);
        }
    }
}
