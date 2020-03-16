// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramObjectViewModel.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels
{
    using System;
    using System.Linq;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Dal.Operations;
    using CDP4CommonView;
    using CDP4CommonView.Diagram;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The view-model representing a <see cref="DiagramObject"/>
    /// </summary>
    public class DiagramObjectViewModel : DiagramObjectRowViewModel, IDiagramObjectViewModel
    {
        #region Private field
        /// <summary>
        /// Backing field for <see cref="Position"/>
        /// </summary>
        private System.Windows.Point position;

        /// <summary>
        /// Backing field for <see cref="Height"/>
        /// </summary>
        private double height;

        /// <summary>
        /// Backing field for <see cref="Width"/>
        /// </summary>
        private double width;

        /// <summary>
        /// Backing field for <see cref="IsDirty"/>
        /// </summary>
        private bool isDirty;

        /// <summary>
        /// The <see cref="DiagramEditorViewModel"/> container
        /// </summary>
        private DiagramEditorViewModel containerViewModel;
        #endregion

        #region Constructors and properties
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramObjectViewModel"/> class
        /// </summary>
        /// <param name="diagramObject">The <see cref="DiagramObject"/></param>
        /// <param name="session">The current <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container view-model</param>
        public DiagramObjectViewModel(DiagramObject diagramObject, ISession session, DiagramEditorViewModel containerViewModel) : base(diagramObject, session, containerViewModel)
        {
            this.containerViewModel = containerViewModel;
            this.UpdateProperties();
            this.WhenAnyValue(x => x.Height, x => x.Width, x => x.Position).Subscribe(x => this.SetDirty());
        }
        
        /// <summary>
        /// Gets or sets the position 
        /// </summary>
        public System.Windows.Point Position
        {
            get { return this.position; }
            set { this.RaiseAndSetIfChanged(ref this.position, value); }
        }

        /// <summary>
        /// Gets or sets the height of this <see cref="DiagramObject"/>
        /// </summary>
        public double Height
        {
            get { return this.height; }
            set { this.RaiseAndSetIfChanged(ref this.height, value); }
        }

        /// <summary>
        /// Gets or sets the Width of this <see cref="DiagramObject"/>
        /// </summary>
        public double Width
        {
            get { return this.width; }
            set { this.RaiseAndSetIfChanged(ref this.width, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the diagram editor is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return this.isDirty; }
            set { this.RaiseAndSetIfChanged(ref this.isDirty, value); }
        }
        #endregion

        /// <summary>
        /// Handles the <see cref="ObjectChangedEvent"/>
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this view-model
        /// </summary>
        private void UpdateProperties()
        {
            var bound = this.Thing.Bounds.SingleOrDefault();
            if (bound == null)
            {
                throw new InvalidOperationException("The bound object was not found.");
            }

            this.Height = bound.Height;
            this.width = bound.Width;
            this.Position = new System.Windows.Point(bound.X, bound.Y);
        }

        /// <summary>
        /// Update a <see cref="Bounds"/> with the current values
        /// </summary>
        /// <param name="bound">The <see cref="Bounds"/> to update</param>
        private void UpdateBound(Bounds bound)
        {
            bound.Height = (float)this.Height;
            bound.Width = (float)this.Width;
            bound.X = (float)this.Position.X;
            bound.Y = (float)this.Position.Y;
            bound.Name = "should not have a name";
        }

        /// <summary>
        /// Set the <see cref="IsDirty"/> property
        /// </summary>
        private void SetDirty()
        {
            var bound = this.Thing.Bounds.Single();
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
                var bound = this.Thing.Bounds.Single();
                this.UpdateBound(bound);

                container.DiagramElement.Add(this.Thing);
                transaction.Create(bound);
                transaction.Create(this.Thing);
            }
            else
            {
                var clone = this.Thing.Clone(true);
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
    }
}