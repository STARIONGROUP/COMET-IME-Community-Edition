// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportSpecObjectTypeRowViewModel.cs" company="Starion Group S.A.">
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
    /// <summary>
    /// A read-only preview row describing a <c>SpecObjectType</c> that the ReqIF exporter will produce for the
    /// current export selection. It lets the user see which types (and therefore which rules) drive the export
    /// before writing the file.
    /// </summary>
    public class ReqIfExportSpecObjectTypeRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfExportSpecObjectTypeRowViewModel"/> class
        /// </summary>
        /// <param name="name">The name (<c>LONG-NAME</c>) of the <c>SpecObjectType</c> that will be exported</param>
        /// <param name="numberOfObjects">The number of requirements or groups that are mapped to this type</param>
        /// <param name="distinguishingAttributes">
        /// The parameter-derived attributes that distinguish this type from the other types with the same name.
        /// Two types can share a name (e.g. "Requirement") yet be exported separately because they carry different
        /// parameters; this shows which parameters set them apart.
        /// </param>
        public ReqIfExportSpecObjectTypeRowViewModel(string name, int numberOfObjects, string distinguishingAttributes)
        {
            this.Name = name;
            this.NumberOfObjects = numberOfObjects;
            this.DistinguishingAttributes = distinguishingAttributes;
        }

        /// <summary>
        /// Gets the name of the <c>SpecObjectType</c> that will be exported
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the number of requirements or groups that are mapped to this type
        /// </summary>
        public int NumberOfObjects { get; }

        /// <summary>
        /// Gets the parameter-derived attributes that distinguish this type from other types with the same name
        /// </summary>
        public string DistinguishingAttributes { get; }
    }
}
