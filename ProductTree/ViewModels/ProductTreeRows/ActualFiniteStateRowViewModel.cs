// ------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The row-view-model representing an <see cref="ActualFiniteState"/> 
    /// </summary>
    /// <remarks>
    /// This row shall be used when a <see cref="Parameter"/> is state dependent
    /// </remarks>
    public class ActualFiniteStateRowViewModel : CDP4CommonView.ActualFiniteStateRowViewModel, IModelCodeRowViewModel
    {
        /// <summary>
        /// backing field for <see cref="Value"/> property.
        /// </summary>
        private string value;

        /// <summary>
        /// Backing field for <see cref="Switch"/> property.
        /// </summary>
        private ParameterSwitchKind switchValue;

        /// <summary>
        /// Backing field for <see cref="IsPublishable"/> property.
        /// </summary>
        private bool isPublishable;

        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Backing field for the <see cref="OwnerShortName"/> property.
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateRowViewModel"/> class
        /// </summary>
        /// <param name="actualFiniteState">The <see cref="ActualFiniteState"/> represented</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ActualFiniteStateRowViewModel(ActualFiniteState actualFiniteState, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(actualFiniteState, session, containerViewModel)
        {
            this.IsPublishable = false;
            this.IsDefault = actualFiniteState.IsDefault;

            var parameterOrOverrideBaseRowViewModel = containerViewModel as ParameterOrOverrideBaseRowViewModel;
            if (parameterOrOverrideBaseRowViewModel != null)
            {
                this.OwnerShortName = parameterOrOverrideBaseRowViewModel.OwnerShortName;
            }
        }

        /// <summary>
        /// Gets the model-code
        /// </summary>
        public string ModelCode
        {
            get { return this.modelCode; }
            private set { this.RaiseAndSetIfChanged(ref this.modelCode, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current represented <see cref="ParameterValueSetBase"/> is publishable
        /// </summary>
        public bool IsPublishable
        {
            get { return this.isPublishable; }
            set { this.RaiseAndSetIfChanged(ref this.isPublishable, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ActualFiniteState"/> associated with this row is the default value of the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.RaiseAndSetIfChanged(ref this.isDefault, value); }
        }
        /// <summary>
        /// Gets or sets the <see cref="ParameterSwitchKind"/>
        /// </summary>
        public ParameterSwitchKind Switch
        {
            get { return this.switchValue; }
            set { this.RaiseAndSetIfChanged(ref this.switchValue, value); }
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set { this.RaiseAndSetIfChanged(ref this.value, value); }
        }

        /// <summary>
        /// Gets or sets the short name of the <see cref="DomainOfExpertise"/> that owns the container
        /// <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        public string OwnerShortName
        {
            get { return this.ownerShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerShortName, value); }
        }

        /// <summary>
        /// Set the value for this row from the <see cref="ParameterOrOverrideBase"/> and the associated <see cref="ParameterValueSetBase"/>
        /// </summary>
        /// <param name="parameterOrOveride">The <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="valueSet">The <see cref="ParameterValueSetBase"/></param>
        public void SetScalarValue(ParameterOrOverrideBase parameterOrOveride, ParameterValueSetBase valueSet)
        {
            // perform checks to see if this is indeed a scalar value
            if (valueSet.Published.Count() > 1)
            {
                logger.Warn("The value set of Parameter or override {0} is marked as Scalar, yet has multiple values.", this.Thing.Iid);
            }

            this.Value = valueSet.Published.FirstOrDefault();

            // handle zero values returned
            if (this.Value == null)
            {
                logger.Warn("The value set of Parameter or override {0} is marked as Scalar, yet has no values.", this.Thing.Iid);
                this.Value = "-";
            }

            if (parameterOrOveride.Scale != null)
            {
                this.Value += " [" + parameterOrOveride.Scale.ShortName + "]";
            }

            this.Switch = valueSet.ValueSwitch;
            this.ModelCode = valueSet.ModelCode();
        }
    }
}