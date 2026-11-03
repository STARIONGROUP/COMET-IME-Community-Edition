// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionExcelRow.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.RowModels.CrossviewSheet
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.Generator;

    /// <summary>
    /// The purpose of the <see cref="ElementDefinitionExcelRow"/> is to represent an <see cref="ElementDefinition"/>
    /// on the Parameter Sheet in Excel
    /// </summary>
    public class ElementDefinitionExcelRow : ExcelRow<ElementDefinition>
    {
        /// <summary>
        /// The default level of an <see cref="ElementDefinition"/> row, used when no depth is supplied.
        /// </summary>
        private const int LevelOffset = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionExcelRow"/> class.
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/> that is represented by the current row</param>
        /// <param name="level">
        /// The depth of the <see cref="ElementDefinition"/> in the product tree, which determines the indentation of the row.
        /// </param>
        public ElementDefinitionExcelRow(ElementDefinition elementDefinition, int level = LevelOffset)
            : base(elementDefinition)
        {
            this.UpdateProperties(level);
        }

        /// <summary>
        /// Update the properties of the <see cref="ExcelRow{T}"/>
        /// </summary>
        /// <param name="level">
        /// The depth of the <see cref="ElementDefinition"/> in the product tree
        /// </param>
        private void UpdateProperties(int level)
        {
            this.Id = this.Thing.Iid.ToString();
            this.Name = $"{new string(' ', 3 * level)}{this.Thing.Name}";
            this.ShortName = $"{new string(' ', 3 * level)}{this.Thing.ShortName}";
            this.Type = CrossviewSheetConstants.ED;
            this.Owner = this.Thing.Owner.ShortName;
            this.Level = level;
            this.ModelCode = this.Thing.ModelCode();
            this.Categories = this.Thing.GetAllCategoryShortNames();
        }
    }
}
