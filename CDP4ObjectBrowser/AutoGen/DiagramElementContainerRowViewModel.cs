// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramElementContainerRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Common.ReportingData;
    using System;
    using System.Reactive.Linq;

    /// <summary>
    /// Row class representing a <see cref="DiagramElementContainer"/>
    /// </summary>
    public abstract partial class DiagramElementContainerRowViewModel<T> : DiagramThingBaseRowViewModel<T>, IDiagramElementContainerRowViewModel<T> where T :DiagramElementContainer
    {
        /// <summary>
        /// Intermediate folder containing <see cref="DiagramElementRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel diagramElementFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="BoundsRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel boundsFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramElementContainerRowViewModel{T}"/> class
        /// </summary>
        /// <param name="diagramElementContainer">The <see cref="DiagramElementContainer"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected DiagramElementContainerRowViewModel(T diagramElementContainer, ISession session, IViewModelBase<Thing> containerViewModel) : base(diagramElementContainer, session, containerViewModel)
        {
            this.diagramElementFolder = new CDP4Composition.FolderRowViewModel("Diagram Element", "Diagram Element", this.Session, this);
            this.ContainedRows.Add(this.diagramElementFolder);
            this.boundsFolder = new CDP4Composition.FolderRowViewModel("Bounds", "Bounds", this.Session, this);
            this.ContainedRows.Add(this.boundsFolder);
            this.UpdateProperties();
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
        /// Updates all the properties rows
        /// /// </summary>
        private void UpdateProperties()
        {
            this.ComputeRows(this.Thing.DiagramElement, this.diagramElementFolder, this.AddDiagramElementRowViewModel);
            this.ComputeRows(this.Thing.Bounds, this.boundsFolder, this.AddBoundsRowViewModel);
        }
        /// <summary>
        /// Add an Diagram Element row view model to the list of <see cref="DiagramElementThing"/>
        /// </summary>
        /// <param name="diagramElement">
        /// The <see cref="DiagramElement"/> that is to be added
        /// </param>
        private IDiagramElementThingRowViewModel<DiagramElementThing> AddDiagramElementRowViewModel(DiagramElementThing diagramElement)
        {
        var diagramEdge = diagramElement as DiagramEdge;
        if (diagramEdge != null)
        {
            return new DiagramEdgeRowViewModel(diagramEdge, this.Session, this);
        }
        var diagramObject = diagramElement as DiagramObject;
        if (diagramObject != null)
        {
            return new DiagramObjectRowViewModel(diagramObject, this.Session, this);
        }
        throw new Exception("No DiagramElementThing to return");
        }
        /// <summary>
        /// Add an Bounds row view model to the list of <see cref="Bounds"/>
        /// </summary>
        /// <param name="bounds">
        /// The <see cref="Bounds"/> that is to be added
        /// </param>
        private BoundsRowViewModel AddBoundsRowViewModel(Bounds bounds)
        {
            return new BoundsRowViewModel(bounds, this.Session, this);
        }
    }
}
