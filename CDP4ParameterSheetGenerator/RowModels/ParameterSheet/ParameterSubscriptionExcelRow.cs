// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionExcelRow.cs" company="RHEA System S.A.">
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

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.ViewModels;

    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.Converter;

    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="ParameterSubscriptionExcelRow"/> is to represent an <see cref="ParameterSubscription"/>
    /// on the Parameter Sheet in Excel
    /// </summary>
    public class ParameterSubscriptionExcelRow : ExcelRowBase<ParameterSubscription>
    {
        /// <summary>
        /// The level offset of the current row.
        /// </summary>
        /// <remarks>
        /// <see cref="Parameter"/>s are always at level 1.
        /// </remarks>
        private const int LevelOffset = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterSubscriptionExcelRow"/> class.
        /// </summary>
        /// <param name="parameterSubscription">
        /// The <see cref="ParameterSubscription"/> that is represented by the current row
        /// </param>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        public ParameterSubscriptionExcelRow(ParameterSubscription parameterSubscription, IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
            : base(parameterSubscription)
        {
            this.UpdateProperties();
            this.ProcessParameter(processedValueSets);
        }

        /// <summary>
        /// Update the properties of the <see cref="ParameterSubscriptionExcelRow"/>
        /// </summary>
        private void UpdateProperties()
        {
            var level = LevelOffset + this.Thing.Level();
            this.Id = this.Thing.Iid.ToString();
            this.Name = string.Format("{0}{1}", new string(' ', 3 * level), this.Thing.ParameterType.Name);
            this.ShortName = this.Thing.ParameterType.ShortName;
            this.Owner = string.Format("--{0}", PropertyHelper.ComputeContainerOwnerShortName(this.Thing));
            this.Level = level;
            this.ParameterType = this.Thing.ParameterType;

            if (this.ParameterType is CompoundParameterType)
            {
                this.Type = ParameterSheetConstants.PSVSCD;
            }
            else
            {
                this.Type = ParameterSheetConstants.PSVS;
            }

            this.ParameterTypeShortName = this.Thing.Scale == null ? this.Thing.ParameterType.ShortName : string.Format("{0} [{1}]", this.Thing.ParameterType.ShortName, this.Thing.Scale.ShortName);
        }
        
        /// <summary>
        /// Process the current <see cref="ParameterSubscription"/>s to create any ContainedRows for value-sets
        /// </summary>
        /// <param name="processedValueSets">
        /// The <see cref="Thing"/>s for which the values need to be restored to what the user provided.
        /// </param>
        private void ProcessParameter(IReadOnlyDictionary<Guid, ProcessedValueSet> processedValueSets)
        {
            if (!this.Thing.IsOptionDependent && this.Thing.StateDependence == null)
            {
                var scalarParameterType = this.Thing.ParameterType as ScalarParameterType;
                if (scalarParameterType != null)
                {
                    var parameterValueSet = this.Thing.ValueSet.FirstOrDefault();
                    if (parameterValueSet != null)
                    {
                        ProcessedValueSet processedValueSet;                        
                        if (processedValueSets.TryGetValue(parameterValueSet.Iid, out processedValueSet))
                        {
                            parameterValueSet = (ParameterSubscriptionValueSet)processedValueSet.ClonedThing;
                        }

                        this.Id = parameterValueSet.Iid.ToString();
                        this.Switch = parameterValueSet.ValueSwitch.ToString();
                        this.ActualValue = ParameterValueConverter.ConvertToObject(scalarParameterType, parameterValueSet.ActualValue[0]);
                        this.ComputedValue = ParameterValueConverter.ConvertToObject(scalarParameterType, parameterValueSet.Computed[0]);
                        this.ManualValue = ParameterValueConverter.ConvertToObject(scalarParameterType, parameterValueSet.Manual[0]);
                        this.ReferenceValue = ParameterValueConverter.ConvertToObject(scalarParameterType, parameterValueSet.Reference[0]);
                        
                        this.Format = NumberFormat.Format(this.Thing.ParameterType, this.Thing.Scale);

                        this.ModelCode = parameterValueSet.ModelCode();
                    }

                    // do not create sub rows, the parameter row represents the only valueset that exists                    
                    return;
                }

                var compoundParameterType = this.Thing.ParameterType as CompoundParameterType;
                if (compoundParameterType != null)
                {
                    this.Format = NumberFormat.Format(this.Thing.ParameterType, this.Thing.Scale);
                    this.ModelCode = this.Thing.ModelCode();

                    var parameterValueSet = this.Thing.ValueSet.FirstOrDefault();
                    if (parameterValueSet != null)
                    {
                        ProcessedValueSet processedValueSet;
                        if (processedValueSets.TryGetValue(parameterValueSet.Iid, out processedValueSet))
                        {
                            parameterValueSet = (ParameterSubscriptionValueSet)processedValueSet.ClonedThing;
                        }

                        this.Id = parameterValueSet.Iid.ToString();
                        this.Switch = parameterValueSet.ValueSwitch.ToString();
                    }

                    // create rows for each component
                    foreach (ParameterTypeComponent component in compoundParameterType.Component)
                    {
                        var componentExcelRow = new ComponentExcelRow(component, parameterValueSet, this.Level + 1);
                        this.ContainedRows.Add(componentExcelRow);
                        componentExcelRow.Container = this;
                    }
                }

                return;
            }

            var sortedvaluesets = this.Thing.ValueSet.OrderBy(p => p, new ParameterSubscriptionValueSetComparer());
            foreach (var valueset in sortedvaluesets)
            {
                if (valueset.ActualState != null && valueset.ActualState.Kind == ActualFiniteStateKind.FORBIDDEN)
                {
                    continue;
                }

                var valueSetRow = new ParameterSubscriptionValuesetExcelRow(valueset, this.Level + 1, processedValueSets);
                this.ContainedRows.Add(valueSetRow);
                valueSetRow.Container = this;
            }
        }
    }
}
