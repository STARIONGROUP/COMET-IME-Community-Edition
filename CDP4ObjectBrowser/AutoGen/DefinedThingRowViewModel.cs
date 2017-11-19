// -------------------------------------------------------------------------------------------------
// <copyright file="DefinedThingRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="DefinedThing"/>
    /// </summary>
    public abstract partial class DefinedThingRowViewModel<T> : ObjectBrowserRowViewModel<T>, IDefinedThingRowViewModel<T> where T :DefinedThing
    {
        /// <summary>
        /// Intermediate folder containing <see cref="AliasRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel aliasFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="DefinitionRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel definitionFolder;

        /// <summary>
        /// Intermediate folder containing <see cref="HyperLinkRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel hyperLinkFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedThingRowViewModel{T}"/> class
        /// </summary>
        /// <param name="definedThing">The <see cref="DefinedThing"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected DefinedThingRowViewModel(T definedThing, ISession session, IViewModelBase<Thing> containerViewModel) : base(definedThing, session, containerViewModel)
        {
            this.aliasFolder = new CDP4Composition.FolderRowViewModel("Alias", "Alias", this.Session, this);
            this.ContainedRows.Add(this.aliasFolder);
            this.definitionFolder = new CDP4Composition.FolderRowViewModel("Definition", "Definition", this.Session, this);
            this.ContainedRows.Add(this.definitionFolder);
            this.hyperLinkFolder = new CDP4Composition.FolderRowViewModel("Hyper Link", "Hyper Link", this.Session, this);
            this.ContainedRows.Add(this.hyperLinkFolder);
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
            this.ComputeRows(this.Thing.Alias, this.aliasFolder, this.AddAliasRowViewModel);
            this.ComputeRows(this.Thing.Definition, this.definitionFolder, this.AddDefinitionRowViewModel);
            this.ComputeRows(this.Thing.HyperLink, this.hyperLinkFolder, this.AddHyperLinkRowViewModel);
        }
        /// <summary>
        /// Add an Alias row view model to the list of <see cref="Alias"/>
        /// </summary>
        /// <param name="alias">
        /// The <see cref="Alias"/> that is to be added
        /// </param>
        private AliasRowViewModel AddAliasRowViewModel(Alias alias)
        {
            return new AliasRowViewModel(alias, this.Session, this);
        }
        /// <summary>
        /// Add an Definition row view model to the list of <see cref="Definition"/>
        /// </summary>
        /// <param name="definition">
        /// The <see cref="Definition"/> that is to be added
        /// </param>
        private DefinitionRowViewModel AddDefinitionRowViewModel(Definition definition)
        {
            return new DefinitionRowViewModel(definition, this.Session, this);
        }
        /// <summary>
        /// Add an Hyper Link row view model to the list of <see cref="HyperLink"/>
        /// </summary>
        /// <param name="hyperLink">
        /// The <see cref="HyperLink"/> that is to be added
        /// </param>
        private HyperLinkRowViewModel AddHyperLinkRowViewModel(HyperLink hyperLink)
        {
            return new HyperLinkRowViewModel(hyperLink, this.Session, this);
        }
    }
}
