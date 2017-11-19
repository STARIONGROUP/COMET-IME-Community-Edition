// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="ElementDefinition"/>
    /// </summary>
    public partial class ElementDefinitionRowViewModel : ElementBaseRowViewModel<ElementDefinition>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ContainedElementRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel containedElementFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ParameterRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel parameterFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="ParameterGroupRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel parameterGroupFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRowViewModel"/> class
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public ElementDefinitionRowViewModel(ElementDefinition elementDefinition, ISession session, IViewModelBase<Thing> containerViewModel) : base(elementDefinition, session, containerViewModel)
        {
            this.containedElementFolder = new CDP4Composition.FolderRowViewModel("Contained Element", "Contained Element", this.Session, this);
            this.ContainedRows.Add(this.containedElementFolder);
            this.parameterFolder = new CDP4Composition.FolderRowViewModel("Parameter", "Parameter", this.Session, this);
            this.ContainedRows.Add(this.parameterFolder);
            this.parameterGroupFolder = new CDP4Composition.FolderRowViewModel("Parameter Group", "Parameter Group", this.Session, this);
            this.ContainedRows.Add(this.parameterGroupFolder);
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
            this.ComputeRows(this.Thing.ContainedElement, this.containedElementFolder, this.AddContainedElementRowViewModel);
            this.ComputeRows(this.Thing.Parameter, this.parameterFolder, this.AddParameterRowViewModel);
            this.ComputeRows(this.Thing.ParameterGroup, this.parameterGroupFolder, this.AddParameterGroupRowViewModel);
        }
        /// <summary>
        /// Add an Contained Element row view model to the list of <see cref="ContainedElement"/>
        /// </summary>
        /// <param name="containedElement">
        /// The <see cref="ContainedElement"/> that is to be added
        /// </param>
        private ElementUsageRowViewModel AddContainedElementRowViewModel(ElementUsage containedElement)
        {
            return new ElementUsageRowViewModel(containedElement, this.Session, this);
        }
        /// <summary>
        /// Add an Parameter row view model to the list of <see cref="Parameter"/>
        /// </summary>
        /// <param name="parameter">
        /// The <see cref="Parameter"/> that is to be added
        /// </param>
        private ParameterRowViewModel AddParameterRowViewModel(Parameter parameter)
        {
            return new ParameterRowViewModel(parameter, this.Session, this);
        }
        /// <summary>
        /// Add an Parameter Group row view model to the list of <see cref="ParameterGroup"/>
        /// </summary>
        /// <param name="parameterGroup">
        /// The <see cref="ParameterGroup"/> that is to be added
        /// </param>
        private ParameterGroupRowViewModel AddParameterGroupRowViewModel(ParameterGroup parameterGroup)
        {
            return new ParameterGroupRowViewModel(parameterGroup, this.Session, this);
        }
    }
}
