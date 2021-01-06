// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageExcelRow.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor.RowModels.CrossviewSheet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.ViewModels;

    /// <summary>
    /// The purpose of the <see cref="ElementUsageExcelRow"/> is to represent an <see cref="ElementUsage"/>
    /// on the Parameter Sheet in Excel
    /// </summary>
    public class ElementUsageExcelRow : ExcelRowBase<ElementUsage>
    {
        /// <summary>
        /// The level offset of the current row.
        /// </summary>
        /// <remarks>
        /// <see cref="ElementUsage"/>s are always at level 1.
        /// </remarks>
        private const int LevelOffset = 1;

        public string[] ParameterTypes { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageExcelRow"/> class.
        /// </summary>
        /// <param name="elementUsage">
        /// The <see cref="ElementUsage"/> that is represented by the current row
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementUsage"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public ElementUsageExcelRow(ElementUsage elementUsage, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
            : base(elementUsage)
        {
            this.UpdateProperties();

            var sortedOverrides = elementUsage.ParameterOverride.Where(po => po.Owner == owner).OrderBy(po => po.ParameterType.Name);
            this.ProcessParameterOverrides(sortedOverrides, owner, processedValueSets);
        }

        /// <summary>
        /// Update the properties of the <see cref="ExcelRowBase{T}"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Id = this.Thing.Iid.ToString();
            this.Name = $"{new string(' ', 3)}{this.Thing.ShortName} : {this.Thing.ElementDefinition.ShortName}";
            this.Type = "EU";
            this.Owner = this.Thing.Owner.ShortName;
            this.Level = LevelOffset;
            this.ModelCode = this.Thing.ModelCode();

            var categories = this.Thing.ElementDefinition.GetAllCategories().ToList();
            categories.AddRange(this.Thing.GetAllCategories());
            this.Categories = categories.Distinct().Aggregate(string.Empty, (current, cat) => current + " " + cat.ShortName).Trim();
        }

        /// <summary>
        /// Process the <see cref="ParameterOverride"/>s that are contained by the <see cref="ElementUsage"/> that is represented by the current excel row
        /// </summary>
        /// <param name="parameterOverrides">The <see cref="ParameterOverride"/>s contained by the <see cref="ElementUsage"/>.</param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any ParameterOverrides
        /// in the <see cref="ElementUsage"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessParameterOverrides(IEnumerable<ParameterOverride> parameterOverrides, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var parameterOverride in parameterOverrides)
            {
            }
        }
    }
}
