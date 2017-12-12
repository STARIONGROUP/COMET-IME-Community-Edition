// -------------------------------------------------------------------------------------------------
// <copyright file="ReferenceDataLibraryRowViewModel.cs" company="RHEA S.A.">
//   Copyright (c) 2015-2017 RHEA System S.A.
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
    /// Row class representing a <see cref="ReferenceDataLibrary"/>
    /// </summary>
    public abstract partial class ReferenceDataLibraryRowViewModel<T> : DefinedThingRowViewModel<T> where T : ReferenceDataLibrary
    {

        /// <summary>
        /// Backing field for <see cref="RequiredRdl"/>
        /// </summary>
        private SiteReferenceDataLibrary requiredRdl;

        /// <summary>
        /// Backing field for <see cref="RequiredRdlShortName"/>
        /// </summary>
        private string requiredRdlShortName;

        /// <summary>
        /// Backing field for <see cref="RequiredRdlName"/>
        /// </summary>
        private string requiredRdlName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataLibraryRowViewModel{T}"/> class
        /// </summary>
        /// <param name="referenceDataLibrary">The <see cref="ReferenceDataLibrary"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected ReferenceDataLibraryRowViewModel(T referenceDataLibrary, ISession session, IViewModelBase<Thing> containerViewModel) : base(referenceDataLibrary, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the RequiredRdl
        /// </summary>
        public SiteReferenceDataLibrary RequiredRdl
        {
            get { return this.requiredRdl; }
            set { this.RaiseAndSetIfChanged(ref this.requiredRdl, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="RequiredRdl"/>
        /// </summary>
        public string RequiredRdlShortName
        {
            get { return this.requiredRdlShortName; }
            set { this.RaiseAndSetIfChanged(ref this.requiredRdlShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="RequiredRdl"/>
        /// </summary>
        public string RequiredRdlName
        {
            get { return this.requiredRdlName; }
            set { this.RaiseAndSetIfChanged(ref this.requiredRdlName, value); }
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
            this.RequiredRdl = this.Thing.RequiredRdl;
        }
    }
}
