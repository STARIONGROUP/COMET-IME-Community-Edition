// -------------------------------------------------------------------------------------------------
// <copyright file="BookRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="Book"/>
    /// </summary>
    public partial class BookRowViewModel : ObjectBrowserRowViewModel<Book>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="SectionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel sectionFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookRowViewModel"/> class
        /// </summary>
        /// <param name="book">The <see cref="Book"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public BookRowViewModel(Book book, ISession session, IViewModelBase<Thing> containerViewModel) : base(book, session, containerViewModel)
        {
            this.sectionFolder = new CDP4Composition.FolderRowViewModel("Section", "Section", this.Session, this);
            this.ContainedRows.Add(this.sectionFolder);
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
            this.ComputeRows(this.Thing.Section, this.sectionFolder, this.AddSectionRowViewModel);
        }
        /// <summary>
        /// Add an Section row view model to the list of <see cref="Section"/>
        /// </summary>
        /// <param name="section">
        /// The <see cref="Section"/> that is to be added
        /// </param>
        private SectionRowViewModel AddSectionRowViewModel(Section section)
        {
            return new SectionRowViewModel(section, this.Session, this);
        }
    }
}
