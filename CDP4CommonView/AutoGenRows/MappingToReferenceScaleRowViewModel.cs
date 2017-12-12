// -------------------------------------------------------------------------------------------------
// <copyright file="MappingToReferenceScaleRowViewModel.cs" company="RHEA S.A.">
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
    /// Row class representing a <see cref="MappingToReferenceScale"/>
    /// </summary>
    public partial class MappingToReferenceScaleRowViewModel : RowViewModelBase<MappingToReferenceScale>
    {

        /// <summary>
        /// Backing field for <see cref="ReferenceScaleValue"/>
        /// </summary>
        private ScaleValueDefinition referenceScaleValue;

        /// <summary>
        /// Backing field for <see cref="ReferenceScaleValueShortName"/>
        /// </summary>
        private string referenceScaleValueShortName;

        /// <summary>
        /// Backing field for <see cref="ReferenceScaleValueName"/>
        /// </summary>
        private string referenceScaleValueName;

        /// <summary>
        /// Backing field for <see cref="DependentScaleValue"/>
        /// </summary>
        private ScaleValueDefinition dependentScaleValue;

        /// <summary>
        /// Backing field for <see cref="DependentScaleValueShortName"/>
        /// </summary>
        private string dependentScaleValueShortName;

        /// <summary>
        /// Backing field for <see cref="DependentScaleValueName"/>
        /// </summary>
        private string dependentScaleValueName;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingToReferenceScaleRowViewModel"/> class
        /// </summary>
        /// <param name="mappingToReferenceScale">The <see cref="MappingToReferenceScale"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public MappingToReferenceScaleRowViewModel(MappingToReferenceScale mappingToReferenceScale, ISession session, IViewModelBase<Thing> containerViewModel) : base(mappingToReferenceScale, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the ReferenceScaleValue
        /// </summary>
        public ScaleValueDefinition ReferenceScaleValue
        {
            get { return this.referenceScaleValue; }
            set { this.RaiseAndSetIfChanged(ref this.referenceScaleValue, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ReferenceScaleValue"/>
        /// </summary>
        public string ReferenceScaleValueShortName
        {
            get { return this.referenceScaleValueShortName; }
            set { this.RaiseAndSetIfChanged(ref this.referenceScaleValueShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ReferenceScaleValue"/>
        /// </summary>
        public string ReferenceScaleValueName
        {
            get { return this.referenceScaleValueName; }
            set { this.RaiseAndSetIfChanged(ref this.referenceScaleValueName, value); }
        }

        /// <summary>
        /// Gets or sets the DependentScaleValue
        /// </summary>
        public ScaleValueDefinition DependentScaleValue
        {
            get { return this.dependentScaleValue; }
            set { this.RaiseAndSetIfChanged(ref this.dependentScaleValue, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="DependentScaleValue"/>
        /// </summary>
        public string DependentScaleValueShortName
        {
            get { return this.dependentScaleValueShortName; }
            set { this.RaiseAndSetIfChanged(ref this.dependentScaleValueShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="DependentScaleValue"/>
        /// </summary>
        public string DependentScaleValueName
        {
            get { return this.dependentScaleValueName; }
            set { this.RaiseAndSetIfChanged(ref this.dependentScaleValueName, value); }
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
            this.ReferenceScaleValue = this.Thing.ReferenceScaleValue;
            this.DependentScaleValue = this.Thing.DependentScaleValue;
        }
    }
}
