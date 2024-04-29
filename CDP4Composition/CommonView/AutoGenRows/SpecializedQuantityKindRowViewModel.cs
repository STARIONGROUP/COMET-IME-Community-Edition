﻿// -------------------------------------------------------------------------------------------------
// <copyright file="SpecializedQuantityKindRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="SpecializedQuantityKind"/>
    /// </summary>
    public partial class SpecializedQuantityKindRowViewModel : QuantityKindRowViewModel<SpecializedQuantityKind>
    {

        /// <summary>
        /// Backing field for <see cref="General"/>
        /// </summary>
        private QuantityKind general;

        /// <summary>
        /// Backing field for <see cref="GeneralShortName"/>
        /// </summary>
        private string generalShortName;

        /// <summary>
        /// Backing field for <see cref="GeneralName"/>
        /// </summary>
        private string generalName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecializedQuantityKindRowViewModel"/> class
        /// </summary>
        /// <param name="specializedQuantityKind">The <see cref="SpecializedQuantityKind"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public SpecializedQuantityKindRowViewModel(SpecializedQuantityKind specializedQuantityKind, ISession session, IViewModelBase<Thing> containerViewModel) : base(specializedQuantityKind, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the General
        /// </summary>
        public QuantityKind General
        {
            get { return this.general; }
            set { this.RaiseAndSetIfChanged(ref this.general, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="General"/>
        /// </summary>
        public string GeneralShortName
        {
            get { return this.generalShortName; }
            set { this.RaiseAndSetIfChanged(ref this.generalShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="General"/>
        /// </summary>
        public string GeneralName
        {
            get { return this.generalName; }
            set { this.RaiseAndSetIfChanged(ref this.generalName, value); }
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
			if (this.Thing.General != null)
			{
				this.GeneralShortName = this.Thing.General.ShortName;
				this.GeneralName = this.Thing.General.Name;
			}			
            this.General = this.Thing.General;
        }
    }
}
