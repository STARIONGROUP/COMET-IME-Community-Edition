// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsContainerRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="RequirementsContainer"/>
    /// </summary>
    public abstract partial class RequirementsContainerRowViewModel<T> : DefinedThingRowViewModel<T>, IRequirementsContainerRowViewModel<T> where T :RequirementsContainer
    {
        /// <summary>
        /// Intermediate folder containing <see cref="GroupRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel groupFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ParameterValueRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel parameterValueFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsContainerRowViewModel{T}"/> class
        /// </summary>
        /// <param name="requirementsContainer">The <see cref="RequirementsContainer"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected RequirementsContainerRowViewModel(T requirementsContainer, ISession session, IViewModelBase<Thing> containerViewModel) : base(requirementsContainer, session, containerViewModel)
        {
            this.groupFolder = new CDP4Composition.FolderRowViewModel("Group", "Group", this.Session, this);
            this.ContainedRows.Add(this.groupFolder);
            this.parameterValueFolder = new CDP4Composition.FolderRowViewModel("Parameter Value", "Parameter Value", this.Session, this);
            this.ContainedRows.Add(this.parameterValueFolder);
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
            this.ComputeRows(this.Thing.Group, this.groupFolder, this.AddGroupRowViewModel);
            this.ComputeRows(this.Thing.ParameterValue, this.parameterValueFolder, this.AddParameterValueRowViewModel);
        }
        /// <summary>
        /// Add an Group row view model to the list of <see cref="Group"/>
        /// </summary>
        /// <param name="group">
        /// The <see cref="Group"/> that is to be added
        /// </param>
        private RequirementsGroupRowViewModel AddGroupRowViewModel(RequirementsGroup group)
        {
            return new RequirementsGroupRowViewModel(group, this.Session, this);
        }
        /// <summary>
        /// Add an Parameter Value row view model to the list of <see cref="ParameterValue"/>
        /// </summary>
        /// <param name="parameterValue">
        /// The <see cref="ParameterValue"/> that is to be added
        /// </param>
        private RequirementsContainerParameterValueRowViewModel AddParameterValueRowViewModel(RequirementsContainerParameterValue parameterValue)
        {
            return new RequirementsContainerParameterValueRowViewModel(parameterValue, this.Session, this);
        }
    }
}
