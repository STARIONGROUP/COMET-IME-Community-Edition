// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleViolationRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    /// <summary>
    /// A row representing a <see cref="RuleViolation"/>.
    /// </summary>
    /// <remarks>
    /// In addition to the textual <see cref="RuleViolation.Description"/>, this row resolves the
    /// <see cref="RuleViolation.ViolatingThing"/> identifiers to the actual <see cref="Thing"/>s and exposes
    /// them as child rows. This presents the offending model items with their name, short name, owner and type
    /// in dedicated columns, rather than only as identifiers embedded in a single sentence.
    /// </remarks>
    public class RuleViolationRowViewModel : CDP4CommonView.RuleViolationRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleViolationRowViewModel"/> class.
        /// </summary>
        /// <param name="ruleViolation">
        /// The <see cref="RuleViolation"/> that is represented by the current row-view-model.
        /// </param>
        /// <param name="session">
        /// The current active <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The view-model that is the container of the current row-view-model.
        /// </param>
        public RuleViolationRowViewModel(RuleViolation ruleViolation, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(ruleViolation, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row on the update of the current <see cref="Thing"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Tooltip = this.Thing.Description;
            this.PopulateViolatingThings();
        }

        /// <summary>
        /// Update the <see cref="RowViewModelBase{T}.Details"/> so that the full, readable violation text is shown
        /// in the details box at the bottom of the browser, rather than only the type of the selected row.
        /// </summary>
        protected override void UpdateDetails()
        {
            this.Details = this.Thing.Description;
        }

        /// <summary>
        /// Populates the child rows that represent the <see cref="Thing"/>s referenced by
        /// <see cref="RuleViolation.ViolatingThing"/>.
        /// </summary>
        private void PopulateViolatingThings()
        {
            var resolvedThings = this.Thing.ViolatingThing
                .Select(this.ResolveViolatingThing)
                .Where(thing => thing != null)
                .ToList();

            var currentThings = this.ContainedRows.Select(x => x.Thing).ToList();

            var newThings = resolvedThings.Except(currentThings).ToList();
            var oldThings = currentThings.Except(resolvedThings).ToList();

            foreach (var violatingThing in newThings)
            {
                var row = new ViolatingThingRowViewModel(violatingThing, this.Session, this);
                this.ContainedRows.Add(row);
            }

            foreach (var violatingThing in oldThings)
            {
                var row = this.ContainedRows.SingleOrDefault(x => x.Thing == violatingThing);

                if (row != null)
                {
                    this.ContainedRows.RemoveAndDispose(row);
                }
            }
        }

        /// <summary>
        /// Resolves a violating <see cref="Thing"/> identifier to the cached <see cref="Thing"/>.
        /// </summary>
        /// <param name="iid">The <see cref="System.Guid"/> identifier of the violating <see cref="Thing"/>.</param>
        /// <returns>The resolved <see cref="Thing"/>, or null when it cannot be found in the cache.</returns>
        private Thing ResolveViolatingThing(System.Guid iid)
        {
            if (this.Thing.Cache == null)
            {
                return null;
            }

            return this.Thing.Cache.Values
                .Select(lazy => lazy.Value)
                .FirstOrDefault(thing => thing.Iid == iid);
        }
    }
}
