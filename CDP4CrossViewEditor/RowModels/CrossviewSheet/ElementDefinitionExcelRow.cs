// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionExcelRow.cs" company="RHEA System S.A.">
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
    /// The purpose of the <see cref="ElementDefinitionExcelRow"/> is to represent an <see cref="ElementDefinition"/>
    /// on the Parameter Sheet in Excel
    /// </summary>
    public class ElementDefinitionExcelRow : ExcelRow<ElementDefinition>
    {
        /// <summary>
        /// The level offset of the current row.
        /// </summary>
        /// <remarks>
        /// <see cref="ElementUsage"/>s are always at level 1.
        /// </remarks>
        private const int LevelOffset = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionExcelRow"/> class.
        /// </summary>
        /// <param name="elementDefinition">
        /// The <see cref="ElementDefinition"/> that is represented by the current row
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementDefinition"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public ElementDefinitionExcelRow(ElementDefinition elementDefinition, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
            : base(elementDefinition)
        {
            this.UpdateProperties();

            var sortedParameters = elementDefinition.Parameter.OrderBy(parameter => parameter.ParameterType.Name);
            this.ProcessParameters(sortedParameters, owner, processedValueSets);

            var sortedParameterGroups = elementDefinition.ParameterGroup.Where(pg => pg.ContainingGroup == null);
            this.ProcessParameterGroups(sortedParameterGroups, owner, processedValueSets);

            var sortedElementUSages = elementDefinition.ContainedElement.OrderBy(elementUsage => elementUsage.Name);
            this.ProcessElementUsages(sortedElementUSages, owner, processedValueSets);
        }

        /// <summary>
        /// Update the properties of the <see cref="ExcelRow{T}"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Id = this.Thing.Iid.ToString();
            this.Name = this.Thing.Name;
            this.ShortName = this.Thing.ShortName;
            this.Type = "ED";
            this.Owner = this.Thing.Owner.ShortName;
            this.Level = LevelOffset;
            this.ModelCode = this.Thing.ModelCode();
            this.Categories = this.Thing.GetAllCategoryShortNames();
        }

        /// <summary>
        /// Process the <see cref="Parameter"/>s that are contained by the <see cref="ElementDefinition"/> that is represented by the current excel row
        /// and that are NOT "contained" by a <see cref="ParameterGroup"/>.
        /// </summary>
        /// <param name="parameters">
        /// The <see cref="Parameter"/>s contained by the <see cref="ElementDefinition"/>.
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementDefinition"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessParameters(IEnumerable<Parameter> parameters, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Group != null)
                {
                    continue;
                }

                if (parameter.Owner == owner)
                {
                }
                else
                {
                    var parameterSubscription = parameter.ParameterSubscription.SingleOrDefault(x => x.Owner == owner);
                    if (parameterSubscription != null)
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Process the <see cref="ParameterGroup"/> that are contained by the <see cref="ElementDefinition"/> that is represented by the current excel row
        /// and that are NOT "contained" by a <see cref="ParameterGroup"/>.
        /// </summary>
        /// <param name="parameterGroups">The <see cref="ParameterGroup"/>s contained by the <see cref="ElementDefinition"/>.</param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementDefinition"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessParameterGroups(IEnumerable<ParameterGroup> parameterGroups, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var parameterGroup in parameterGroups)
            {
                //var row = new ParameterGroupExcelRow(parameterGroup, owner, processedValueSets);
                //this.ContainedRows.Add(row);
                //row.Container = this;
            }
        }

        /// <summary>
        /// Process the <see cref="ElementUsage"/>s that are contained by the <see cref="ElementDefinition"/> that is represented by the current excel row
        /// </summary>
        /// <param name="elementUsages">The <see cref="ElementUsage"/>s contained by the <see cref="ElementDefinition"/>.</param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ElementDefinition"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessElementUsages(IEnumerable<ElementUsage> elementUsages, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var elementUsage in elementUsages)
            {
                //var row = new ElementUsageExcelRow(elementUsage, owner, processedValueSets);
                //this.ContainedRows.Add(row);
                //row.Container = this;
            }
        }
    }
}
