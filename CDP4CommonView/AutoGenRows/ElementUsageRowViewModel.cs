// -------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="ElementUsage"/>
    /// </summary>
    public partial class ElementUsageRowViewModel : ElementBaseRowViewModel<ElementUsage>
    {

        /// <summary>
        /// Backing field for <see cref="InterfaceEnd"/>
        /// </summary>
        private InterfaceEndKind interfaceEnd;

        /// <summary>
        /// Backing field for <see cref="ElementDefinition"/>
        /// </summary>
        private ElementDefinition elementDefinition;

        /// <summary>
        /// Backing field for <see cref="ElementDefinitionShortName"/>
        /// </summary>
        private string elementDefinitionShortName;

        /// <summary>
        /// Backing field for <see cref="ElementDefinitionName"/>
        /// </summary>
        private string elementDefinitionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageRowViewModel"/> class
        /// </summary>
        /// <param name="elementUsage">The <see cref="ElementUsage"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ElementUsageRowViewModel(ElementUsage elementUsage, ISession session, IViewModelBase<Thing> containerViewModel) : base(elementUsage, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the InterfaceEnd
        /// </summary>
        public InterfaceEndKind InterfaceEnd
        {
            get { return this.interfaceEnd; }
            set { this.RaiseAndSetIfChanged(ref this.interfaceEnd, value); }
        }

        /// <summary>
        /// Gets or sets the ElementDefinition
        /// </summary>
        public ElementDefinition ElementDefinition
        {
            get { return this.elementDefinition; }
            set { this.RaiseAndSetIfChanged(ref this.elementDefinition, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ElementDefinition"/>
        /// </summary>
        public string ElementDefinitionShortName
        {
            get { return this.elementDefinitionShortName; }
            set { this.RaiseAndSetIfChanged(ref this.elementDefinitionShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ElementDefinition"/>
        /// </summary>
        public string ElementDefinitionName
        {
            get { return this.elementDefinitionName; }
            set { this.RaiseAndSetIfChanged(ref this.elementDefinitionName, value); }
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
            this.InterfaceEnd = this.Thing.InterfaceEnd;
            this.ElementDefinition = this.Thing.ElementDefinition;
        }
    }
}
