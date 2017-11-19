// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComponentExcelRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.Converter;
    using CDP4ParameterSheetGenerator.RowModels;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="ComponentExcelRow"/> is to represent the value of a <see cref="ParameterTypeComponent"/> of
    /// a <see cref="ParameterValueSet"/>, a <see cref="ParameterSubscriptionValueSet"/> or a <see cref="ParameterOverrideValueSet"/>
    /// </summary>
    public class ComponentExcelRow : ExcelRowBase<ParameterTypeComponent>  
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentExcelRow"/> class.
        /// </summary>
        /// <param name="parameterTypeComponent">
        /// The <see cref="ParameterTypeComponent"/> that represents the current row
        /// </param>
        /// <param name="parameterValueSet">
        /// The <see cref="ParameterValueSet"/> from which the value is to be represented
        /// </param>
        /// <param name="level">
        /// the grouping level of the current row.
        /// </param>
        public ComponentExcelRow(ParameterTypeComponent parameterTypeComponent, ParameterValueSet parameterValueSet, int level)
            : base(parameterTypeComponent)
        {
            this.Level = level;
            this.Name = string.Format("{0}{1}", new string(' ', 3 * level), parameterTypeComponent.ShortName);
            this.ShortName = parameterTypeComponent.ShortName;
            this.Type = ParameterSheetConstants.PVSCT;
            this.Switch = parameterValueSet.ValueSwitch.ToString();
            this.Id = string.Format("{0}:{1}", parameterValueSet.Iid, parameterTypeComponent.Index);
            this.Owner = parameterValueSet.Owner.ShortName;
            this.ParameterType = parameterTypeComponent.ParameterType;

            this.Format = NumberFormat.Format(parameterTypeComponent.ParameterType, parameterTypeComponent.Scale);

            this.ModelCode = parameterValueSet.ModelCode(parameterTypeComponent.Index);

            if (parameterTypeComponent.Scale == null)
            {
                this.ParameterTypeShortName = parameterTypeComponent.ParameterType.ShortName;
            }
            else
            {
                this.ParameterTypeShortName = string.Format("{0} [{1}]", parameterTypeComponent.ParameterType.ShortName, parameterTypeComponent.Scale.ShortName);
            }

            try
            {
                this.ActualValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterValueSet.ActualValue[parameterTypeComponent.Index]);
                this.ComputedValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterValueSet.Computed[parameterTypeComponent.Index]);
                this.ManualValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterValueSet.Manual[parameterTypeComponent.Index]);
                this.ReferenceValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterValueSet.Reference[parameterTypeComponent.Index]);
                this.Formula = parameterValueSet.Formula[parameterTypeComponent.Index];
            }
            catch (Exception ex)
            {
                this.SetDefaultValues();
                Logger.Debug(ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentExcelRow"/> class.
        /// </summary>
        /// <param name="parameterTypeComponent">
        /// The <see cref="ParameterTypeComponent"/> that represents the current row
        /// </param>
        /// <param name="parameterSubscriptionValueSet">
        /// The <see cref="ParameterSubscriptionValueSet"/> from which the value is to be represented
        /// </param>
        /// <param name="level">
        /// the grouping level of the current row.
        /// </param>
        public ComponentExcelRow(ParameterTypeComponent parameterTypeComponent, ParameterSubscriptionValueSet parameterSubscriptionValueSet, int level)
            : base(parameterTypeComponent)
        {
            this.Level = level;
            this.Name = string.Format("{0}{1}", new string(' ', 3 * level), parameterTypeComponent.ShortName);
            this.ShortName = parameterTypeComponent.ShortName;
            this.Type = ParameterSheetConstants.PSVSCT;
            this.Switch = parameterSubscriptionValueSet.ValueSwitch.ToString();
            this.Id = string.Format("{0}:{1}", parameterSubscriptionValueSet.Iid, parameterTypeComponent.Index);            
            this.Owner =  string.Format("--{0}", PropertyHelper.ComputeContainerOwnerShortName(parameterSubscriptionValueSet)) ;
            this.ParameterType = parameterTypeComponent.ParameterType;

            this.Format = NumberFormat.Format(parameterTypeComponent.ParameterType, parameterTypeComponent.Scale);

            this.ModelCode = parameterSubscriptionValueSet.ModelCode(parameterTypeComponent.Index);

            if (parameterTypeComponent.Scale == null)
            {
                this.ParameterTypeShortName = parameterTypeComponent.ParameterType.ShortName;
            }
            else
            {
                this.ParameterTypeShortName = string.Format("{0} [{1}]", parameterTypeComponent.ParameterType.ShortName, parameterTypeComponent.Scale.ShortName);
            }

            try
            {
                this.ActualValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterSubscriptionValueSet.ActualValue[parameterTypeComponent.Index]);
                this.ComputedValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterSubscriptionValueSet.Computed[parameterTypeComponent.Index]);
                this.ManualValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterSubscriptionValueSet.Manual[parameterTypeComponent.Index]);
                this.ReferenceValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterSubscriptionValueSet.Reference[parameterTypeComponent.Index]);
            }
            catch (Exception ex)
            {
                this.SetDefaultValues();
                Logger.Debug(ex);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentExcelRow"/> class.
        /// </summary>
        /// <param name="parameterTypeComponent">
        /// The <see cref="ParameterTypeComponent"/> that represents the current row
        /// </param>
        /// <param name="parameterOverrideValueSet">
        /// The <see cref="ParameterOverrideValueSet"/> from which the value is to be represented
        /// </param>
        /// <param name="level">
        /// the grouping level of the current row.
        /// </param>
        public ComponentExcelRow(ParameterTypeComponent parameterTypeComponent, ParameterOverrideValueSet parameterOverrideValueSet, int level)
            : base(parameterTypeComponent)
        {
            this.Level = level;
            this.Name = string.Format("{0}{1}", new string(' ', 3 * level), parameterTypeComponent.ShortName);
            this.ShortName = parameterTypeComponent.ShortName;
            this.Type = ParameterSheetConstants.POVSCT;
            this.Switch = parameterOverrideValueSet.ValueSwitch.ToString();
            this.Id = string.Format("{0}:{1}", parameterOverrideValueSet.Iid, parameterTypeComponent.Index);
            this.Owner = parameterOverrideValueSet.Owner.ShortName;
            this.ParameterType = parameterTypeComponent.ParameterType;

            this.Format = NumberFormat.Format(parameterTypeComponent.ParameterType, parameterTypeComponent.Scale);

            if (parameterTypeComponent.Scale == null)
            {
                this.ParameterTypeShortName = parameterTypeComponent.ParameterType.ShortName;
            }
            else
            {
                this.ParameterTypeShortName = string.Format("{0} [{1}]", parameterTypeComponent.ParameterType.ShortName, parameterTypeComponent.Scale.ShortName);
            }

            try
            {
                this.ActualValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterOverrideValueSet.ActualValue[parameterTypeComponent.Index]);
                this.ComputedValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterOverrideValueSet.Computed[parameterTypeComponent.Index]);
                this.ManualValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterOverrideValueSet.Manual[parameterTypeComponent.Index]);
                this.ReferenceValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, parameterOverrideValueSet.Reference[parameterTypeComponent.Index]);
                this.Formula = parameterOverrideValueSet.Formula[parameterTypeComponent.Index];
            }
            catch (Exception ex)
            {
                this.SetDefaultValues();
                Logger.Debug(ex);
            }
        }

        /// <summary>
        /// sets the default value for the Actual-Value, the Computed-Value, the Manual-Value and the Reference-Value
        /// </summary>
        private void SetDefaultValues()
        {
            this.ActualValue = "-";
            this.ComputedValue = "-";
            this.ManualValue = "-";
            this.ReferenceValue = "-";
        }
    }
}
