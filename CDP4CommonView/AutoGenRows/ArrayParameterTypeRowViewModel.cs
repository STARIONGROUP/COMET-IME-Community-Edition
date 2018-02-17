// -------------------------------------------------------------------------------------------------
// <copyright file="ArrayParameterTypeRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="ArrayParameterType"/>
    /// </summary>
    public partial class ArrayParameterTypeRowViewModel : CompoundParameterTypeRowViewModel
    {

        /// <summary>
        /// Backing field for <see cref="IsTensor"/>
        /// </summary>
        private bool isTensor;

        /// <summary>
        /// Backing field for <see cref="HasSingleComponentType"/>
        /// </summary>
        private bool hasSingleComponentType;

        /// <summary>
        /// Backing field for <see cref="Rank"/>
        /// </summary>
        private int rank;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayParameterTypeRowViewModel"/> class
        /// </summary>
        /// <param name="arrayParameterType">The <see cref="ArrayParameterType"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        public ArrayParameterTypeRowViewModel(ArrayParameterType arrayParameterType, ISession session, IViewModelBase<Thing> containerViewModel) : base(arrayParameterType, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the IsTensor
        /// </summary>
        public bool IsTensor
        {
            get { return this.isTensor; }
            set { this.RaiseAndSetIfChanged(ref this.isTensor, value); }
        }

        /// <summary>
        /// Gets or sets the HasSingleComponentType
        /// </summary>
        public bool HasSingleComponentType
        {
            get { return this.hasSingleComponentType; }
            set { this.RaiseAndSetIfChanged(ref this.hasSingleComponentType, value); }
        }

        /// <summary>
        /// Gets or sets the Rank
        /// </summary>
        public int Rank
        {
            get { return this.rank; }
            set { this.RaiseAndSetIfChanged(ref this.rank, value); }
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
            this.IsTensor = ((ArrayParameterType)this.Thing).IsTensor;
            this.HasSingleComponentType = ((ArrayParameterType)this.Thing).HasSingleComponentType;
            this.Rank = ((ArrayParameterType)this.Thing).Rank;
        }
    }
}
