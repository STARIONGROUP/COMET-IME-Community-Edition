// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using ReactiveUI;

    /// <summary>
    /// The parameter row view model.
    /// </summary>
    public class ParameterOverrideRowViewModel : ParameterOrOverrideBaseRowViewModel
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideRowViewModel"/> class. 
        /// </summary>
        /// <param name="parameterOverride">
        /// The parameter Override.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        /// <param name="containerViewModel">
        /// The container view-model.
        /// </param>
        public ParameterOverrideRowViewModel(ParameterOverride parameterOverride, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterOverride, session, containerViewModel, false)
        {
        }

        #endregion
        /// <summary>
        /// Initiallizes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            var parameterOverride = (ParameterOverride)this.Thing;
            var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(parameterOverride.Parameter)
                       .Where(objectChange => objectChange.EventKind == EventKind.Updated)
                       .ObserveOn(RxApp.MainThreadScheduler)
                       .Subscribe(x => this.ObjectChangeEventHandler(new ObjectChangedEvent(this.Thing)));
            this.Disposables.Add(listener);
        }
    }
}
