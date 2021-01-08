// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetRowAssembler.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4CrossViewEditor.Assemblers
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4CrossViewEditor.RowModels.CrossviewSheet;

    /// <summary>
    /// The purpose of the <see cref="CrossviewSheetRowAssembler"/> is to assemble a list of <see cref="IExcelRow{T}"/>
    /// that will be written to a workbook.
    /// </summary>
    public class CrossviewSheetRowAssembler
    {
        /// <summary>
        /// Backing field for the <see cref="IExcelRow{T}"/> property
        /// </summary>
        private readonly List<IExcelRow<Thing>> excelRows = new List<IExcelRow<Thing>>();

        /// <summary>
        /// Gets the <see cref="IExcelRow{Thing}"/>s that have been assembled
        /// </summary>
        public IEnumerable<IExcelRow<Thing>> ExcelRows => this.excelRows;

        /// <summary>
        /// Assemble the <see cref="IExcelRow{Thing}"/>s
        /// </summary>
        /// <param name="elementDefinitions">Selected element definition</param>
        public void Assemble(IEnumerable<ElementDefinition> elementDefinitions)
        {
            if (elementDefinitions == null)
            {
                return;
            }

            var definitions = elementDefinitions.ToList();

            if (!definitions.Any())
            {
                return;
            }

            var definitionRows = definitions.Select(elementDefinition => new ElementDefinitionExcelRow(elementDefinition)).ToList();
            var allElementDefinitions = (definitions.FirstOrDefault()?.Container as Iteration)?.Element;

            if (allElementDefinitions == null)
            {
                return;
            }

            foreach (var elementDefinitionExcelRow in definitionRows)
            {
                this.excelRows.Add(elementDefinitionExcelRow);

                foreach (var element in allElementDefinitions)
                {
                    var elementUsage = element.ContainedElement.FirstOrDefault(eu => eu.ElementDefinition == elementDefinitionExcelRow.Thing);

                    if (elementUsage == null)
                    {
                        continue;
                    }

                    var elemenUsageExcelRow = new ElementUsageExcelRow(elementUsage);
                    this.excelRows.Add(elemenUsageExcelRow);
                }
            }
        }
    }
}
