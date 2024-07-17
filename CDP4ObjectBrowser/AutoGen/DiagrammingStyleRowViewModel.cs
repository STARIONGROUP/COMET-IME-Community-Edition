// -------------------------------------------------------------------------------------------------
// <copyright file="DiagrammingStyleRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2017 Starion Group S.A.
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
    /// Row class representing a <see cref="DiagrammingStyle"/>
    /// </summary>
    public abstract partial class DiagrammingStyleRowViewModel<T> : DiagramThingBaseRowViewModel<T>, IDiagrammingStyleRowViewModel<T> where T :DiagrammingStyle
    {
        /// <summary>
        /// Intermediate folder containing <see cref="UsedColorRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel usedColorFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagrammingStyleRowViewModel{T}"/> class
        /// </summary>
        /// <param name="diagrammingStyle">The <see cref="DiagrammingStyle"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected DiagrammingStyleRowViewModel(T diagrammingStyle, ISession session, IViewModelBase<Thing> containerViewModel) : base(diagrammingStyle, session, containerViewModel)
        {
            this.usedColorFolder = new CDP4Composition.FolderRowViewModel("Used Color", "Used Color", this.Session, this);
            this.ContainedRows.Add(this.usedColorFolder);
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
            this.ComputeRows(this.Thing.UsedColor, this.usedColorFolder, this.AddUsedColorRowViewModel);
        }
        /// <summary>
        /// Add an Used Color row view model to the list of <see cref="UsedColor"/>
        /// </summary>
        /// <param name="usedColor">
        /// The <see cref="UsedColor"/> that is to be added
        /// </param>
        private ColorRowViewModel AddUsedColorRowViewModel(Color usedColor)
        {
            return new ColorRowViewModel(usedColor, this.Session, this);
        }
    }
}
