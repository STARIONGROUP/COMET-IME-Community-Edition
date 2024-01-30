// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4ObjectBrowser
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Mvvm.Types;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// A row-view-model that represents a <see cref="Session"/> 
    /// </summary>
    public class SessionRowViewModel : RowViewModelBase<SiteDirectory>
    {
        /// <summary>
        /// Backing field for the <see cref="Uri"/> property
        /// </summary>
        private string uri;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionRowViewModel"/> class.
        /// </summary>
        /// <param name="siteDirectory">
        /// The <see cref="SiteDirectory"/> that is represented by the row-view-model
        /// </param>
        /// <param name="session">
        /// The <see cref="CDP4Dal.Session"/> that is represented by the row-view-model
        /// </param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public SessionRowViewModel(SiteDirectory siteDirectory, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(siteDirectory, session, containerViewModel)
        {
            var engineeringModelAdded = session.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(EngineeringModel))
                .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(objectChange => objectChange.ChangedThing as EngineeringModel)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(this.AddEngineeringModel);

            this.Disposables.Add(engineeringModelAdded);

            var engineeringModelRemoved = session.CDPMessageBus.Listen<ObjectChangedEvent>(typeof(EngineeringModel))
                .Where(objectChange => objectChange.EventKind == EventKind.Removed)
                .Select(objectChange => objectChange.ChangedThing as EngineeringModel)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(this.RemoveEngineeringModel);

            this.Disposables.Add(engineeringModelRemoved);

            this.SiteDirectoryRowViewModel = new SiteDirectoryRowViewModel(siteDirectory, this.Session, this);
            this.ContainedRows.Add(this.SiteDirectoryRowViewModel);

            this.EngineeringModelRowViewModels = new DisposableReactiveList<EngineeringModelRowViewModel>();

            this.Uri = session.DataSourceUri;
        }

        /// <summary>
        /// Gets or sets the Uri of the <see cref="Session"/> current row-view-model
        /// </summary>
        public string Uri
        {
            get => this.uri;

            set => this.RaiseAndSetIfChanged(ref this.uri, value);
        }

        /// <summary>
        /// Gets the 
        /// </summary>
        public string Name => this.Uri;

        /// <summary>
        /// Gets the <see cref="EngineeringModelRowViewModel"/>s that are contained by the current row-view-model
        /// </summary>
        public DisposableReactiveList<EngineeringModelRowViewModel> EngineeringModelRowViewModels { get; private set; }

        /// <summary>
        /// Gets the <see cref="SiteDirectoryRowViewModel"/> that is contained by the current row-view-model
        /// </summary>
        public SiteDirectoryRowViewModel SiteDirectoryRowViewModel { get; private set; }

        /// <summary>
        /// Add a new <see cref="EngineeringModelRowViewModel"/> to the current row-view-model
        /// </summary>
        /// <param name="engineeringModel">
        /// The <see cref="EngineeringModel"/> that is being represented by a new 
        /// </param>
        private void AddEngineeringModel(EngineeringModel engineeringModel)
        {
            var row = new EngineeringModelRowViewModel(engineeringModel, this.Session, this);
            this.EngineeringModelRowViewModels.Add(row);
            this.ContainedRows.Add(row);
        }

        /// <summary>
        /// Remove the <see cref="EngineeringModelRowViewModel"/> from the current row-view-model that represents the <see cref="EngineeringModel"/> that has been removed
        /// </summary>
        /// <param name="engineeringModel">
        /// The <see cref="EngineeringModel"/> that has been removed
        /// </param>
        private void RemoveEngineeringModel(EngineeringModel engineeringModel)
        {
            var row = this.EngineeringModelRowViewModels.SingleOrDefault(x => x.Thing == engineeringModel);

            if (row != null)
            {
                this.EngineeringModelRowViewModels.RemoveWithoutDispose(row);
                this.ContainedRows.RemoveAndDispose(row);
            }
        }
    }
}
