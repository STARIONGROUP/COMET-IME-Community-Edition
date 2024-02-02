// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramCanvasRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels.Rows
{
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="DiagramCanvas"/>
    /// </summary>
    public class DiagramCanvasRowViewModel : CDP4CommonView.DiagramCanvasRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="Description" />
        /// </summary>
        private string description;

        /// <summary>
        /// Backing field for <see cref="PublicationState" />
        /// </summary>
        private PublicationState publicationState;

        /// <summary>
        /// Backing field for <see cref="OwnerShortName"/>
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramCanvasRowViewModel" /> class
        /// </summary>
        /// <param name="diagramCanvas">The <see cref="DiagramCanvas" /> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">
        /// The <see cref="IViewModelBase{T}" /> that is the container of this
        /// <see cref="IRowViewModelBase{Thing}" />
        /// </param>
        public DiagramCanvasRowViewModel(DiagramCanvas diagramCanvas, ISession session, IViewModelBase<Thing> containerViewModel) : base(diagramCanvas, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets a value that represents the Description
        /// </summary>
        public string Description
        {
            get { return this.description; }
            set { this.RaiseAndSetIfChanged(ref this.description, value); }
        }

        /// <summary>
        /// Gets or sets the PublicationState
        /// </summary>
        public PublicationState PublicationState
        {
            get { return this.publicationState; }
            set { this.RaiseAndSetIfChanged(ref this.publicationState, value); }
        }

        /// <summary>
        /// Gets or sets the Owner ShortName
        /// </summary>
        public string OwnerShortName
        {
            get { return this.ownerShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerShortName, value); }
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.Description = this.Thing.Description;
            this.PublicationState = this.Thing.PublicationState;
        }
    }
}
