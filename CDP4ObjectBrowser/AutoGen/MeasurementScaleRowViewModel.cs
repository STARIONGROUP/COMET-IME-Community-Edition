// -------------------------------------------------------------------------------------------------
// <copyright file="MeasurementScaleRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="MeasurementScale"/>
    /// </summary>
    public abstract partial class MeasurementScaleRowViewModel<T> : DefinedThingRowViewModel<T>, IMeasurementScaleRowViewModel<T> where T :MeasurementScale
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ValueDefinitionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel valueDefinitionFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="MappingToReferenceScaleRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel mappingToReferenceScaleFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementScaleRowViewModel{T}"/> class
        /// </summary>
        /// <param name="measurementScale">The <see cref="MeasurementScale"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected MeasurementScaleRowViewModel(T measurementScale, ISession session, IViewModelBase<Thing> containerViewModel) : base(measurementScale, session, containerViewModel)
        {
            this.valueDefinitionFolder = new CDP4Composition.FolderRowViewModel("Value Definition", "Value Definition", this.Session, this);
            this.ContainedRows.Add(this.valueDefinitionFolder);
            this.mappingToReferenceScaleFolder = new CDP4Composition.FolderRowViewModel("Mapping To Reference Scale", "Mapping To Reference Scale", this.Session, this);
            this.ContainedRows.Add(this.mappingToReferenceScaleFolder);
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
            this.ComputeRows(this.Thing.ValueDefinition, this.valueDefinitionFolder, this.AddValueDefinitionRowViewModel);
            this.ComputeRows(this.Thing.MappingToReferenceScale, this.mappingToReferenceScaleFolder, this.AddMappingToReferenceScaleRowViewModel);
        }
        /// <summary>
        /// Add an Value Definition row view model to the list of <see cref="ValueDefinition"/>
        /// </summary>
        /// <param name="valueDefinition">
        /// The <see cref="ValueDefinition"/> that is to be added
        /// </param>
        private ScaleValueDefinitionRowViewModel AddValueDefinitionRowViewModel(ScaleValueDefinition valueDefinition)
        {
            return new ScaleValueDefinitionRowViewModel(valueDefinition, this.Session, this);
        }
        /// <summary>
        /// Add an Mapping To Reference Scale row view model to the list of <see cref="MappingToReferenceScale"/>
        /// </summary>
        /// <param name="mappingToReferenceScale">
        /// The <see cref="MappingToReferenceScale"/> that is to be added
        /// </param>
        private MappingToReferenceScaleRowViewModel AddMappingToReferenceScaleRowViewModel(MappingToReferenceScale mappingToReferenceScale)
        {
            return new MappingToReferenceScaleRowViewModel(mappingToReferenceScale, this.Session, this);
        }
    }
}
