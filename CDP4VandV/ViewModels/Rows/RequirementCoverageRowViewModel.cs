// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementCoverageRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.ViewModels.Rows
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4VandV.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// A root row in the VCD browser representing a system <see cref="Requirement"/>, whose child rows are the V&amp;V
    /// items covering it. This is the row the user right-clicks to add a V&amp;V item.
    /// </summary>
    public class RequirementCoverageRowViewModel : RowViewModelBase<Requirement>
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
        /// Initializes a new instance of the <see cref="RequirementCoverageRowViewModel"/> class.
        /// </summary>
        /// <param name="requirement">The system <see cref="Requirement"/>.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{Thing}"/>.</param>
        public RequirementCoverageRowViewModel(Requirement requirement, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(requirement, session, containerViewModel)
        {
            this.SetProperties();
        }

        /// <summary>Gets the requirement name.</summary>
        public string Name
        {
            get => this.name;
            private set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>Gets the requirement short-name.</summary>
        public string ShortName
        {
            get => this.shortName;
            private set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets the coverage summary, how many V&amp;V items cover this requirement and where they stand, or
        /// "Not covered".
        /// </summary>
        public string Coverage
        {
            get => this.coverage;
            private set => this.RaiseAndSetIfChanged(ref this.coverage, value);
        }

        /// <summary>
        /// Gets the status roll-up of the V&amp;V items covering this requirement, so container rows can accumulate it.
        /// </summary>
        public VandVStatusRollUp RollUp { get; private set; } = new VandVStatusRollUp(0, 0, 0);

        /// <summary>
        /// Gets a value indicating whether every V&amp;V item covering this requirement has closed out. An uncovered
        /// requirement is not verified.
        /// </summary>
        public bool IsVerified => this.RollUp.Total > 0 && this.RollUp.Passed == this.RollUp.Total;

        /// <summary>
        /// Refreshes the coverage summary from the current child rows.
        /// </summary>
        public void RefreshCoverage()
        {
            this.RollUp = VandVCoverageQuery.RollUp(this.ContainedRows.OfType<VandVItemRowViewModel>().Select(x => x.Thing));
            this.Coverage = this.RollUp.ToRequirementSummary();
        }

        /// <summary>
        /// Walks a sub-tree and yields every requirement row in it, at any depth, so a specification or group row can
        /// roll up the requirements below it without knowing how they are nested.
        /// </summary>
        /// <param name="row">The row to walk.</param>
        /// <returns>The requirement rows below <paramref name="row"/>.</returns>
        public static IEnumerable<RequirementCoverageRowViewModel> QueryRequirementRows(IHaveContainedRows row)
        {
            foreach (var child in row.ContainedRows)
            {
                if (child is RequirementCoverageRowViewModel requirementRow)
                {
                    yield return requirementRow;
                }
                else
                {
                    foreach (var descendant in QueryRequirementRows(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the projected properties when the underlying <see cref="Requirement"/> changes.
        /// </summary>
        protected override void UpdateThingStatus()
        {
            base.UpdateThingStatus();
            this.SetProperties();
        }

        /// <summary>
        /// Updates the projected properties from the underlying <see cref="Requirement"/>.
        /// </summary>
        private void SetProperties()
        {
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
            this.RefreshCoverage();
        }
    }
}
