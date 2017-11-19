// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// A row view model that represents a <see cref="ReferenceSource"/>
    /// </summary>
    public class ReferenceSourceRowViewModel : CDP4CommonView.ReferenceSourceRowViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="ContainerRdl"/> property
        /// </summary>
        private string containerRdl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceSourceRowViewModel"/> class. 
        /// </summary>
        /// <param name="filetype">The <see cref="ReferenceSource"/> that is represented by the current view-model</param>
        /// <param name="session">The session.</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ReferenceSourceRowViewModel(ReferenceSource filetype, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(filetype, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the Container RDL ShortName.
        /// </summary>
        public string ContainerRdl
        {
            get { return this.containerRdl; }
            set { this.RaiseAndSetIfChanged(ref this.containerRdl, value); }
        }

        /// <summary>
        /// Updates the properties of the view-model
        /// </summary>
        private void UpdateProperties()
        {
            var container = (ReferenceDataLibrary)this.Thing.Container;
            this.ContainerRdl = container.ShortName;
        }

        #region RowViewModelBase
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
        #endregion
    }
}
