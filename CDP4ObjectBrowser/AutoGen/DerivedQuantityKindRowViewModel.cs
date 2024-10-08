﻿// -------------------------------------------------------------------------------------------------
// <copyright file="DerivedQuantityKindRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="DerivedQuantityKind"/>
    /// </summary>
    public partial class DerivedQuantityKindRowViewModel : QuantityKindRowViewModel<DerivedQuantityKind>
    {
        /// <summary>
        /// Intermediate folder containing <see cref="QuantityKindFactorRowViewModel"/>
        /// </summary>
        private CDP4Composition.FolderRowViewModel quantityKindFactorFolder;

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivedQuantityKindRowViewModel"/> class
        /// </summary>
        /// <param name="derivedQuantityKind">The <see cref="DerivedQuantityKind"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase"/> that is the container of this <see cref="IRowViewModelBase"/></param>
        public DerivedQuantityKindRowViewModel(DerivedQuantityKind derivedQuantityKind, ISession session, IViewModelBase<Thing> containerViewModel) : base(derivedQuantityKind, session, containerViewModel)
        {
            this.quantityKindFactorFolder = new CDP4Composition.FolderRowViewModel("Quantity Kind Factor", "Quantity Kind Factor", this.Session, this);
            this.ContainedRows.Add(this.quantityKindFactorFolder);
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
            this.ComputeRows(this.Thing.QuantityKindFactor, this.quantityKindFactorFolder, this.AddQuantityKindFactorRowViewModel);
        }
        /// <summary>
        /// Add an Quantity Kind Factor row view model to the list of <see cref="QuantityKindFactor"/>
        /// </summary>
        /// <param name="quantityKindFactor">
        /// The <see cref="QuantityKindFactor"/> that is to be added
        /// </param>
        private QuantityKindFactorRowViewModel AddQuantityKindFactorRowViewModel(QuantityKindFactor quantityKindFactor)
        {
            return new QuantityKindFactorRowViewModel(quantityKindFactor, this.Session, this);
        }
    }
}
