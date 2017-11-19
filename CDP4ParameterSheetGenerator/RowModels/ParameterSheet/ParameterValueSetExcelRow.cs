// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueSetExcelRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.Converter;

    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="ParameterValueSetExcelRow"/> is to represent an <see cref="ParameterValueSet"/>
    /// </summary>
    public class ParameterValueSetExcelRow : ExcelRowBase<ParameterValueSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterValueSetExcelRow"/> class.
        /// </summary>
        /// <param name="parameterValueSet">
        /// The <see cref="ParameterValueSet"/> that is represented by the current row.
        /// </param>
        /// <param name="level">
        /// the grouping level of the current row.
        /// </param>
        /// <param name="clones">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public ParameterValueSetExcelRow(ParameterValueSet parameterValueSet, int level, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
            : base(parameterValueSet)
        {
            this.Level = level;            
            this.Id = this.Thing.Iid.ToString();

            ProcessedValueSet processedValueSet;
            if (processedValueSets.TryGetValue(parameterValueSet.Iid, out processedValueSet))
            {
                parameterValueSet = (ParameterValueSet)processedValueSet.ClonedThing;
            }
            
            var parameter = parameterValueSet.Container as Parameter;
            if (parameter != null)
            {
                this.ParameterType = parameter.ParameterType;

                if (this.ParameterType is CompoundParameterType)
                {
                    this.Type = ParameterSheetConstants.PVSCD;
                }
                else
                {
                    this.Type = ParameterSheetConstants.PVS;
                }

                var spaces = new string(' ', 3 * Math.Abs(level-1));
                this.ParameterTypeShortName = parameter.Scale == null ? parameter.ParameterType.ShortName : string.Format("{0} [{1}]", parameter.ParameterType.ShortName, parameter.Scale.ShortName);
                var optionPart = parameterValueSet.ActualOption != null ? parameterValueSet.ActualOption.ShortName : string.Empty;
                var statePart = parameterValueSet.ActualState != null ? parameterValueSet.ActualState.ShortName : string.Empty;

                this.Name = string.Format("{0}{1}\\{2}\\{3}", spaces, parameter.ParameterType.Name, optionPart, statePart);
                this.ShortName = string.Format("{0}\\{1}\\{2}", parameter.ParameterType.ShortName, optionPart, statePart);
                this.Format = NumberFormat.Format(parameter.ParameterType, parameter.Scale);
                this.Switch = parameterValueSet.ValueSwitch.ToString();

                this.ModelCode = parameterValueSet.ModelCode();

                var compoundParameterType = parameter.ParameterType as CompoundParameterType;
                if (compoundParameterType == null)
                {
                    this.ActualValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterValueSet.ActualValue[0]);
                    this.ComputedValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterValueSet.Computed[0]);
                    this.ManualValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterValueSet.Manual[0]);
                    this.ReferenceValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterValueSet.Reference[0]);
                    this.Formula = parameterValueSet.Formula[0];
                }
                else
                {
                    // create rows for each component
                    foreach (ParameterTypeComponent component in compoundParameterType.Component)
                    {
                        var componentExcelRow = new ComponentExcelRow(component, parameterValueSet, this.Level + 1);
                        this.ContainedRows.Add(componentExcelRow);
                        componentExcelRow.Container = this;
                    }
                }
            }
        }
    }
}
