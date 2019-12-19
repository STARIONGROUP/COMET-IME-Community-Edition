// -------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateListRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Extensions;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;

    using DevExpress.XtraPrinting.Native;

    using ReactiveUI;

    /// <summary>
    /// A row representing a <see cref="ActualFiniteStateList"/>
    /// </summary>
    public class ActualFiniteStateListRowViewModel : CDP4CommonView.ActualFiniteStateListRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateListRowViewModel"/> class
        /// </summary>
        /// <param name="actualFiniteStateList">The <see cref="ActualFiniteStateList"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ActualFiniteStateListRowViewModel(ActualFiniteStateList actualFiniteStateList, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(actualFiniteStateList, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        #region RowBase

        /// <summary>
        /// Initializes the subscriptions
        /// </summary>
        protected override void InitializeSubscriptions()
        {
            base.InitializeSubscriptions();

            // subscribe to update on the possible state list
            foreach (var possibleFiniteStateList in this.Thing.PossibleFiniteStateList)
            {
                var listener =
                    CDPMessageBus.Current.Listen<ObjectChangedEvent>(possibleFiniteStateList)
                        .Where(
                            objectChange =>
                            objectChange.EventKind == EventKind.Updated
                            && objectChange.ChangedThing.RevisionNumber > this.RevisionNumber)
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

        #endregion

        /// <summary>
        /// Update the row's properties on update of the current <see cref="Thing"/>
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Owner != null)
            {
                this.OwnerName = this.Owner.Name;
                this.OwnerShortName = this.Owner.ShortName;
            }

            this.PopulateFiniteState();
        }

        /// <summary>
        /// Populate the <see cref="ActualFiniteState"/> row list
        /// </summary>
        private void PopulateFiniteState()
        {
            this.ContainedRows.DisposeAndClear();
            foreach (var state in this.GetOrderedActualFiniteStates())
            {
                var row = new ActualFiniteStateRowViewModel(state, this.Session, this) { IsDefault = state.IsDefault };
                this.ContainedRows.Add(row);
            }
        }

        /// <summary>
        /// Returns the list of ordered actual finite states
        /// </summary>
        /// <remarks>
        /// The order is derived from the order of the <see cref="PossibleFiniteStateList"/>s within the <see cref="ActualFiniteState"/>s 
        /// and the position of each <see cref="PossibleFiniteState"/> inside its container <see cref="PossibleFiniteStateList"/>
        /// </remarks>
        private IEnumerable<ActualFiniteState> GetOrderedActualFiniteStates()
        {
            var actualFiniteStateDictionary = new SortedDictionary<int, ActualFiniteState>();
            var possibleFiniteStateListsSize = this.Thing.PossibleFiniteStateList.SortedItems.Values.Select(x => x.PossibleState.Count).ToList();

            foreach (var actualState in this.Thing.ActualState)
            {
                // The OCDT WSP may return a broken model where the actualState.PossibleState is empty.
                if (actualState.PossibleState.Count == 0)
                {
                    logger.Error("The PossibleState property of the ActualState with iid {0} is empty (The multiplicity is 1..*). The data-source has returned a broken model.", actualState.Iid);
                    break;
                }

                var orderKey = 0;
                foreach (var possibleState in actualState.PossibleState)
                {
                    var power = 1;
                    var containerPossibleFiniteStateList = (PossibleFiniteStateList)possibleState.Container;
                    var position = containerPossibleFiniteStateList.PossibleState.IndexOf(possibleState);

                    for (var i = this.Thing.PossibleFiniteStateList.IndexOf(containerPossibleFiniteStateList) + 1; i < possibleFiniteStateListsSize.Count;  i++)
                    {
                        power = power * possibleFiniteStateListsSize[i];
                    }

                    orderKey += power * position;
                }

                actualFiniteStateDictionary.Add(orderKey, actualState);
            }

            return actualFiniteStateDictionary.Values;
        }
    }
}