// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterRowViewModel.cs" company="RHEA System S.A.">
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

    /// <summary>
    /// row-view-model that represent a <see cref="Parameter"/> in the product tree
    /// </summary>
    public class ParameterRowViewModel : ParameterOrOverrideBaseRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterRowViewModel"/> class
        /// </summary>
        /// <param name="parameter">The associated <see cref="Parameter"/></param>
        /// <param name="option">The actual <see cref="Option"/></param>
        /// <param name="session">The current <see cref="ISession"/></param>
        /// <param name="containerViewModel">the container view-model that contains this row</param>
        public ParameterRowViewModel(Parameter parameter, Option option, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameter, option, session, containerViewModel)
        {
        }

        /// <summary>
        /// Gets the <see cref="ParameterValueSetBase"/> for an <see cref="Option"/> (if this <see cref="ParameterOrOverrideBase"/> is option dependent) and a <see cref="ActualFiniteState"/> (if it is state dependent)
        /// </summary>
        /// <param name="actualState">The <see cref="ActualFiniteState"/></param>
        /// <param name="actualOption">The <see cref="Option"/></param>
        /// <returns>The <see cref="ParameterValueSetBase"/> if a value is defined for the <see cref="Option"/></returns>
        protected override ParameterValueSetBase GetValueSet(ActualFiniteState actualState = null, Option actualOption = null)
        {
            var isStateDependent = this.StateDependence != null;
            var parameter = (Parameter)this.Thing;
            var valueset = parameter.ValueSet.SingleOrDefault(x => (!isStateDependent || x.ActualState == actualState) && (!this.IsOptionDependent || x.ActualOption == actualOption));
            return valueset;
        }
    }
}