// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterBaseRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
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
    /// Row class representing a <see cref="ParameterBase"/>
    /// </summary>
    public abstract partial class ParameterBaseRowViewModel<T> : RowViewModelBase<T> where T : ParameterBase
    {

        /// <summary>
        /// Backing field for <see cref="IsOptionDependent"/>
        /// </summary>
        private bool isOptionDependent;

        /// <summary>
        /// Backing field for <see cref="ParameterType"/>
        /// </summary>
        private ParameterType parameterType;

        /// <summary>
        /// Backing field for <see cref="ParameterTypeShortName"/>
        /// </summary>
        private string parameterTypeShortName;

        /// <summary>
        /// Backing field for <see cref="ParameterTypeName"/>
        /// </summary>
        private string parameterTypeName;

        /// <summary>
        /// Backing field for <see cref="Scale"/>
        /// </summary>
        private MeasurementScale scale;

        /// <summary>
        /// Backing field for <see cref="ScaleShortName"/>
        /// </summary>
        private string scaleShortName;

        /// <summary>
        /// Backing field for <see cref="ScaleName"/>
        /// </summary>
        private string scaleName;

        /// <summary>
        /// Backing field for <see cref="StateDependence"/>
        /// </summary>
        private ActualFiniteStateList stateDependence;

        /// <summary>
        /// Backing field for <see cref="Group"/>
        /// </summary>
        private ParameterGroup group;

        /// <summary>
        /// Backing field for <see cref="Owner"/>
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Backing field for <see cref="OwnerShortName"/>
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Backing field for <see cref="OwnerName"/>
        /// </summary>
        private string ownerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterBaseRowViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterBase">The <see cref="ParameterBase"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected ParameterBaseRowViewModel(T parameterBase, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameterBase, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the IsOptionDependent
        /// </summary>
        public bool IsOptionDependent
        {
            get { return this.isOptionDependent; }
            set { this.RaiseAndSetIfChanged(ref this.isOptionDependent, value); }
        }

        /// <summary>
        /// Gets or sets the ParameterType
        /// </summary>
        public ParameterType ParameterType
        {
            get { return this.parameterType; }
            set { this.RaiseAndSetIfChanged(ref this.parameterType, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ParameterType"/>
        /// </summary>
        public string ParameterTypeShortName
        {
            get { return this.parameterTypeShortName; }
            set { this.RaiseAndSetIfChanged(ref this.parameterTypeShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ParameterType"/>
        /// </summary>
        public string ParameterTypeName
        {
            get { return this.parameterTypeName; }
            set { this.RaiseAndSetIfChanged(ref this.parameterTypeName, value); }
        }

        /// <summary>
        /// Gets or sets the Scale
        /// </summary>
        public MeasurementScale Scale
        {
            get { return this.scale; }
            set { this.RaiseAndSetIfChanged(ref this.scale, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Scale"/>
        /// </summary>
        public string ScaleShortName
        {
            get { return this.scaleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.scaleShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Scale"/>
        /// </summary>
        public string ScaleName
        {
            get { return this.scaleName; }
            set { this.RaiseAndSetIfChanged(ref this.scaleName, value); }
        }

        /// <summary>
        /// Gets or sets the StateDependence
        /// </summary>
        public ActualFiniteStateList StateDependence
        {
            get { return this.stateDependence; }
            set { this.RaiseAndSetIfChanged(ref this.stateDependence, value); }
        }

        /// <summary>
        /// Gets or sets the Group
        /// </summary>
        public ParameterGroup Group
        {
            get { return this.group; }
            set { this.RaiseAndSetIfChanged(ref this.group, value); }
        }

        /// <summary>
        /// Gets or sets the Owner
        /// </summary>
        public DomainOfExpertise Owner
        {
            get { return this.owner; }
            set { this.RaiseAndSetIfChanged(ref this.owner, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Owner"/>
        /// </summary>
        public string OwnerShortName
        {
            get { return this.ownerShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Owner"/>
        /// </summary>
        public string OwnerName
        {
            get { return this.ownerName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerName, value); }
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
            this.IsOptionDependent = this.Thing.IsOptionDependent;
			if (this.Thing.ParameterType != null)
			{
				this.ParameterTypeShortName = this.Thing.ParameterType.ShortName;
				this.ParameterTypeName = this.Thing.ParameterType.Name;
			}			
            this.ParameterType = this.Thing.ParameterType;
			if (this.Thing.Scale != null)
			{
				this.ScaleShortName = this.Thing.Scale.ShortName;
				this.ScaleName = this.Thing.Scale.Name;
			}			
            this.Scale = this.Thing.Scale;
            this.StateDependence = this.Thing.StateDependence;
            this.Group = this.Thing.Group;
			if (this.Thing.Owner != null)
			{
				this.OwnerShortName = this.Thing.Owner.ShortName;
				this.OwnerName = this.Thing.Owner.Name;
			}			
            this.Owner = this.Thing.Owner;
        }
    }
}
