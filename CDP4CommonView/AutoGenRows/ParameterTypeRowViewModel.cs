// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeRowViewModel.cs" company="RHEA System S.A.">
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
    /// Row class representing a <see cref="ParameterType"/>
    /// </summary>
    public abstract partial class ParameterTypeRowViewModel<T> : DefinedThingRowViewModel<T> where T : ParameterType
    {

        /// <summary>
        /// Backing field for <see cref="NumberOfValues"/>
        /// </summary>
        private int numberOfValues;

        /// <summary>
        /// Backing field for <see cref="Symbol"/>
        /// </summary>
        private string symbol;

        /// <summary>
        /// Backing field for <see cref="IsDeprecated"/>
        /// </summary>
        private bool isDeprecated;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeRowViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterType">The <see cref="ParameterType"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected ParameterTypeRowViewModel(T parameterType, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameterType, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the NumberOfValues
        /// </summary>
        public int NumberOfValues
        {
            get { return this.numberOfValues; }
            set { this.RaiseAndSetIfChanged(ref this.numberOfValues, value); }
        }

        /// <summary>
        /// Gets or sets the Symbol
        /// </summary>
        public string Symbol
        {
            get { return this.symbol; }
            set { this.RaiseAndSetIfChanged(ref this.symbol, value); }
        }

        /// <summary>
        /// Gets or sets the IsDeprecated
        /// </summary>
        public bool IsDeprecated
        {
            get { return this.isDeprecated; }
            set { this.RaiseAndSetIfChanged(ref this.isDeprecated, value); }
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
            this.NumberOfValues = this.Thing.NumberOfValues;
            this.Symbol = this.Thing.Symbol;
            this.IsDeprecated = this.Thing.IsDeprecated;
        }
    }
}
