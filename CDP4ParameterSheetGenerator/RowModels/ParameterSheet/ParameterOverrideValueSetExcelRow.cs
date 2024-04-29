﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideValueSetExcelRow.cs" company="Starion Group S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.ViewModels;

    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.Converter;

    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="ParameterOverrideValueSetExcelRow"/> is to represent an <see cref="ParameterOverrideValueSet"/>
    /// </summary>
    public class ParameterOverrideValueSetExcelRow : ExcelRowBase<ParameterOverrideValueSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterOverrideValueSetExcelRow"/> class.
        /// </summary>
        /// <param name="parameterOverrideValueSet">
        /// The <see cref="ParameterOverrideValueSet"/> that is represented by the current row.
        /// </param>
        /// <param name="level">
        /// the grouping level of the current row.
        /// </param>
        /// <param name="clones">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public ParameterOverrideValueSetExcelRow(ParameterOverrideValueSet parameterOverrideValueSet, int level, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
            : base(parameterOverrideValueSet)
        {
            this.Level = level;            
            this.Id = this.Thing.Iid.ToString();

            ProcessedValueSet processedValueSet;            
            if (processedValueSets.TryGetValue(parameterOverrideValueSet.Iid, out processedValueSet))
            {
                parameterOverrideValueSet = (ParameterOverrideValueSet)processedValueSet.ClonedThing;
            }

            var parameterOverride = parameterOverrideValueSet.Container as ParameterOverride;
            if (parameterOverride != null)
            {
                this.ParameterType = parameterOverride.ParameterType;

                if (this.ParameterType is CompoundParameterType)
                {
                    this.Type = ParameterSheetConstants.POVSCD;
                }
                else if (this.ParameterType is SampledFunctionParameterType)
                {
                    this.Type = ParameterSheetConstants.SFPOVS;
                }
                else
                {
                    this.Type = ParameterSheetConstants.POVS;
                }

                var spaces = new string(' ', 3 * Math.Abs(level - 1));
                this.ParameterTypeShortName = parameterOverride.Scale == null ? parameterOverride.ParameterType.ShortName : string.Format("{0} [{1}]", parameterOverride.ParameterType.ShortName, parameterOverride.Scale.ShortName);                
                var optionPart = parameterOverrideValueSet.ActualOption != null ? parameterOverrideValueSet.ActualOption.ShortName : string.Empty;
                var statePart = parameterOverrideValueSet.ActualState != null ? parameterOverrideValueSet.ActualState.ShortName : string.Empty;

                this.Name = string.Format("{0}{1}\\{2}\\{3}", spaces, parameterOverride.ParameterType.Name, optionPart, statePart);
                this.ShortName = string.Format("{0}\\{1}\\{2}", parameterOverride.ParameterType.ShortName, optionPart, statePart);
                this.Format = NumberFormat.Format(parameterOverride.ParameterType, parameterOverride.Scale);
                this.Switch = parameterOverrideValueSet.ValueSwitch.ToString();

                this.ModelCode = parameterOverrideValueSet.ModelCode();

                var sampledFunctionParameterType = this.ParameterType as SampledFunctionParameterType;
                if (sampledFunctionParameterType != null)
                {
                    this.Id = parameterOverrideValueSet.Iid.ToString();
                    this.Switch = parameterOverrideValueSet.ValueSwitch.ToString();
                    this.ActualValue = "N/A";
                    this.ComputedValue = "N/A";
                    this.ManualValue = "N/A";
                    this.ReferenceValue = "N/A";
                    this.Formula = string.Empty;

                    this.Format = NumberFormat.Format(this.ParameterType, null);

                    this.ModelCode = parameterOverrideValueSet.ModelCode();

                    // do not create sub rows, the parameter row represents the only valueset that exists
                    return;
                }

                var compoundParameterType = parameterOverride.ParameterType as CompoundParameterType;
                if (compoundParameterType == null)
                {
                    this.ActualValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterOverrideValueSet.ActualValue[0]);
                    this.ComputedValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterOverrideValueSet.Computed[0]);
                    this.ManualValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterOverrideValueSet.Manual[0]);
                    this.ReferenceValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterOverrideValueSet.Reference[0]);
                    this.Formula = parameterOverrideValueSet.Formula[0];
                }
                else
                {
                    // create rows for each component
                    foreach (ParameterTypeComponent component in compoundParameterType.Component)
                    {
                        var componentExcelRow = new ComponentExcelRow(component, parameterOverrideValueSet, this.Level + 1);
                        this.ContainedRows.Add(componentExcelRow);
                        componentExcelRow.Container = this;
                    }
                }
            }
        }
    }
}
