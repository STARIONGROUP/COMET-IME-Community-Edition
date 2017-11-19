// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideValueSetExcelRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Validation;
    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.Converter;

    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
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
