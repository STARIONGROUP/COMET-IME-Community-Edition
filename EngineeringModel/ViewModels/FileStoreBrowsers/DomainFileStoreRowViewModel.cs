﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Extensions;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4EngineeringModel.ViewModels.FileStoreBrowsers;
    
    using ReactiveUI;

    /// <summary>
    /// The <see cref="DomainFileStore"/> row-view-model
    /// </summary>
    public class DomainFileStoreRowViewModel : CDP4CommonView.DomainFileStoreRowViewModel, IFileStoreRow<DomainFileStore>, IDropTarget
    {
        /// <summary>
        /// Backing field for the <see cref="CreationDate"/> property
        /// </summary>
        private string creationDate;

        /// <summary>
        /// The <see cref="IFileStoreFileAndFolderHandler"/>
        /// </summary>
        private readonly IFileStoreFileAndFolderHandler fileStoreFileAndFolderHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainFileStoreRowViewModel"/> class
        /// </summary>
        /// <param name="store">The associated <see cref="DomainFileStore"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container view-model</param>
        public DomainFileStoreRowViewModel(DomainFileStore store, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(store, session, containerViewModel)
        {
            this.fileStoreFileAndFolderHandler = new FileStoreFileAndFolderHandler<DomainFileStore>(this);
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the date of creation of the <see cref="Folder"/>
        /// </summary>
        public string CreationDate
        {
            get => this.creationDate;
            private set => this.RaiseAndSetIfChanged(ref this.creationDate, value);
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
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.fileStoreFileAndFolderHandler.UpdateFileRows();
            this.CreationDate = this.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);
            this.UpdateThingStatus();
        }

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            base.UpdateThingStatus();
            this.ThingStatus = new ThingStatus(this.Thing) { IsHidden = this.IsHidden };
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

                if (dropInfo.Payload is File file && (file.CurrentContainingFolder != null) && file.Container.Iid.Equals(this.Thing.Iid))
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
            if (dropInfo.Payload is File file && (file.CurrentContainingFolder != null) && file.Container.Iid.Equals(this.Thing.Iid))
            {
                try
                {
                    this.IsBusy = true;

                    var iteration = this.Thing.GetContainerOfType<Iteration>();
                    this.Session.OpenIterations.TryGetValue(iteration, out var tuple);

                    await file.MoveFile(null, tuple?.Item2, this.Session);
                }
                catch (Exception e)
                {
                    logger.Error(e.Message);
                }
                finally
                {
                    this.IsBusy = false;
                }
            }
        }
    }
}
