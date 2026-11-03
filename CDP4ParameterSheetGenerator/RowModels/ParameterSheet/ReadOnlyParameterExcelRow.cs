// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReadOnlyParameterExcelRow.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.ParameterSheet
{
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4OfficeInfrastructure;
    using CDP4OfficeInfrastructure.Converter;

    using CDP4ParameterSheetGenerator.RowModels;

    /// <summary>
    /// The purpose of the <see cref="ReadOnlyParameterExcelRow"/> is to represent a <see cref="Parameter"/> or a
    /// <see cref="ParameterOverride"/> that is owned by another <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise"/> than the
    /// one the Parameter sheet is generated for, and for which the current domain does not have a <see cref="ParameterSubscription"/>.
    /// </summary>
    /// <remarks>
    /// These rows are informative only; they are tagged with <see cref="ParameterSheetConstants.RO"/> so that the
    /// <see cref="CDP4ParameterSheetGenerator.ParameterSheet.ParameterSheetProcessor"/> never picks them up as a value that is to be
    /// submitted, and so that the value cells are locked by the <see cref="ParameterArrayAssembler"/>.
    /// </remarks>
    public class ReadOnlyParameterExcelRow : ExcelRowBase<ParameterOrOverrideBase>
    {
        /// <summary>
        /// The level offset of the current row.
        /// </summary>
        private const int LevelOffset = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyParameterExcelRow"/> class.
        /// </summary>
        /// <param name="parameterOrOverride">
        /// The <see cref="Parameter"/> or <see cref="ParameterOverride"/> that is represented by the current row and that is owned
        /// by another <see cref="CDP4Common.SiteDirectoryData.DomainOfExpertise"/>.
        /// </param>
        public ReadOnlyParameterExcelRow(ParameterOrOverrideBase parameterOrOverride)
            : base(parameterOrOverride)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the properties of the <see cref="ReadOnlyParameterExcelRow"/>
        /// </summary>
        private void UpdateProperties()
        {
            this.Id = this.Thing.Iid.ToString();
            this.Name = string.Format("{0}{1}", new string(' ', 3 * LevelOffset), this.Thing.ParameterType.Name);
            this.ShortName = this.Thing.ParameterType.ShortName;
            this.Owner = this.Thing.Owner.ShortName;
            this.Level = LevelOffset;
            this.Type = ParameterSheetConstants.RO;
            this.ParameterType = this.Thing.ParameterType;
            this.ModelCode = this.Thing.ModelCode();

            this.ParameterTypeShortName = this.Thing.Scale == null
                ? this.Thing.ParameterType.ShortName
                : string.Format("{0} [{1}]", this.Thing.ParameterType.ShortName, this.Thing.Scale.ShortName);

            this.Format = NumberFormat.Format(this.Thing.ParameterType, this.Thing.Scale);

            this.UpdateActualValue();
        }

        /// <summary>
        /// Update the <see cref="ExcelRowBase{T}.ActualValue"/> with the published value of the owning domain.
        /// </summary>
        /// <remarks>
        /// ponytail: only the scalar, non option- and non state-dependent case carries a value. For the dependent and
        /// compound cases the row is still emitted - so that the parameter is visible and can be subscribed to - but
        /// without a value. Expand into per-value-set rows if users ask to read those values from the sheet.
        /// </remarks>
        private void UpdateActualValue()
        {
            if (this.Thing.IsOptionDependent || this.Thing.StateDependence != null)
            {
                return;
            }

            if (!(this.Thing.ParameterType is ScalarParameterType))
            {
                return;
            }

            var valueSet = this.Thing.ValueSets.FirstOrDefault();

            if (valueSet == null)
            {
                return;
            }

            this.ActualValue = ParameterValueConverter.ConvertToObject(this.Thing.ParameterType, valueSet.ActualValue[0]);
        }
    }
}
