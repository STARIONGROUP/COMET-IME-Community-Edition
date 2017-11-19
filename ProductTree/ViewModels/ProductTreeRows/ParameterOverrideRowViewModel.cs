// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// row-view-model that represent a <see cref="ParameterOverride"/> in the product tree
    /// </summary>
    public class ParameterOverrideRowViewModel : ParameterOrOverrideBaseRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideRowViewModel"/> class
        /// </summary>
        /// <param name="parameterOverride">The associated <see cref="ParameterOverride"/></param>
        /// <param name="option">The actual <see cref="Option"/></param>
        /// <param name="session">The current <see cref="ISession"/></param>
        /// <param name="containerViewModel">the container view-model that contains this row</param>
        public ParameterOverrideRowViewModel(ParameterOverride parameterOverride, Option option, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterOverride, option, session, containerViewModel)
        {
        }

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();
            var parameterOverride = (ParameterOverride)this.Thing;
            var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterOverride.Parameter)
                       .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                       .ObserveOn(RxApp.MainThreadScheduler)
                       .Subscribe(x => this.UpdateProperties());
            this.Disposables.Add(listener);
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
            var parameterOverride = (ParameterOverride)this.Thing;
            var valueset = parameterOverride.ValueSet.SingleOrDefault(x => (!isStateDependent || x.ActualState == actualState) && (!this.IsOptionDependent || x.ActualOption == actualOption));
            return valueset;
        }
    }
}