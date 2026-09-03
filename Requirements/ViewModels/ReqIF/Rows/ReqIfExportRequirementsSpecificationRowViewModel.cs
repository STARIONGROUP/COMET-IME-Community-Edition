// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportRequirementsSpecificationRowViewModel.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.ViewModels
{
    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    /// <summary>
    /// The <see cref="RequirementsSpecification"/> row in the ReqIF export view-model. It allows the user
    /// to select which <see cref="RequirementsSpecification"/>s are included in the export.
    /// </summary>
    public class ReqIfExportRequirementsSpecificationRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="IsSelected"/>
        /// </summary>
        private bool isSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfExportRequirementsSpecificationRowViewModel"/> class
        /// </summary>
        /// <param name="requirementsSpecification">The <see cref="RequirementsSpecification"/> represented</param>
        /// <param name="isSelected">A value indicating whether the <see cref="RequirementsSpecification"/> is selected for export. Defaults to true.</param>
        public ReqIfExportRequirementsSpecificationRowViewModel(RequirementsSpecification requirementsSpecification, bool isSelected = true)
        {
            this.RequirementsSpecification = requirementsSpecification;
            this.Name = requirementsSpecification.Name;
            this.ShortName = requirementsSpecification.ShortName;
            this.IsSelected = isSelected;
        }

        /// <summary>
        /// Gets the <see cref="RequirementsSpecification"/> represented by this row
        /// </summary>
        public RequirementsSpecification RequirementsSpecification { get; }

        /// <summary>
        /// Gets the name of the <see cref="RequirementsSpecification"/>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the short-name of the <see cref="RequirementsSpecification"/>
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="RequirementsSpecification"/> is selected for export
        /// </summary>
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.RaiseAndSetIfChanged(ref this.isSelected, value);
        }
    }
}
