﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupExcelRow.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
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

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.ViewModels;

    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="ParameterGroupExcelRow"/> is to represent an <see cref="ParameterGroup"/>
    /// on the Parameter Sheet in Excel
    /// </summary>
    public class ParameterGroupExcelRow : ExcelRowBase<ParameterGroup>        
    {
        /// <summary>
        /// The level offset of the current row.
        /// </summary>
        /// <remarks>
        /// <see cref="Parameter"/>s are always at level 1.
        /// </remarks>
        private const int LevelOffset = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterGroupExcelRow"/> class.
        /// </summary>
        /// <param name="parameterGroup">
        /// The <see cref="ParameterGroup"/> that is represented by the current row
        /// </param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ParameterGroup"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public ParameterGroupExcelRow(ParameterGroup parameterGroup, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
            : base(parameterGroup)
        {
            this.UpdateProperties();

            var sortedParameters = parameterGroup.ContainedParameter().OrderBy(parameter => parameter.ParameterType.Name);
            this.ProcessParameters(sortedParameters, owner, processedValueSets);

            var sortedGroups = parameterGroup.ContainedGroup().OrderBy(group => group.Name);
            this.ProcessParameterGroups(sortedGroups, owner, processedValueSets);
        }

        /// <summary>
        /// Update the properties of the <see cref="ExcelRowBase{T}"/>
        /// </summary>
        private void UpdateProperties()
        {
            var level = LevelOffset + this.Thing.Level();

            this.Id = this.Thing.Iid.ToString();
            this.Name = string.Format("{0}{1}", new string(' ', 3 * level), this.Thing.Name);
            this.Type = ParameterSheetConstants.PG;
            this.Level = level;
        }

        /// <summary>
        /// Process the <see cref="Parameter"/>s that are virtually contained by the <see cref="ParameterGroup"/> that is represented by the current excel row
        /// </summary>
        /// <param name="parameters">The <see cref="Parameter"/>s that are virtually contained by the <see cref="ParameterGroup"/>.</param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ParameterGroup"/> that is represented by the current row.
        /// </param>
        /// <param name="clones">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessParameters(IEnumerable<Parameter> parameters, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.Owner == owner)
                {
                    var parameterExcelRow = new ParameterExcelRow(parameter, processedValueSets);

                    if (!parameter.IsOptionDependent && parameter.StateDependence == null)
                    {
                        this.ContainedRows.Add(parameterExcelRow);
                        parameterExcelRow.Container = this;
                        continue;
                    }

                    if (parameter.IsOptionDependent || parameter.StateDependence != null)
                    {
                        var containedItems = parameterExcelRow.GetContainedRows();
                        foreach (var containedItem in containedItems)
                        {
                            this.ContainedRows.Add(containedItem);
                            containedItem.Container = this;
                        }

                        continue;
                    }
                }

                if (parameter.Owner != owner)
                {
                    var parameterSubscription = parameter.ParameterSubscription.SingleOrDefault(x => x.Owner == owner);
                    if (parameterSubscription != null)
                    {
                        var parameterSubscriptionExcelRow = new ParameterSubscriptionExcelRow(parameterSubscription, processedValueSets);

                        if (!parameter.IsOptionDependent && parameter.StateDependence == null)
                        {
                            this.ContainedRows.Add(parameterSubscriptionExcelRow);
                            parameterSubscriptionExcelRow.Container = this;
                            continue;
                        }

                        if (parameter.IsOptionDependent || parameter.StateDependence != null)
                        {
                            var containedItems = parameterSubscriptionExcelRow.GetContainedRows();
                            foreach (var containedItem in containedItems)
                            {
                                this.ContainedRows.Add(containedItem);
                                containedItem.Container = this;
                            }

                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Process the <see cref="ParameterGroup"/>s are that virtually contained by the <see cref="ParameterGroup"/> that is represented by the current excel row
        /// </summary>
        /// <param name="parameterGroups">The <see cref="ParameterGroup"/>s that are virtually contained by the <see cref="ParameterGroup"/>.</param>
        /// <param name="owner">
        /// The <see cref="DomainOfExpertise"/> that is the owner of any Parameter, ParameterSubscriptions or ParameterOverrides
        /// in the <see cref="ParameterGroup"/> that is represented by the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessParameterGroups(IEnumerable<ParameterGroup> parameterGroups, DomainOfExpertise owner, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            foreach (var parameterGroup in parameterGroups)
            {
                var row = new ParameterGroupExcelRow(parameterGroup, owner, processedValueSets);
                this.ContainedRows.Add(row);
                row.Container = this;
            }
        }
    }
}
