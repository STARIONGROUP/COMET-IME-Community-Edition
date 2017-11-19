// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
            var engineeringModelAdded = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(EngineeringModel))
                .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                .Select(objectChange => objectChange.ChangedThing as EngineeringModel)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(this.AddEngineeringModel);

            this.Disposables.Add(engineeringModelAdded);

            var engineeringModelRemoved = CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(EngineeringModel))
                .Where(objectChange => objectChange.EventKind == EventKind.Removed)
                .Select(objectChange => objectChange.ChangedThing as EngineeringModel)
                .SubscribeOn(RxApp.MainThreadScheduler)
                .Subscribe(this.RemoveEngineeringModel);
            this.Disposables.Add(engineeringModelRemoved);

            this.SiteDirectoryRowViewModel = new SiteDirectoryRowViewModel(siteDirectory, this.Session, this);
            this.ContainedRows.Add(this.SiteDirectoryRowViewModel);

            this.EngineeringModelRowViewModels = new ReactiveList<EngineeringModelRowViewModel>();

            this.Uri = session.DataSourceUri;
        }

        /// <summary>
        /// Gets or sets the Uri of the <see cref="Session"/> current row-view-model
        /// </summary>
        public string Uri
        {
            get
            {
                return this.uri;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.uri, value);
            }
        }

        /// <summary>
        /// Gets the 
        /// </summary>
        public string Name
        {
            get
            {
                return this.Uri;
            }
        }

        /// <summary>
        /// Gets the <see cref="EngineeringModelRowViewModel"/>s that are contained by the current row-view-model
        /// </summary>
        public ReactiveList<EngineeringModelRowViewModel> EngineeringModelRowViewModels { get; private set; }

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
                this.EngineeringModelRowViewModels.Remove(row);
                this.ContainedRows.Remove(row);
            }
        }
    }
}
