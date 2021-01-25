// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetConstants.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor.Generator
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Provides access to constants that define the structure of the Parameter Sheet
    /// </summary>
    internal static class CrossviewSheetConstants
    {
        /// <summary>
        /// Hardcoded power related parameter type short names list
        /// </summary>
        public static readonly string[] PowerParameters = { "redundancy", "P_on", "P_stby", "P_peak", "P_duty_cyc", "P_mean" };

        /// <summary>
        /// Initial fixed columns
        /// </summary>
        internal const int FixedColumns = 5;

        /// <summary>
        /// The number of header nested "layers":
        /// ParameterType -> ParamterTypeComponent -> MeasurementScale -> Option -> ActualFiniteStateList -> ActualFiniteState.
        /// </summary>
        internal const int HeaderDepth = 6;

        /// <summary>
        /// The name of the range on the Parameters sheet that contains the header rows.
        /// </summary>
        internal const string HeaderName = "Header";

        /// <summary>
        /// The name of the range on the Parameters sheet that contains the parameter and value-set rows.
        /// </summary>
        internal const string RangeName = "Crossview";

        /// <summary>
        /// The name of the worksheet that contains the parameter and value-set rows.
        /// </summary>
        internal const string CrossviewSheetName = "Crossview";

        /// <summary>
        /// The name of the <see cref="ElementDefinition"/> row type
        /// </summary>
        internal const string ED = "ED";

        /// <summary>
        /// The name of the <see cref="ElementUsage"/> row type
        /// </summary>
        internal const string EU = "EU";

        /// <summary>
        /// Header date format
        /// </summary>
        internal const string HeaderDateFormat = "yyyy-mm-dd hh:mm:ss";

        /// <summary>
        /// Header column names info
        /// </summary>
        internal static readonly string[] HeaderColumnNames = { "Engineering Model", "Iteration number", "Study Phase", "Domain", "User", "Rebuild Date" };
    }
}
