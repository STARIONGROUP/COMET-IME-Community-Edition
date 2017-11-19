// -------------------------------------------------------------------------------------------------
// <copyright file="OptionRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="Option"/>
    /// </summary>
    public partial class OptionRowViewModel : DefinedThingRowViewModel<Option>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="NestedElementRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel nestedElementFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionRowViewModel"/> class
        /// </summary>
        /// <param name="option">The <see cref="Option"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public OptionRowViewModel(Option option, ISession session, IViewModelBase<Thing> containerViewModel) : base(option, session, containerViewModel)
        {
            this.nestedElementFolder = new CDP4Composition.FolderRowViewModel("Nested Element", "Nested Element", this.Session, this);
            this.ContainedRows.Add(this.nestedElementFolder);
            this.UpdateProperties();
            this.UpdateColumnValues();
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
            this.ComputeRows(this.Thing.NestedElement, this.nestedElementFolder, this.AddNestedElementRowViewModel);
        }
        /// <summary>
        /// Add an Nested Element row view model to the list of <see cref="NestedElement"/>
        /// </summary>
        /// <param name="nestedElement">
        /// The <see cref="NestedElement"/> that is to be added
        /// </param>
        private NestedElementRowViewModel AddNestedElementRowViewModel(NestedElement nestedElement)
        {
            return new NestedElementRowViewModel(nestedElement, this.Session, this);
        }
    }
}
