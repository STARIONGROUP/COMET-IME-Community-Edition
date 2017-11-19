// -------------------------------------------------------------------------------------------------
// <copyright file="FolderRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4EngineeringModel.ViewModels.FileStoreBrowsers;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    /// <summary>
    /// The folder row view model.
    /// </summary>
    public class FolderRowViewModel : CDP4CommonView.FolderRowViewModel
    {
        /// <summary>
        /// The <see cref="IViewModelBase<Thing>"/>
        /// </summary>
        private readonly IViewModelBase<Thing> fileStoreRow;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderRowViewModel"/> class. 
        /// </summary>
        /// <param name="folder">
        /// The <see cref="Folder"/> associated with this row
        /// </param>
        /// <param name="session">
        /// The session
        /// </param>
        /// <param name="containerViewModel">
        /// The <see cref="IViewModelBase<Thing>"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/>
        /// </param>
        public FolderRowViewModel(Folder folder, ISession session, IFileStoreRow containerViewModel)
            : base(folder, session, (IViewModelBase<Thing>)containerViewModel)
        {
            this.fileStoreRow = (IViewModelBase<Thing>)containerViewModel;
        }

        /// <summary>
        /// Update the <see cref="ContainerViewModel"/>
        /// </summary>
        /// <param name="containerViewModel">The new <see cref="ContainerViewModel"/></param>
        public void UpdateContainerViewModel(IViewModelBase<Thing> containerViewModel)
        {
            this.ContainerViewModel = containerViewModel;
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
            ((IFileStoreRow)this.fileStoreRow).UpdateFolderRowPosition(this.Thing);
        }
    }
}
