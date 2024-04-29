﻿// -------------------------------------------------------------------------------------------------
// <copyright file="PrefixedUnitRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.DiagramData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;    
    using ReactiveUI;

    /// <summary>
    /// Row class representing a <see cref="PrefixedUnit"/>
    /// </summary>
    public partial class PrefixedUnitRowViewModel : ConversionBasedUnitRowViewModel<PrefixedUnit>
    {

        /// <summary>
        /// Backing field for <see cref="Prefix"/>
        /// </summary>
        private UnitPrefix prefix;

        /// <summary>
        /// Backing field for <see cref="PrefixShortName"/>
        /// </summary>
        private string prefixShortName;

        /// <summary>
        /// Backing field for <see cref="PrefixName"/>
        /// </summary>
        private string prefixName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixedUnitRowViewModel"/> class
        /// </summary>
        /// <param name="prefixedUnit">The <see cref="PrefixedUnit"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public PrefixedUnitRowViewModel(PrefixedUnit prefixedUnit, ISession session, IViewModelBase<Thing> containerViewModel) : base(prefixedUnit, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Prefix
        /// </summary>
        public UnitPrefix Prefix
        {
            get { return this.prefix; }
            set { this.RaiseAndSetIfChanged(ref this.prefix, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Prefix"/>
        /// </summary>
        public string PrefixShortName
        {
            get { return this.prefixShortName; }
            set { this.RaiseAndSetIfChanged(ref this.prefixShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Prefix"/>
        /// </summary>
        public string PrefixName
        {
            get { return this.prefixName; }
            set { this.RaiseAndSetIfChanged(ref this.prefixName, value); }
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
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;
			if (this.Thing.Prefix != null)
			{
				this.PrefixShortName = this.Thing.Prefix.ShortName;
				this.PrefixName = this.Thing.Prefix.Name;
			}			
            this.Prefix = this.Thing.Prefix;
        }
    }
}
