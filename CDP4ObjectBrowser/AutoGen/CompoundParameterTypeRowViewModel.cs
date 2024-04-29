// -------------------------------------------------------------------------------------------------
// <copyright file="CompoundParameterTypeRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="CompoundParameterType"/>
    /// </summary>
    public partial class CompoundParameterTypeRowViewModel : ParameterTypeRowViewModel<CompoundParameterType>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ComponentRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel componentFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundParameterTypeRowViewModel"/> class
        /// </summary>
        /// <param name="compoundParameterType">The <see cref="CompoundParameterType"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public CompoundParameterTypeRowViewModel(CompoundParameterType compoundParameterType, ISession session, IViewModelBase<Thing> containerViewModel) : base(compoundParameterType, session, containerViewModel)
        {
            this.componentFolder = new CDP4Composition.FolderRowViewModel("Component", "Component", this.Session, this);
            this.ContainedRows.Add(this.componentFolder);
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
            this.ComputeRows(this.Thing.Component, this.componentFolder, this.AddComponentRowViewModel);
        }
        /// <summary>
        /// Add an Component row view model to the list of <see cref="Component"/>
        /// </summary>
        /// <param name="component">
        /// The <see cref="Component"/> that is to be added
        /// </param>
        private ParameterTypeComponentRowViewModel AddComponentRowViewModel(ParameterTypeComponent component)
        {
            return new ParameterTypeComponentRowViewModel(component, this.Session, this);
        }
    }
}
