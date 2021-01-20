// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetPMeanUtility.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Adrian Chivu, Cozmin Velciu, Alex Vorobiev
//
//    This file is part of CDP4-Server-Administration-Tool.
//    The CDP4-Server-Administration-Tool is an ECSS-E-TM-10-25 Compliant tool
//    for advanced server administration.
//
//    The CDP4-Server-Administration-Tool is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as
//    published by the Free Software Foundation; either version 3 of the
//    License, or (at your option) any later version.
//
//    The CDP4-Server-Administration-Tool is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.Generator
{
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Helper class that is required for computing value of P_mean parameter
    /// </summary>
    internal static class CrossviewSheetPMeanUtility
    {
        /// <summary>
        /// Hardcoded P_mean related parameter types
        /// </summary>
        private static readonly string[] RequiredParameters = { "P_stby", "P_on", "P_duty_cyc", "redundancy.scheme", "redundancy.type", "redundancy.k", "redundancy.n", "P_mean" };

        private const string RedundancySchemeHot = "Hot";

        private const string RedundancySchemeCold = "Cold";

        private const string RedundancyTypeInternal = "Internal";

        private const string RedundancyTypeExternal = "External";

        /// <summary>
        /// Check if P_mean parameter calculation is possible
        /// </summary>
        /// <param name="namesObjects">Parameter type names</param>
        /// <param name="rowIndex">Current row index</param>
        /// <returns>True if all power related parameters are present, False otherwise</returns>
        public static bool IsCalculationPossible(object[,] namesObjects, int rowIndex)
        {
            var rowNamesObjects = Enumerable.Range(CrossviewSheetConstants.FixedColumns, namesObjects.GetLength(1) - CrossviewSheetConstants.FixedColumns)
                .Select(x => namesObjects[rowIndex, x])
                .ToArray();

            var countPowerParameters = (from name in rowNamesObjects from parameter in RequiredParameters where name != null && name.ToString().EndsWith(parameter) select name).Count();

            return RequiredParameters.Length == countPowerParameters;
        }

        /// <summary>
        /// Calculate P_mean parameter value based on multiple parameter values:
        /// "P_stby", "P_on", "P_duty_cyc", "redundancy.scheme", "redundancy.type", "redundancy.k", "redundancy.n"
        /// </summary>
        /// <param name="namesObjects">Parameter type names</param>
        /// <param name="valuesObjects">Parameter actual values</param>
        /// <param name="rowIndex">Current row index</param>
        /// <returns>P_mean value <see cref="double"/></returns>
        public static double? ComputeCalculation(object[,] namesObjects, object[,] valuesObjects, int rowIndex)
        {
            var rowNamesObjects = Enumerable.Range(CrossviewSheetConstants.FixedColumns, namesObjects.GetLength(1) - CrossviewSheetConstants.FixedColumns)
                .Select(x => namesObjects[rowIndex, x])
                .ToArray();

            var rowValuesObjects = Enumerable.Range(CrossviewSheetConstants.FixedColumns, valuesObjects.GetLength(1) - CrossviewSheetConstants.FixedColumns)
                .Select(x => valuesObjects[rowIndex, x])
                .ToArray();

            var calculationDictionary = rowNamesObjects.Zip(rowValuesObjects, (k, v) => new { k, v })
                .ToDictionary(key => key.k.ToString(), value => value.v.ToString());

            var keyPowerStandBy = calculationDictionary.Keys.FirstOrDefault(key => key.EndsWith("P_stby"));
            var keyPowerOn = calculationDictionary.Keys.FirstOrDefault(key => key.EndsWith("P_on"));
            var keyPowerDutyCycle = calculationDictionary.Keys.FirstOrDefault(key => key.EndsWith("P_duty_cyc"));
            var keyRedundancyType = calculationDictionary.Keys.FirstOrDefault(key => key.EndsWith("redundancy.type"));
            var keyRedundancyScheme = calculationDictionary.Keys.FirstOrDefault(key => key.EndsWith("redundancy.scheme"));
            var keyRedundancyK = calculationDictionary.Keys.FirstOrDefault(key => key.EndsWith("redundancy.k"));
            var keyRedundancyN = calculationDictionary.Keys.FirstOrDefault(key => key.EndsWith("redundancy.n"));

            if (keyPowerStandBy == null || keyPowerOn == null || keyPowerDutyCycle == null ||
                keyRedundancyType == null || keyRedundancyScheme == null ||
                keyRedundancyK == null || keyRedundancyN == null)
            {
                return null;
            }

            double.TryParse(calculationDictionary[keyPowerStandBy], NumberStyles.Any, CultureInfo.InvariantCulture, out var pStandByValue);
            double.TryParse(calculationDictionary[keyPowerOn], NumberStyles.Any, CultureInfo.InvariantCulture, out var pOnValue);
            double.TryParse(calculationDictionary[keyPowerDutyCycle], NumberStyles.Any, CultureInfo.InvariantCulture, out var pDutyCycleValue);
            double.TryParse(calculationDictionary[keyRedundancyK], NumberStyles.Any, CultureInfo.InvariantCulture, out var pRedundancyKValue);
            double.TryParse(calculationDictionary[keyRedundancyN], NumberStyles.Any, CultureInfo.InvariantCulture, out var pRedundancyNValue);

            if (pDutyCycleValue == -1)
            {
                return 0;
            }

            if (pDutyCycleValue < 0 || pDutyCycleValue > 1)
            {
                return null;
            }

            switch (calculationDictionary[keyRedundancyType])
            {
                case RedundancyTypeInternal:
                    return pDutyCycleValue * pOnValue + (1 - pDutyCycleValue) * pStandByValue;

                case RedundancyTypeExternal:
                    switch (calculationDictionary[keyRedundancyScheme])
                    {
                        case RedundancySchemeCold:
                            return(pDutyCycleValue * pOnValue + (1 - pDutyCycleValue) * pStandByValue) * pRedundancyKValue / pRedundancyNValue;

                        case RedundancySchemeHot:
                            return pDutyCycleValue * pOnValue + (1 - pDutyCycleValue) * pStandByValue;

                        default:
                            return null;
                    }

                default:
                    return null;
            }
        }
    }
}
