// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionValuesetExcelRow.cs" company="RHEA System S.A.">
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
    /// The purpose of the <see cref="ParameterSubscriptionValuesetExcelRow"/> is to represent an <see cref="ParameterSubscriptionValueSet"/>
    /// on the Parameter Sheet in Excel
    /// </summary>
    public class ParameterSubscriptionValuesetExcelRow : ExcelRowBase<ParameterSubscriptionValueSet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionValuesetExcelRow"/> class.
        /// </summary>
        /// <param name="parameterSubscriptionValueSet">
        /// The <see cref="ParameterSubscriptionValueSet"/> that is represented by the row.
        /// </param>
        /// <param name="level">
        /// the grouping level of the current row.
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public ParameterSubscriptionValuesetExcelRow(ParameterSubscriptionValueSet parameterSubscriptionValueSet, int level, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
            : base(parameterSubscriptionValueSet)
        {
            this.Level = level;            
            this.Id = this.Thing.Iid.ToString();

            ProcessedValueSet processedValueSet;
            if (processedValueSets.TryGetValue(parameterSubscriptionValueSet.Iid, out processedValueSet))
            {
                parameterSubscriptionValueSet = (ParameterSubscriptionValueSet)processedValueSet.ClonedThing;
            }
            
            var parameterSubscription = parameterSubscriptionValueSet.Container as ParameterSubscription;
            if (parameterSubscription != null)
            {
                this.ParameterType = parameterSubscription.ParameterType;

                if (this.ParameterType is CompoundParameterType)
                {
                    this.Type = ParameterSheetConstants.PSVSCD;
                }
                else
                {
                    this.Type = ParameterSheetConstants.PSVS;
                }

                var spaces = new string(' ', 3 * Math.Abs(level - 1));

                this.ParameterTypeShortName = parameterSubscription.Scale == null ? parameterSubscription.ParameterType.ShortName : string.Format("{0} [{1}]", parameterSubscription.ParameterType.ShortName, parameterSubscription.Scale.ShortName);
                var optionPart = parameterSubscriptionValueSet.ActualOption != null ? parameterSubscriptionValueSet.ActualOption.ShortName : string.Empty;
                var statePart = parameterSubscriptionValueSet.ActualState != null ? parameterSubscriptionValueSet.ActualState.ShortName : string.Empty;

                this.Name = string.Format("{0}{1}\\{2}\\{3}", spaces, parameterSubscription.ParameterType.Name, optionPart, statePart);
                this.ShortName = string.Format("{0}\\{1}\\{2}", parameterSubscription.ParameterType.ShortName, optionPart, statePart);
                this.Format = NumberFormat.Format(parameterSubscription.ParameterType, parameterSubscription.Scale);
                this.Switch = parameterSubscriptionValueSet.ValueSwitch.ToString();
                this.Owner = string.Format("--{0}", PropertyHelper.ComputeContainerOwnerShortName(parameterSubscription));

                this.ModelCode = parameterSubscriptionValueSet.ModelCode();

                var compoundParameterType = parameterSubscription.ParameterType as CompoundParameterType;
                if (compoundParameterType == null)
                {
                    this.ActualValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterSubscriptionValueSet.ActualValue[0]);
                    this.ComputedValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterSubscriptionValueSet.Computed[0]);
                    this.ManualValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterSubscriptionValueSet.Manual[0]);
                    this.ReferenceValue = ParameterValueConverter.ConvertToObject(this.ParameterType, parameterSubscriptionValueSet.Reference[0]);
                    this.Formula = string.Empty;
                }
                else
                {
                    // create rows for each component
                    foreach (ParameterTypeComponent component in compoundParameterType.Component)
                    {
                        var componentExcelRow = new ComponentExcelRow(component, parameterSubscriptionValueSet, this.Level + 1);
                        this.ContainedRows.Add(componentExcelRow);
                        componentExcelRow.Container = this;
                    }
                }
            }
        }
    }
}
