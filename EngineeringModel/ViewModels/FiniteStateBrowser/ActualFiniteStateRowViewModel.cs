// -------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

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
    /// A row representing a <see cref="ActualFiniteState"/>
    /// </summary>
    public class ActualFiniteStateRowViewModel : CDP4CommonView.ActualFiniteStateRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDefault"/>
        /// </summary>
        private bool isDefault;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateRowViewModel"/> class
        /// </summary>
        /// <param name="actualFiniteState">The <see cref="ActualFiniteState"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ActualFiniteStateRowViewModel(ActualFiniteState actualFiniteState, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(actualFiniteState, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PossibleFiniteState"/> is the default value of the <see cref="PossibleFiniteStateList"/>
        /// </summary>
        public bool IsDefault
        {
            get { return this.isDefault; }
            set { this.RaiseAndSetIfChanged(ref this.isDefault, value); }
        }

        /// <summary>
        /// Initializes the subscriptions 
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();
            // subscribe to update on the possible state
            foreach (var possibleFiniteState in this.Thing.PossibleState)
            {
                var listener = CDPMessageBus.Current.Listen<ObjectChangedEvent>(possibleFiniteState)
                .Where(objectChange => objectChange.EventKind == EventKind.Updated && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.ObjectChangeEventHandler);
                this.Disposables.Add(listener);
            }
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.IsDefault = this.Thing.IsDefault;
        }
    }
}