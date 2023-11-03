// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FolderRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Extensions;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// The folder row view model.
    /// </summary>
    public class FolderRowViewModel : CDP4CommonView.FolderRowViewModel, IDropTarget, IOwnedThingViewModel
    {
        /// <summary>
        /// The <see cref="FileStoreFileAndFolderHandler{T}"/>
        /// </summary>
        private readonly IFileStoreFileAndFolderHandler parentFileStoreFileAndFolderHandler;

        /// <summary>
        /// The (injected) <see cref="DevExpress.Mvvm.IMessageBoxService"/>
        /// </summary>
        private readonly IMessageBoxService messageBoxService = ServiceLocator.Current.GetInstance<IMessageBoxService>();

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
        /// The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="FolderRowViewModel"/>
        /// </param>
        public FolderRowViewModel(Folder folder, ISession session, IViewModelBase<Thing> containerViewModel, IFileStoreFileAndFolderHandler parentFileStoreFileAndFolderHandler)
            : base(folder, session, containerViewModel)
        {
            this.parentFileStoreFileAndFolderHandler = parentFileStoreFileAndFolderHandler;
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
            this.parentFileStoreFileAndFolderHandler?.UpdateFolderRowPosition(this.Thing);
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                logger.Trace("drag over {0}", dropInfo.TargetItem);

                if (dropInfo.Payload is File file && (file.CurrentContainingFolder != this.Thing) && file.Container.Iid.Equals(this.Thing.Container.Iid))
                {
                    dropInfo.Effects = DragDropEffects.Move;
                    return;
                }

                dropInfo.Effects = DragDropEffects.None;
            }
            catch (Exception ex)
            {
                dropInfo.Effects = DragDropEffects.None;
                logger.Error(ex, "drag-over caused an error");
                throw;
            }
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is File file && (file.CurrentContainingFolder != this.Thing) && file.Container.Iid.Equals(this.Thing.Container.Iid))
            {
                try
                {
                    this.IsBusy = true;

                    var iteration = this.Thing.GetContainerOfType<Iteration>();

                    if (iteration == null)
                    {
                        var participant = this.Session.OpenIterations.FirstOrDefault().Value.Item2;
                        await file.MoveFile(this.Thing, participant, this.Session);
                    }
                    else
                    {
                        this.Session.OpenIterations.TryGetValue(iteration, out var tuple);
                        await file.MoveFile(this.Thing, tuple?.Item2, this.Session);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                    this.messageBoxService.Show(e.Message, "Moving file failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    this.IsBusy = false;
                }
            }
        }
    }
}
