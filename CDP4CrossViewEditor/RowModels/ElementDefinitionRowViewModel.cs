// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Adrian Chivu, Cozmin Velciu, Alex Vorobiev
//
//    This file is part of CDP4-Server-Administration-Tool.
//    The CDP4-Server-Administration-Tool is an ECSS-E-TM-10-25 Compliant tool
//    for advanced server administration.
//
//    The CDP4-Server-Administration-Tool is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as
//    published by the Free Software Foundation; either version 3 of the
//    License, or (at your option) any later version.
//
//    The CDP4-Server-Administration-Tool is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.RowModels
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Row class representing a <see cref="ElementDefinition"/> as a plain object
    /// </summary>
    public class ElementDefinitionRowViewModel : DefinedThingRowViewModel<ElementDefinition>
    {
        /// <summary>
        /// Gets the current <see cref="DomainOfExpertise"/> represented as <see cref="string"/>
        /// </summary>
        public string DomainOfExpertise { private set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRowViewModel"/> class
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/> associated with this row
        /// </param>
        public ElementDefinitionRowViewModel(ElementDefinition elementDefinition)
            : base(elementDefinition)
        {
            this.DomainOfExpertise = elementDefinition.Owner.ShortName;
        }
    }
}
