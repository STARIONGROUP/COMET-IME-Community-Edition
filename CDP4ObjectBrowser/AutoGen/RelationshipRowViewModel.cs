﻿// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="Relationship"/>
    /// </summary>
    public abstract partial class RelationshipRowViewModel<T> : ObjectBrowserRowViewModel<T>, IRelationshipRowViewModel<T> where T :Relationship
    {
        /// <summary>
        /// Intermediate folder containing <see cref="ParameterValueRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel parameterValueFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipRowViewModel{T}"/> class
        /// </summary>
        /// <param name="relationship">The <see cref="Relationship"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        protected RelationshipRowViewModel(T relationship, ISession session, IViewModelBase<Thing> containerViewModel) : base(relationship, session, containerViewModel)
        {
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
            this.ComputeRows(this.Thing.ParameterValue, this.parameterValueFolder, this.AddParameterValueRowViewModel);
        }
        /// <summary>
        /// Add an Parameter Value row view model to the list of <see cref="ParameterValue"/>
        /// </summary>
        /// <param name="parameterValue">
        /// The <see cref="ParameterValue"/> that is to be added
        /// </param>
        private RelationshipParameterValueRowViewModel AddParameterValueRowViewModel(RelationshipParameterValue parameterValue)
        {
            return new RelationshipParameterValueRowViewModel(parameterValue, this.Session, this);
        }
    }
}
