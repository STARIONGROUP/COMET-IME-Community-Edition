// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="ParameterOverride"/>
    /// </summary>
    public partial class ParameterOverrideRowViewModel : ParameterOrOverrideBaseRowViewModel<ParameterOverride>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ValueSetRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel valueSetFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideRowViewModel"/> class
        /// </summary>
        /// <param name="parameterOverride">The <see cref="ParameterOverride"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public ParameterOverrideRowViewModel(ParameterOverride parameterOverride, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameterOverride, session, containerViewModel)
        {
            this.valueSetFolder = new CDP4Composition.FolderRowViewModel("Value Set", "Value Set", this.Session, this);
            this.ContainedRows.Add(this.valueSetFolder);
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
            this.ComputeRows(this.Thing.ValueSet, this.valueSetFolder, this.AddValueSetRowViewModel);
        }
        /// <summary>
        /// Add an Value Set row view model to the list of <see cref="ValueSet"/>
        /// </summary>
        /// <param name="valueSet">
        /// The <see cref="ValueSet"/> that is to be added
        /// </param>
        private ParameterOverrideValueSetRowViewModel AddValueSetRowViewModel(ParameterOverrideValueSet valueSet)
        {
            return new ParameterOverrideValueSetRowViewModel(valueSet, this.Session, this);
        }
    }
}
