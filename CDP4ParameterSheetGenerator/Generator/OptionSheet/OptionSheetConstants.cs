// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionSheetConstants.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.OptionSheet
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Provides access to constants that define the structure of the Option Sheet
    /// </summary>
    internal static class OptionSheetConstants
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
        /// The number of the Model code column
        /// </summary>
        internal const int ModelCodeColumn = 3;

        /// <summary>
        /// The number of the Actual value column
        /// </summary>
        internal const int ActualValueColumn = 4;

        /// <summary>
        /// The number of the Parameter type column
        /// </summary>
        internal const int ParameterTypeColumn = 5;

        /// <summary>
        /// The number of the Owner column
        /// </summary>
        internal const int OwnerColumn = 6;

        /// <summary>
        /// The number of the Owner column
        /// </summary>
        internal const int CategoryColumn = 7;

        /// <summary>
        /// The number of the Type column
        /// </summary>
        internal const int TypeColumn = 8;

        /// <summary>
        /// The number of the Unique ID column
        /// </summary>
        internal const int IdColumn = 9;

        /// <summary>
        /// The name of the range on the Parameters sheet that contains the header rows.
        /// </summary>
        internal const string OptionHeaderName = "Header";

        /// <summary>
        /// The name of the <see cref="NestedElement"/> row type
        /// </summary>
        internal const string NE = "NE";

        /// <summary>
        /// The name of the <see cref="NestedParameter"/> row type that represents a <see cref="Parameter"/>
        /// </summary>
        internal const string NP = "NP";

        /// <summary>
        /// The name of the <see cref="NestedParameter"/> row type that represents a <see cref="ParameterOverride"/>
        /// </summary>
        internal const string NPO = "NPO";

        /// <summary>
        /// The name of the <see cref="NestedParameter"/> row type that represents a <see cref="ParameterSubscription"/>
        /// </summary>
        internal const string NPS = "NPS";
    }
}
