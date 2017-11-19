// -------------------------------------------------------------------------------------------------
// <copyright file="DiagramElementThingRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="DiagramElementThing"/>
    /// </summary>
    public abstract partial class DiagramElementThingRowViewModel<T> : DiagramElementContainerRowViewModel<T>, IDiagramElementThingRowViewModel<T> where T :DiagramElementThing
    {
        /// <summary>
        /// Intermediate folder containing <see cref="LocalStyleRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel localStyleFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramElementThingRowViewModel{T}"/> class
        /// </summary>
        /// <param name="diagramElementThing">The <see cref="DiagramElementThing"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected DiagramElementThingRowViewModel(T diagramElementThing, ISession session, IViewModelBase<Thing> containerViewModel) : base(diagramElementThing, session, containerViewModel)
        {
            this.localStyleFolder = new CDP4Composition.FolderRowViewModel("Local Style", "Local Style", this.Session, this);
            this.ContainedRows.Add(this.localStyleFolder);
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
            this.ComputeRows(this.Thing.LocalStyle, this.localStyleFolder, this.AddLocalStyleRowViewModel);
        }
        /// <summary>
        /// Add an Local Style row view model to the list of <see cref="LocalStyle"/>
        /// </summary>
        /// <param name="localStyle">
        /// The <see cref="LocalStyle"/> that is to be added
        /// </param>
        private OwnedStyleRowViewModel AddLocalStyleRowViewModel(OwnedStyle localStyle)
        {
            return new OwnedStyleRowViewModel(localStyle, this.Session, this);
        }
    }
}
