// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueSetBaseRowViewModel.cs" company="Starion Group S.A.">
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
    /// Row class representing a <see cref="ParameterValueSetBase"/>
    /// </summary>
    public abstract partial class ParameterValueSetBaseRowViewModel<T> : RowViewModelBase<T> where T : ParameterValueSetBase
    {

        /// <summary>
        /// Backing field for <see cref="ValueSwitch"/>
        /// </summary>
        private ParameterSwitchKind valueSwitch;

        /// <summary>
        /// Backing field for <see cref="ActualState"/>
        /// </summary>
        private ActualFiniteState actualState;

        /// <summary>
        /// Backing field for <see cref="ActualOption"/>
        /// </summary>
        private Option actualOption;

        /// <summary>
        /// Backing field for <see cref="ActualOptionShortName"/>
        /// </summary>
        private string actualOptionShortName;

        /// <summary>
        /// Backing field for <see cref="ActualOptionName"/>
        /// </summary>
        private string actualOptionName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValueSetBaseRowViewModel{T}"/> class
        /// </summary>
        /// <param name="parameterValueSetBase">The <see cref="ParameterValueSetBase"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The <see cref="IViewModelBase{Thing}"/> that is the container of this <see cref="IRowViewModelBase{Thing}"/></param>
        protected ParameterValueSetBaseRowViewModel(T parameterValueSetBase, ISession session, IViewModelBase<Thing> containerViewModel) : base(parameterValueSetBase, session, containerViewModel)
        {
            this.UpdateProperties();
        }


        /// <summary>
        /// Gets or sets the ValueSwitch
        /// </summary>
        public ParameterSwitchKind ValueSwitch
        {
            get { return this.valueSwitch; }
            set { this.RaiseAndSetIfChanged(ref this.valueSwitch, value); }
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
        /// Gets or sets the ActualOption
        /// </summary>
        public Option ActualOption
        {
            get { return this.actualOption; }
            set { this.RaiseAndSetIfChanged(ref this.actualOption, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ActualOption"/>
        /// </summary>
        public string ActualOptionShortName
        {
            get { return this.actualOptionShortName; }
            set { this.RaiseAndSetIfChanged(ref this.actualOptionShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ActualOption"/>
        /// </summary>
        public string ActualOptionName
        {
            get { return this.actualOptionName; }
            set { this.RaiseAndSetIfChanged(ref this.actualOptionName, value); }
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
            this.ValueSwitch = this.Thing.ValueSwitch;
            this.ActualState = this.Thing.ActualState;
			if (this.Thing.ActualOption != null)
			{
				this.ActualOptionShortName = this.Thing.ActualOption.ShortName;
				this.ActualOptionName = this.Thing.ActualOption.Name;
			}			
            this.ActualOption = this.Thing.ActualOption;
        }
    }
}
