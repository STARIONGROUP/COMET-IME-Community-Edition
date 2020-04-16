// -------------------------------------------------------------------------------------------------
// <copyright file="ThingDiagramContentItem.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using System;
    using System.Linq;

    using CDP4Common.DiagramData;

    using CDP4Dal.Operations;

    using DevExpress.Xpf.Diagram;

    using ReactiveUI;

    using Thing = CDP4Common.CommonData.Thing;

    /// <summary>
    /// Represents a diagram content control class that can store a <see cref="Thing"/>.
    /// </summary>
    public abstract class ThingDiagramContentItem : DiagramContentItem, IThingDiagramItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="diagramThing">
        /// The diagramThing represented.</param>
        protected ThingDiagramContentItem(DiagramObject diagramThing, IDiagramEditorViewModel containerViewModel)
        {
            this.containerViewModel = containerViewModel;
            this.Thing = diagramThing.DepictedThing;
            this.Content = diagramThing.DepictedThing;
            this.DiagramThing = diagramThing;
            this.WhenAnyValue(x => x.Height, x => x.Width, x => x.Position).Subscribe(x => this.SetDirty());
        }


        /// <summary>
        /// The <see cref="IDiagramEditorViewModel"/> container
        /// </summary>
        private IDiagramEditorViewModel containerViewModel;

        /// <summary>
        /// Gets or sets the <see cref="IThingDiagramItem.Thing"/>.
        /// </summary>
        public Thing Thing { get; set; }

        /// <summary>
        /// Gets or sets
        /// </summary>
        public DiagramObject DiagramThing { get; set; }

        /// <summary>
        /// Gets a value indicating whether the diagram editor is dirty
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Set the <see cref="IsDirty"/> property
        /// </summary>
        private void SetDirty()
        {
            var bound = this.DiagramThing.Bounds.Single();
            this.IsDirty = this.Thing.Iid == Guid.Empty
                           || this.Height != bound.Height
                           || this.Width != bound.Width
                           || this.Position.X != bound.X
                           || this.Position.Y != bound.Y;

            this.containerViewModel.UpdateIsDirty();
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
            var parent = (this.Parent as DiagramContentItem);
            bound.Height = (float)parent.ActualHeight;
            bound.Width = (float)parent.ActualWidth;
            bound.X = (float)parent.Position.X;
            bound.Y = (float)parent.Position.Y;
            bound.Name = "should not have a name";
        }
    }
}
