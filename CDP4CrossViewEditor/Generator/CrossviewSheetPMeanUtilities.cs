// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossviewSheetPMeanUtilities.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4CrossViewEditor.Assemblers;

    /// <summary>
    /// Helper class that is required for computing value of the mean power parameter
    /// </summary>
    internal static class CrossviewSheetPMeanUtilities
    {
        /// <summary>
        /// Redundancy parameter type short name
        /// </summary>
        private const string Redundancy = "redundancy";

        /// <summary>
        /// Redundancy Scheme parameter type short name
        /// </summary>
        public const string RedundancyScheme = "scheme";

        /// <summary>
        /// Redundancy Type parameter type short name
        /// </summary>
        public const string RedundancyType = "type";

        /// <summary>
        /// Redundancy K parameter type short name
        /// </summary>
        public const string RedundancyK = "k";

        /// <summary>
        /// Redundancy N parameter type short name
        /// </summary>
        public const string RedundancyN = "n";

        /// <summary>
        /// Power on parameter type short name
        /// </summary>
        private const string POn = "p_on";

        /// <summary>
        /// Power stand by cycle parameter type short name
        /// </summary>
        private const string PStandBy = "p_stby";

        /// <summary>
        /// Power peak parameter type short name
        /// </summary>
        private const string PPeak = "p_peak";

        /// <summary>
        /// Power duty cycle parameter type short name
        /// </summary>
        private const string PDutyCycle = "p_duty_cyc";

        /// <summary>
        /// Power mean parameter type short name
        /// </summary>
        private const string PMean = "p_mean";

        /// <summary>
        /// Hardcoded power related parameter type short names list
        /// </summary>
        public static readonly string[] PowerParameters = { Redundancy, POn, PStandBy, PPeak, PDutyCycle, PMean };

        /// <summary>
        /// Hardcoded redundancy.scheme
        /// NOTE: not sure about this value
        /// </summary>
        private const string RedundancySchemeHot = "active";

        /// <summary>
        /// Hardcoded redundancy.scheme
        /// NOTE: not sure about this value
        /// </summary>
        private const string RedundancySchemeCold = "passive";

        /// <summary>
        /// Hardcoded redundancy.type
        /// </summary>
        private const string RedundancyTypeInternal = "internal";

        /// <summary>
        /// Hardcoded redundancy.type
        /// </summary>
        private const string RedundancyTypeExternal = "external";

        /// <summary>
        /// Calculate mean power parameter value based on multiple parameter values
        /// </summary>
        /// <param name="crossviewArrayAssembler">
        /// The <see cref="CrossviewArrayAssembler"/>
        /// </param>
        /// <param name="contentRow">
        /// The current Excel row
        /// </param>
        /// <param name="parameters">
        /// The <see cref="ParameterOrOverrideBase"/>s present on the <see cref="contentRow"/>
        /// </param>
        internal static void ComputePMean(CrossviewArrayAssembler crossviewArrayAssembler, object[] contentRow, List<ParameterOrOverrideBase> parameters)
        {
            ParameterOrOverrideBase redundancy = null, pOn = null, pStandBy = null, pDutyCycle = null, pMean = null;

            foreach (var parameterOrOverrideBase in parameters)
            {
                switch (parameterOrOverrideBase.ParameterType.ShortName.ToLowerInvariant())
                {
                    case Redundancy:
                        redundancy = parameterOrOverrideBase;
                        break;

                    case POn:
                        pOn = parameterOrOverrideBase;
                        break;

                    case PStandBy:
                        pStandBy = parameterOrOverrideBase;
                        break;

                    case PDutyCycle:
                        pDutyCycle = parameterOrOverrideBase;
                        break;

                    case PMean:
                        pMean = parameterOrOverrideBase;
                        break;
                }
            }

            // ensure all required parameters are present
            if (redundancy == null || pOn == null || pStandBy == null || pDutyCycle == null || pMean == null)
            {
                return;
            }

            // ensure all required parameters have the same option dependency
            if (redundancy.IsOptionDependent != pOn.IsOptionDependent ||
                pOn.IsOptionDependent != pStandBy.IsOptionDependent ||
                pStandBy.IsOptionDependent != pDutyCycle.IsOptionDependent ||
                pDutyCycle.IsOptionDependent != pMean.IsOptionDependent)
            {
                return;
            }

            // ensure redundancy, p_on, and p_stby are not state dependent
            if (redundancy.StateDependence != null || pOn.StateDependence != null || pStandBy.StateDependence != null)
            {
                return;
            }

            // ensure redundancy has required components
            if (!(redundancy.ParameterType is CompoundParameterType))
            {
                return;
            }

            var compoundRedundancy = (CompoundParameterType)redundancy.ParameterType;

            var redundancySchemeComponent = compoundRedundancy.Component
                .FirstOrDefault(x => x.ShortName.Equals(RedundancyScheme, StringComparison.InvariantCultureIgnoreCase));
            if (redundancySchemeComponent == null)
            {
                return;
            }
            
            var redundancyTypeComponent = compoundRedundancy.Component
                .FirstOrDefault(x => x.ShortName.Equals(RedundancyType, StringComparison.InvariantCultureIgnoreCase));
            if (redundancyTypeComponent == null)
            {
                return;
            }

            var redundancyKComponent = compoundRedundancy.Component
                .FirstOrDefault(x => x.ShortName.Equals(RedundancyK, StringComparison.InvariantCultureIgnoreCase));
            if (redundancyKComponent == null)
            {
                return;
            }

            var redundancyNComponent = compoundRedundancy.Component
                .FirstOrDefault(x => x.ShortName.Equals(RedundancyN, StringComparison.InvariantCultureIgnoreCase));
            if (redundancyNComponent == null)
            {
                return;
            }

            // ensure p_duty_cyc and p_mean are state dependent
            if (pDutyCycle.StateDependence == null || pMean.StateDependence == null)
            {
                return;
            }

            // ensure p_duty_cyc and p_mean have the same state dependency
            if (pDutyCycle.StateDependence != pMean.StateDependence)
            {
                return;
            }

            foreach (var option in redundancy.ValueSets.Select(x => x.ActualOption))
            {
                var redundancyValueSet = redundancy.ValueSets.First(x => x.ActualOption == option) as ParameterValueSetBase;
                var pOnValueSet = pOn.ValueSets.First(x => x.ActualOption == option) as ParameterValueSetBase;
                var pStandByValueSet = pStandBy.ValueSets.First(x => x.ActualOption == option) as ParameterValueSetBase;

                var pDutyCycleValueSets = pDutyCycle.ValueSets.Where(x => x.ActualOption == option);
                var pMeanValueSets = pMean.ValueSets.Where(x => x.ActualOption == option);

                foreach (var state in pDutyCycleValueSets.Select(x => x.ActualState))
                {
                    var pDutyCycleValueSet = pDutyCycleValueSets.First(x => x.ActualState == state) as ParameterValueSetBase;
                    var pMeanValueSet = pMeanValueSets.First(x => x.ActualState == state) as ParameterValueSetBase;

                    contentRow[crossviewArrayAssembler.GetContentColumnIndex(pMeanValueSet)] = ComputePMean(
                        contentRow[crossviewArrayAssembler.GetContentColumnIndex(redundancyValueSet, redundancySchemeComponent)] as string,
                        contentRow[crossviewArrayAssembler.GetContentColumnIndex(redundancyValueSet, redundancyTypeComponent)] as string,
                        contentRow[crossviewArrayAssembler.GetContentColumnIndex(redundancyValueSet, redundancyKComponent)] as string,
                        contentRow[crossviewArrayAssembler.GetContentColumnIndex(redundancyValueSet, redundancyNComponent)] as string,
                        contentRow[crossviewArrayAssembler.GetContentColumnIndex(pOnValueSet)] as string,
                        contentRow[crossviewArrayAssembler.GetContentColumnIndex(pStandByValueSet)] as string,
                        contentRow[crossviewArrayAssembler.GetContentColumnIndex(pDutyCycleValueSet)] as string);
                }
            }
        }

        /// <summary>
        /// Calculate mean power parameter value based on multiple parameter values
        /// </summary>
        /// <param name="standByValue">
        /// standBy power value
        /// </param>
        /// <param name="onValue">
        /// on power value
        /// </param>
        /// <param name="dutyCycleValue">
        /// duty cycle power value
        /// </param>
        /// <param name="redundancyScheme">
        /// redundancy scheme
        /// </param>
        /// <param name="redundancyType">
        /// redundancy type
        /// </param>
        /// <param name="redundancyK">
        /// redundancy k
        /// </param>
        /// <param name="redundancyN">
        /// redundancy n
        /// </param>
        /// <returns>
        /// Computed value for the mean power
        /// </returns>
        private static double? ComputePMean(
            string redundancyScheme,
            string redundancyType,
            string redundancyK,
            string redundancyN,
            string onValue,
            string standByValue,
            string dutyCycleValue)
        {
            if (
                !int.TryParse(redundancyK, NumberStyles.Any, CultureInfo.InvariantCulture, out var pRedundancyKValue) ||
                !int.TryParse(redundancyN, NumberStyles.Any, CultureInfo.InvariantCulture, out var pRedundancyNValue) ||
                !double.TryParse(onValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var pOnValue) ||
                !double.TryParse(standByValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var pStandByValue) ||
                !double.TryParse(dutyCycleValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var pDutyCycleValue))
            {
                return null;
            }

            if (pDutyCycleValue == -1)
            {
                return 0;
            }

            if (pDutyCycleValue < 0 || pDutyCycleValue > 1)
            {
                return null;
            }

            switch (redundancyType.ToLowerInvariant())
            {
                case RedundancyTypeInternal:
                    return pDutyCycleValue * pOnValue + (1 - pDutyCycleValue) * pStandByValue;

                case RedundancyTypeExternal:
                    switch (redundancyScheme.ToLowerInvariant())
                    {
                        case RedundancySchemeCold:
                            return (pDutyCycleValue * pOnValue + (1 - pDutyCycleValue) * pStandByValue) * pRedundancyKValue / pRedundancyNValue;

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
