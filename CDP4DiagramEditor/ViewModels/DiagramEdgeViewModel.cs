// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramEdgeViewModel.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4CommonView;
    using CDP4CommonView.Diagram;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;
    using Point = System.Windows.Point;

    /// <summary>
    /// The view-model representing a <see cref="DiagramEdge"/>
    /// </summary>
    public class DiagramEdgeViewModel : DiagramEdgeRowViewModel, IDiagramConnectorViewModel
    {
        /// <summary>
        /// Backing field for <see cref="DisplayedText"/>
        /// </summary>
        private string displayedText;

        /// <summary>
        /// Backing field for <see cref="ConnectingPoints"/>
        /// </summary>
        public List<Point> connectingPoints;

        /// <summary>
        /// Initializes a new instance of the <see cref=""/> class
        /// </summary>
        /// <param name="diagramEdge">The associated <see cref="DiagramEdge"/></param>
        /// <param name="session">The current <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container view-model</param>
        public DiagramEdgeViewModel(DiagramEdge diagramEdge, ISession session, IViewModelBase<Thing> containerViewModel) : base(diagramEdge, session, containerViewModel)
        {
            this.ConnectingPoints = new List<Point>();
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets the displayed text
        /// </summary>
        public string DisplayedText
        {
            get { return this.displayedText; }
            private set { this.RaiseAndSetIfChanged(ref this.displayedText, value); }
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> event-handler
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
            var relationship = this.Thing.DepictedThing as BinaryRelationship;
            this.DisplayedText = this.Thing.DepictedThing.UserFriendlyName;

            if (relationship != null)
            {
                this.DisplayedText = relationship.Category.Any() ? string.Join(", ", relationship.Category.Select(x => x.Name)) : "Un-categorized";
            }
        }

        /// <summary>
        /// Gets or sets the collection of connecting <see cref="Point"/>
        /// </summary>
        public List<Point> ConnectingPoints
        {
            get { return this.connectingPoints; }
            set { this.RaiseAndSetIfChanged(ref this.connectingPoints, value); }
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        /// <param name="transaction">The transaction to update</param>
        /// <param name="container">The container</param>
        public void UpdateTransaction(IThingTransaction transaction, DiagramElementContainer container)
        {
            if (this.Thing.Iid == Guid.Empty)
            {
                this.Thing.Source = this.Source;
                this.Thing.Target = this.Target;
                this.Thing.Name = this.DisplayedText;
                container.DiagramElement.Add(this.Thing);
                transaction.Create(this.Thing);
            }
            else
            {
                var clone = this.Thing.Clone(true);
                clone.Source = this.Source;
                clone.Target = this.Target;
                clone.Name = this.DisplayedText;

                container.DiagramElement.Add(clone);
                transaction.CreateOrUpdate(clone);
            }
        }
    }
}