// -------------------------------------------------------------------------------------------------
// <copyright file="SectionRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="Section"/>
    /// </summary>
    public partial class SectionRowViewModel : ObjectBrowserRowViewModel<Section>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="PageRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel pageFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionRowViewModel"/> class
        /// </summary>
        /// <param name="section">The <see cref="Section"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public SectionRowViewModel(Section section, ISession session, IViewModelBase<Thing> containerViewModel) : base(section, session, containerViewModel)
        {
            this.pageFolder = new CDP4Composition.FolderRowViewModel("Page", "Page", this.Session, this);
            this.ContainedRows.Add(this.pageFolder);
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
            this.ComputeRows(this.Thing.Page, this.pageFolder, this.AddPageRowViewModel);
        }
        /// <summary>
        /// Add an Page row view model to the list of <see cref="Page"/>
        /// </summary>
        /// <param name="page">
        /// The <see cref="Page"/> that is to be added
        /// </param>
        private PageRowViewModel AddPageRowViewModel(Page page)
        {
            return new PageRowViewModel(page, this.Session, this);
        }
    }
}
