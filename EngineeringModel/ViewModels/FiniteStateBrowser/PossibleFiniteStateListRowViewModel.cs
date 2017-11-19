// -------------------------------------------------------------------------------------------------
// <copyright file="PossibleFiniteStateListRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;

    /// <summary>
    /// A row representing a <see cref="PossibleFiniteStateList"/>
    /// </summary>
    public class PossibleFiniteStateListRowViewModel : CDP4CommonView.PossibleFiniteStateListRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PossibleFiniteStateListRowViewModel"/> class
        /// </summary>
        /// <param name="possibleFiniteStateList">The <see cref="PossibleFiniteStateList"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public PossibleFiniteStateListRowViewModel(PossibleFiniteStateList possibleFiniteStateList, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(possibleFiniteStateList, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        #region RowBase
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
        /// Updates the properties of this row on the update of the current <see cref="Thing"/>
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
        /// Populate the <see cref="PossibleFiniteState"/> row list
        /// </summary>
        private void PopulateFiniteState()
        {
            var currentPossibleStates = this.ContainedRows.Select(x => (PossibleFiniteState)x.Thing).ToList();
            var updatedPossibleStates = this.Thing.PossibleState.ToList();

            var newPossibleStates = updatedPossibleStates.Except(currentPossibleStates).ToList();
            var oldPossibleStates = currentPossibleStates.Except(updatedPossibleStates).ToList();

            foreach (var statelist in newPossibleStates)
            {
                var row = new PossibleFiniteStateRowViewModel(statelist, this.Session, this);
                row.Index = this.Thing.PossibleState.IndexOf(statelist);
                this.ContainedRows.Add(row);
            }

            foreach (var statelist in oldPossibleStates)
            {
                var row = this.ContainedRows.SingleOrDefault(x => x.Thing == statelist);
                if (row != null)
                {
                    this.ContainedRows.Remove(row);
                }
            }

            var rowsToUpdate = this.ContainedRows.Where(x => this.Thing.PossibleState.Contains(x.Thing)).ToList();
            foreach (PossibleFiniteStateRowViewModel row in rowsToUpdate)
            {
                row.IsDefault = row.Thing.IsDefault;
            }

            // Reorder the list if necessary
            if (!this.Thing.PossibleState.SequenceEqual(this.ContainedRows.Select(x => x.Thing)))
            {
                this.ContainedRows.Sort((c1, c2) => this.Thing.PossibleState.FindIndex(c => c.Iid == c1.Thing.Iid) - this.Thing.PossibleState.FindIndex(c => c.Iid == c2.Thing.Iid));
            }
        }
    }
}