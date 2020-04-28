// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingDiagramContentItem.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Composition.Diagram
{
    using System;
    using System.Linq;

    using CDP4Common.DiagramData;

    using CDP4Dal.Operations;

    using DevExpress.Xpf.Diagram;
    
    using Thing = CDP4Common.CommonData.Thing;

    /// <summary>
    /// Represents a diagram content control class that can store a <see cref="Thing"/>.
    /// </summary>
    public abstract class ThingDiagramContentItem : DiagramContentItem, IThingDiagramItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing represented.</param>
        protected ThingDiagramContentItem(Thing thing)
        {
            this.Thing = thing;
            this.Content = thing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="diagramThing">
        /// The diagramThing contained</param>
        /// <param name="containerViewModel">
        /// The view model container of kind <see cref="IDiagramEditorViewModel"/></param>
        protected ThingDiagramContentItem(DiagramObject diagramThing, IDiagramEditorViewModel containerViewModel)
        {
            this.containerViewModel = containerViewModel;
            this.Thing = diagramThing.DepictedThing;
            this.Content = diagramThing.DepictedThing;
            this.DiagramThing = diagramThing;
        }

        /// <summary>
        /// The <see cref="IDiagramEditorViewModel"/> container
        /// </summary>
        private readonly IDiagramEditorViewModel containerViewModel;

        /// <summary>
        /// Gets or sets the <see cref="IThingDiagramItem.Thing"/>.
        /// </summary>
        public Thing Thing { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CDP4Common.CommonData.Thing"/> representing the diagram with all of its diagram elements
        /// </summary>
        public DiagramObject DiagramThing { get; set; }

        /// <summary>
        /// Gets a value indicating whether the diagram editor is dirty
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// The observable for when the position of the view object changed
        /// </summary>
        public IDisposable PositionObservable { get; set; }

        /// <summary>
        /// Sets the <see cref="IsDirty"/> property
        /// </summary>
        public void SetDirty()
        {
            var bound = this.DiagramThing.Bounds.Single();

            this.IsDirty = this.Parent is DiagramContentItem parent
                           && parent.Position != default
                           && (this.Thing.Iid == Guid.Empty 
                           || (float)parent.Position.X != bound.X
                           || (float)parent.Position.Y != bound.Y);

            this.containerViewModel?.UpdateIsDirty();
        }

        /// <summary>
        /// Update the transaction with the data contained in this view-model
        /// </summary>
        /// <param name="transaction">The transaction to update</param>
        /// <param name="container">The container</param>
        public void UpdateTransaction(IThingTransaction transaction, DiagramElementContainer container)
        {
            if (this.Thing.Iid == Guid.Empty)
            {
                var bound = this.DiagramThing.Bounds.Single();
                this.UpdateBound(bound);

                container.DiagramElement.Add(this.DiagramThing);
                transaction.Create(bound);
                transaction.Create(this.Thing);
            }
            else
            {
                var clone = this.DiagramThing.Clone(true);
                var bound = clone.Bounds.SingleOrDefault();

                if (bound != null)
                {
                    this.UpdateBound(bound);
                }

                container.DiagramElement.Add(clone);
                transaction.CreateOrUpdate(clone);
                transaction.CreateOrUpdate(bound);
            }
        }

        /// <summary>
        /// Update a <see cref="Bounds"/> with the current values
        /// </summary>
        /// <param name="bound">The <see cref="Bounds"/> to update</param>
        private void UpdateBound(Bounds bound)
        {
            if (this.Parent is DiagramContentItem parent)
            {
                bound.Height = (float)parent.ActualHeight;
                bound.Width = (float)parent.ActualWidth;
                bound.X = (float)parent.Position.X;
                bound.Y = (float)parent.Position.Y;
                bound.Name = "should not have a name";
            }
        }
    }
}
