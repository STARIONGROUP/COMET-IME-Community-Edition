// -------------------------------------------------------------------------------------------------
// <copyright file="ReviewItemDiscrepancyRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="ReviewItemDiscrepancy"/>
    /// </summary>
    public partial class ReviewItemDiscrepancyRowViewModel : ModellingAnnotationItemRowViewModel<ReviewItemDiscrepancy>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="SolutionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel solutionFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReviewItemDiscrepancyRowViewModel"/> class
        /// </summary>
        /// <param name="reviewItemDiscrepancy">The <see cref="ReviewItemDiscrepancy"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public ReviewItemDiscrepancyRowViewModel(ReviewItemDiscrepancy reviewItemDiscrepancy, ISession session, IViewModelBase<Thing> containerViewModel) : base(reviewItemDiscrepancy, session, containerViewModel)
        {
            this.solutionFolder = new CDP4Composition.FolderRowViewModel("Solution", "Solution", this.Session, this);
            this.ContainedRows.Add(this.solutionFolder);
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
            this.ComputeRows(this.Thing.Solution, this.solutionFolder, this.AddSolutionRowViewModel);
        }
        /// <summary>
        /// Add an Solution row view model to the list of <see cref="Solution"/>
        /// </summary>
        /// <param name="solution">
        /// The <see cref="Solution"/> that is to be added
        /// </param>
        private SolutionRowViewModel AddSolutionRowViewModel(Solution solution)
        {
            return new SolutionRowViewModel(solution, this.Session, this);
        }
    }
}
