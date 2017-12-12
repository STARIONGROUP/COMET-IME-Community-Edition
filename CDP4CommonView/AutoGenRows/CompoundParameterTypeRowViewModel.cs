// -------------------------------------------------------------------------------------------------
// <copyright file="CompoundParameterTypeRowViewModel.cs" company="RHEA S.A.">
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
    /// Row class representing a <see cref="CompoundParameterType"/>
    /// </summary>
    public partial class CompoundParameterTypeRowViewModel : ParameterTypeRowViewModel<CompoundParameterType>
    {

        /// <summary>
        /// Backing field for <see cref="IsFinalized"/>
        /// </summary>
        private bool isFinalized;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundParameterTypeRowViewModel"/> class
        /// </summary>
        /// <param name="compoundParameterType">The <see cref="CompoundParameterType"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public CompoundParameterTypeRowViewModel(CompoundParameterType compoundParameterType, ISession session, IViewModelBase<Thing> containerViewModel) : base(compoundParameterType, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the IsFinalized
        /// </summary>
        public bool IsFinalized
        {
            get { return this.isFinalized; }
            set { this.RaiseAndSetIfChanged(ref this.isFinalized, value); }
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
            this.IsFinalized = this.Thing.IsFinalized;
        }
    }
}
