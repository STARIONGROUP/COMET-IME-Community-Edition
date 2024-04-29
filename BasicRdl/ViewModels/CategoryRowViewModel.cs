// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryRowViewModel.cs" company="Starion Group S.A.">
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

namespace BasicRdl.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Represents a row of the <see cref="Views.CategoryBrowser"/> grid
    /// </summary>
    public class CategoryRowViewModel : CDP4CommonView.CategoryRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="ContainerRdl"/>
        /// </summary>
        private string containerRdl;

        /// <summary>
        /// Backing field for <see cref="SuperCategories"/>
        /// </summary>
        private string superCategories;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryRowViewModel"/> class
        /// </summary>
        /// <param name="category">The <see cref="Category"/> represented</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public CategoryRowViewModel(Category category, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(category, session, containerViewModel)
        {
            var containerSubscription = session.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing.Container)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.ObjectChangeEventHandler);

            this.Disposables.Add(containerSubscription);

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the Name of the <see cref="ReferenceDataLibrary"/> container
        /// </summary>
        public string ContainerRdl
        {
            get => this.containerRdl;
            set => this.RaiseAndSetIfChanged(ref this.containerRdl, value);
        }

        /// <summary>
        /// Gets or sets the Names of the super-<see cref="Category"/>s 
        /// </summary>
        public string SuperCategories
        {
            get => this.superCategories;
            set => this.RaiseAndSetIfChanged(ref this.superCategories, value);
        }

        /// <summary>
        /// Update the properties when the <see cref="Category"/> represented by this view-model is updated
        /// </summary>
        private void UpdateProperties()
        {
            var container = this.Thing.Container as ReferenceDataLibrary;
            this.ContainerRdl = container != null ? container.ShortName : string.Empty;
            this.SuperCategories = this.Thing.SuperCategory.Count == 0 ? string.Empty : "{" + string.Join(", ", this.Thing.SuperCategory.Select(x => x.ShortName)) + "}";
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects"/> property on <paramref name="dragInfo"/> 
        /// should be set to a value other than <see cref="DragDropEffects.None"/>. 
        /// </remarks>
        public override void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Payload = this.Thing;
            dragInfo.Effects = DragDropEffects.Copy;
        }
    }
}
