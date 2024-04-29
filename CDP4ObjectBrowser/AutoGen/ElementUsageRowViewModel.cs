// -------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="ElementUsage"/>
    /// </summary>
    public partial class ElementUsageRowViewModel : ElementBaseRowViewModel<ElementUsage>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ParameterOverrideRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel parameterOverrideFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageRowViewModel"/> class
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public ElementUsageRowViewModel(ElementUsage elementUsage, ISession session, IViewModelBase<Thing> containerViewModel) : base(elementUsage, session, containerViewModel)
        {
            this.parameterOverrideFolder = new CDP4Composition.FolderRowViewModel("Parameter Override", "Parameter Override", this.Session, this);
            this.ContainedRows.Add(this.parameterOverrideFolder);
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
            this.ComputeRows(this.Thing.ParameterOverride, this.parameterOverrideFolder, this.AddParameterOverrideRowViewModel);
        }
        /// <summary>
        /// Add an Parameter Override row view model to the list of <see cref="ParameterOverride"/>
        /// </summary>
        /// <param name="parameterOverride">
        /// The <see cref="ParameterOverride"/> that is to be added
        /// </param>
        private ParameterOverrideRowViewModel AddParameterOverrideRowViewModel(ParameterOverride parameterOverride)
        {
            return new ParameterOverrideRowViewModel(parameterOverride, this.Session, this);
        }
    }
}
