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
    public class ActualFiniteStateRowViewModel : CDP4CommonView.ParameterBaseRowViewModel<ParameterBase>, IModelCodeRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsPublishable"/> property.
        /// </summary>
        private bool isPublishable;

        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Backing field for <see cref="ActualState"/>
        /// </summary>
        private ActualFiniteState actualState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateRowViewModel"/> class
        /// </summary>
        /// <param name="actualFiniteState">The <see cref="ActualFiniteState"/> represented</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ActualFiniteStateRowViewModel(ParameterBase parameterBase, ActualFiniteState actualState, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterBase, session, containerViewModel)
        {
            this.IsPublishable = false;
            this.actualState = actualState;
            this.IsDefault = this.actualState.IsDefault;
            this.Name = this.actualState.Name;

            //var parameterOrOverrideBaseRowViewModel = containerViewModel as ParameterOrOverrideBaseRowViewModel;
            //if (parameterOrOverrideBaseRowViewModel != null)
            //{
            //    this.OwnerShortName = parameterOrOverrideBaseRowViewModel.OwnerShortName;
            //}
        }

        /// <summary>
        /// Gets or sets a value of <see cref="ActualFiniteState"/>
        /// </summary>
        public ActualFiniteState ActualState
        {
            get { return this.actualState; }
            private set { this.RaiseAndSetIfChanged(ref this.actualState, value); }
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