// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// A row view model that represents a <see cref="ReferenceSource"/>
    /// </summary>
    public class ReferenceSourceRowViewModel : CDP4CommonView.ReferenceSourceRowViewModel, IDropTarget
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
            get => this.containerRdl;
            set => this.RaiseAndSetIfChanged(ref this.containerRdl, value);
        }

        /// <summary>
        /// Updates the properties of the view-model
        /// </summary>
        private void UpdateProperties()
        {
            var container = (ReferenceDataLibrary)this.Thing.Container;
            this.ContainerRdl = container.ShortName;
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
            if (dropInfo.Payload is Category category)
            {
                dropInfo.Effects = CategoryApplicationValidationService.ValidateDragDrop(this.Session.PermissionService, this.Thing, category, logger);
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var category = dropInfo.Payload as Category;
            if (category != null)
            {
                var clone = this.Thing.Clone(false);
                clone.Category.Add(category);
                await this.DalWrite(clone);
            }
        }
    }
}