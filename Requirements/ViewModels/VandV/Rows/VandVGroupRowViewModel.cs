// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVGroupRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.ViewModels.Rows
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Requirements.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// A row in the VCD browser representing a <see cref="RequirementsGroup"/>, mirroring the grouping of the stock
    /// Requirements browser.
    /// </summary>
    public class VandVGroupRowViewModel : RowViewModelBase<RequirementsGroup>
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Coverage"/>
        /// </summary>
        private string coverage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVGroupRowViewModel"/> class.
        /// </summary>
        /// <param name="group">The <see cref="RequirementsGroup"/>.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{Thing}"/>.</param>
        public VandVGroupRowViewModel(RequirementsGroup group, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(group, session, containerViewModel)
        {
            this.SetProperties();
        }

        /// <summary>Gets the group name.</summary>
        public string Name
        {
            get => this.name;
            private set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>Gets the group short-name.</summary>
        public string ShortName
        {
            get => this.shortName;
            private set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets the roll-up of every requirement below this row: how many are fully verified, and how many V&amp;V
        /// items have failed.
        /// </summary>
        public string Coverage
        {
            get => this.coverage;
            private set => this.RaiseAndSetIfChanged(ref this.coverage, value);
        }

        /// <summary>
        /// Refreshes the roll-up from the requirement rows below this one.
        /// </summary>
        public void RefreshCoverage()
        {
            var requirementRows = RequirementCoverageRowViewModel.QueryRequirementRows(this).ToList();

            this.Coverage = VandVStatusRollUp
                .Sum(requirementRows.Select(x => x.RollUp))
                .ToContainerSummary(requirementRows.Count(x => x.IsVerified), requirementRows.Count);
        }

        /// <summary>
        /// Walks a sub-tree and yields every group row in it, at any depth, deepest first, so a parent's roll-up is
        /// only computed once its nested groups have been.
        /// </summary>
        /// <param name="row">The row to walk.</param>
        /// <returns>The group rows below <paramref name="row"/>.</returns>
        public static IEnumerable<VandVGroupRowViewModel> QueryGroupRows(IHaveContainedRows row)
        {
            foreach (var child in row.ContainedRows)
            {
                foreach (var descendant in QueryGroupRows(child))
                {
                    yield return descendant;
                }

                if (child is VandVGroupRowViewModel groupRow)
                {
                    yield return groupRow;
                }
            }
        }

        /// <summary>
        /// Refreshes the projected properties when the underlying <see cref="RequirementsGroup"/> changes.
        /// </summary>
        protected override void UpdateThingStatus()
        {
            base.UpdateThingStatus();
            this.SetProperties();
        }

        /// <summary>
        /// Updates the projected properties from the underlying <see cref="RequirementsGroup"/>.
        /// </summary>
        private void SetProperties()
        {
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
        }
    }
}
