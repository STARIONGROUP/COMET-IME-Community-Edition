// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetConstants.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Provides access to constants that define the structure of the Parameter Sheet
    /// </summary>
    internal static class ParameterSheetConstants
    {
        /// <summary>
        /// The number of the Name column
        /// </summary>
        internal const int NameColumn = 1;

        /// <summary>
        /// The number of the ShortName column
        /// </summary>
        internal const int ShortNameColumn = 2;

        /// <summary>
        /// The number of the Computed value column
        /// </summary>
        internal const int ComputedColumn = 3;

        /// <summary>
        /// The number of the Manual value column
        /// </summary>
        internal const int ManualColumn = 4;

        /// <summary>
        /// The number of the Reference value column
        /// </summary>
        internal const int ReferenceColumn = 5;

        /// <summary>
        /// The number of the Switch column
        /// </summary>
        internal const int SwitchColumn = 6;

        /// <summary>
        /// The number of the Actual value column
        /// </summary>
        internal const int ActualValueColumn = 7;

        /// <summary>
        /// The number of the Model code column
        /// </summary>
        internal const int ModelCodeColumn = 8;

        /// <summary>
        /// The number of the Parameter type column
        /// </summary>
        internal const int ParameterTypeColumn = 9;

        /// <summary>
        /// The number of the Owner column
        /// </summary>
        internal const int OwnerColumn = 10;

        /// <summary>
        /// The number of the Category column
        /// </summary>
        internal const int CategoryColumn = 11;

        /// <summary>
        /// The number of the Type column
        /// </summary>
        internal const int TypeColumn = 12;

        /// <summary>
        /// The number of the Unique ID column
        /// </summary>
        internal const int IdColumn = 13;

        /// <summary>
        /// The name of the range on the Parameters sheet that contains the header rows.
        /// </summary>
        internal const string ParameterHeaderName = "Header";

        /// <summary>
        /// The name of the range on the Parameters sheet that contains the parameter and value-set rows.
        /// </summary>
        internal const string ParameterRangeName = "Parameters";

        /// <summary>
        /// The name of the worksheet that contains the parameter and value-set rows.
        /// </summary>
        internal const string ParameterSheetName = "Parameters";

        /// <summary>
        /// The color code for a cell whose content has changed since last updated by the application.
        /// </summary>
        internal const int CellHasChangedColor = 1;

        /// <summary>
        /// The color code for a cell whose content is invalid.
        /// </summary>
        internal const int CellContentIsInvalidColor = 1;

        /// <summary>
        /// The color code for a cell whose content is not within the range of the <see cref="MeasurementScale"/> that is applicable
        /// to the parameter associated with the cell.
        /// </summary>
        internal const int CellContentIsOutOfScaleRange = 1;

        /// <summary>
        /// The name of the <see cref="ElementDefinition"/> row type
        /// </summary>
        internal const string ED = "ED";

        /// <summary>
        /// The name of the <see cref="ElementUsage"/> row type
        /// </summary>
        internal const string EU = "EU";

        /// <summary>
        /// The name of the <see cref="ParameterGroup"/> row type
        /// </summary>
        internal const string PG = "PG";

        /// <summary>
        /// The name of the <see cref="ParameterValueSet"/> row type
        /// </summary>
        internal const string PVS = "PVS";

        /// <summary>
        /// The name of the <see cref="ParameterValueSet"/> row type for <see cref="SampledFunctionParameterType"/>
        /// </summary>
        internal const string SFPVS = "SFPVS";

        /// <summary>
        /// The name of the compound <see cref="ParameterValueSet"/> row type
        /// </summary>
        internal const string PVSCD = "PVS:CD";

        /// <summary>
        /// The name of the component <see cref="ParameterValueSet"/> row type
        /// </summary>
        internal const string PVSCT = "PVS:CT";

        /// <summary>
        /// The name of the <see cref="ParameterOverrideValueSet"/> row type
        /// </summary>
        internal const string POVS = "POVS";

        /// <summary>
        /// The name of the <see cref="ParameterOverrideValueSet"/> row type for <see cref="SampledFunctionParameterType"/>
        /// </summary>
        internal const string SFPOVS = "SFPOVS";

        /// <summary>
        /// The name of the compound <see cref="ParameterOverrideValueSet"/> row type
        /// </summary>
        internal const string POVSCD = "POVS:CD";

        /// <summary>
        /// The name of the component <see cref="ParameterOverrideValueSet"/> row type
        /// </summary>
        internal const string POVSCT = "POVS:CT";

        /// <summary>
        /// The name of the <see cref="ParameterSubscriptionValueSet"/> row type
        /// </summary>
        internal const string PSVS = "PSVS";

        /// <summary>
        /// The name of the <see cref="ParameterSubscriptionValueSet"/> row type for <see cref="SampledFunctionParameterType"/>
        /// </summary>
        internal const string SFPSVS = "SFPSVS";

        /// <summary>
        /// The name of the compound <see cref="ParameterSubscriptionValueSet"/> row type
        /// </summary>
        internal const string PSVSCD = "PSVS:CD";

        /// <summary>
        /// The name of the component <see cref="ParameterSubscriptionValueSet"/> row type
        /// </summary>
        internal const string PSVSCT = "PSVS:CT";
    }
}
