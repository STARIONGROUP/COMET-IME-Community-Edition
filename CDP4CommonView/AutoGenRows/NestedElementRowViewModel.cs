// -------------------------------------------------------------------------------------------------
// <copyright file="NestedElementRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="NestedElement"/>
    /// </summary>
    public partial class NestedElementRowViewModel : RowViewModelBase<NestedElement>
    {

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="IsVolatile"/>
        /// </summary>
        private bool isVolatile;

        /// <summary>
        /// Backing field for <see cref="RootElement"/>
        /// </summary>
        private ElementDefinition rootElement;

        /// <summary>
        /// Backing field for <see cref="RootElementShortName"/>
        /// </summary>
        private string rootElementShortName;

        /// <summary>
        /// Backing field for <see cref="RootElementName"/>
        /// </summary>
        private string rootElementName;

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedElementRowViewModel"/> class
        /// </summary>
        /// <param name="nestedElement">The <see cref="NestedElement"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public NestedElementRowViewModel(NestedElement nestedElement, ISession session, IViewModelBase<Thing> containerViewModel) : base(nestedElement, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the ShortName
        /// </summary>
        public string ShortName
        {
            get { return this.shortName; }
            set { this.RaiseAndSetIfChanged(ref this.shortName, value); }
        }

        /// <summary>
        /// Gets or sets the IsVolatile
        /// </summary>
        public bool IsVolatile
        {
            get { return this.isVolatile; }
            set { this.RaiseAndSetIfChanged(ref this.isVolatile, value); }
        }

        /// <summary>
        /// Gets or sets the RootElement
        /// </summary>
        public ElementDefinition RootElement
        {
            get { return this.rootElement; }
            set { this.RaiseAndSetIfChanged(ref this.rootElement, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="RootElement"/>
        /// </summary>
        public string RootElementShortName
        {
            get { return this.rootElementShortName; }
            set { this.RaiseAndSetIfChanged(ref this.rootElementShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="RootElement"/>
        /// </summary>
        public string RootElementName
        {
            get { return this.rootElementName; }
            set { this.RaiseAndSetIfChanged(ref this.rootElementName, value); }
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
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
            this.IsVolatile = this.Thing.IsVolatile;
            this.RootElement = this.Thing.RootElement;
        }
    }
}
