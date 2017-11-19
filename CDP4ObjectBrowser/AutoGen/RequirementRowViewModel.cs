// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="Requirement"/>
    /// </summary>
    public partial class RequirementRowViewModel : SimpleParameterizableThingRowViewModel<Requirement>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ParametricConstraintRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel parametricConstraintFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementRowViewModel"/> class
        /// </summary>
        /// <param name="requirement">The <see cref="Requirement"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public RequirementRowViewModel(Requirement requirement, ISession session, IViewModelBase<Thing> containerViewModel) : base(requirement, session, containerViewModel)
        {
            this.parametricConstraintFolder = new CDP4Composition.FolderRowViewModel("Parametric Constraint", "Parametric Constraint", this.Session, this);
            this.ContainedRows.Add(this.parametricConstraintFolder);
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
            this.ComputeRows(this.Thing.ParametricConstraint, this.parametricConstraintFolder, this.AddParametricConstraintRowViewModel);
        }
        /// <summary>
        /// Add an Parametric Constraint row view model to the list of <see cref="ParametricConstraint"/>
        /// </summary>
        /// <param name="parametricConstraint">
        /// The <see cref="ParametricConstraint"/> that is to be added
        /// </param>
        private ParametricConstraintRowViewModel AddParametricConstraintRowViewModel(ParametricConstraint parametricConstraint)
        {
            return new ParametricConstraintRowViewModel(parametricConstraint, this.Session, this);
        }
    }
}
