// -------------------------------------------------------------------------------------------------
// <copyright file="NestedParameterRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="NestedParameter"/>
    /// </summary>
    public partial class NestedParameterRowViewModel : RowViewModelBase<NestedParameter>
    {

        /// <summary>
        /// Backing field for <see cref="Path"/>
        /// </summary>
        private string path;

        /// <summary>
        /// Backing field for <see cref="Formula"/>
        /// </summary>
        private string formula;

        /// <summary>
        /// Backing field for <see cref="ActualValue"/>
        /// </summary>
        private string actualValue;

        /// <summary>
        /// Backing field for <see cref="IsVolatile"/>
        /// </summary>
        private bool isVolatile;

        /// <summary>
        /// Backing field for <see cref="AssociatedParameter"/>
        /// </summary>
        private ParameterBase associatedParameter;

        /// <summary>
        /// Backing field for <see cref="ActualState"/>
        /// </summary>
        private ActualFiniteState actualState;

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
        /// Initializes a new instance of the <see cref="NestedParameterRowViewModel"/> class
        /// </summary>
        /// <param name="nestedParameter">The <see cref="NestedParameter"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public NestedParameterRowViewModel(NestedParameter nestedParameter, ISession session, IViewModelBase<Thing> containerViewModel) : base(nestedParameter, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        public string Path
        {
            get { return this.path; }
            set { this.RaiseAndSetIfChanged(ref this.path, value); }
        }

        /// <summary>
        /// Gets or sets the Formula
        /// </summary>
        public string Formula
        {
            get { return this.formula; }
            set { this.RaiseAndSetIfChanged(ref this.formula, value); }
        }

        /// <summary>
        /// Gets or sets the ActualValue
        /// </summary>
        public string ActualValue
        {
            get { return this.actualValue; }
            set { this.RaiseAndSetIfChanged(ref this.actualValue, value); }
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
        /// Gets or sets the AssociatedParameter
        /// </summary>
        public ParameterBase AssociatedParameter
        {
            get { return this.associatedParameter; }
            set { this.RaiseAndSetIfChanged(ref this.associatedParameter, value); }
        }

        /// <summary>
        /// Gets or sets the ActualState
        /// </summary>
        public ActualFiniteState ActualState
        {
            get { return this.actualState; }
            set { this.RaiseAndSetIfChanged(ref this.actualState, value); }
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
            this.Path = this.Thing.Path;
            this.Formula = this.Thing.Formula;
            this.ActualValue = this.Thing.ActualValue;
            this.IsVolatile = this.Thing.IsVolatile;
            this.AssociatedParameter = this.Thing.AssociatedParameter;
            this.ActualState = this.Thing.ActualState;
            this.Owner = this.Thing.Owner;
        }
    }
}
