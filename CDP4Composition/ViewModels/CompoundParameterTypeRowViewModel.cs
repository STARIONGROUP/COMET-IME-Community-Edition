// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompoundParameterTypeRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Represents an element parameter to use within the <see cref="ParameterRowControlViewModel"/>
    /// </summary>
    public class CompoundParameterTypeRowViewModel : ParameterRowControlViewModel
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CompoundParameterTypeRowViewModel" /> used in the parameters tree in the
        /// grapher
        /// </summary>
        /// <param name="parametersList">
        /// The <see cref="SortedList{TKey,TValue}" /> of <see cref="ParameterTypeComponent" />
        /// </param>
        /// <param name="valueSet">The <see cref="ParameterValueSet" /></param>
        public CompoundParameterTypeRowViewModel(SortedList<long, ParameterTypeComponent> parametersList, ParameterValueSetBase valueSet)
        {
            this.ParametersList = parametersList;
            this.ValueSet = valueSet;
        }

        /// <summary>
        /// Gets or sets the value set
        /// </summary>
        public ParameterValueSetBase ValueSet { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="SortedList{TKey,TValue}" /> of parameters
        /// </summary>
        public SortedList<long, ParameterTypeComponent> ParametersList { get; private set; }

        /// <summary>
        /// Generate a list of <see cref="ParameterRowControlViewModel" /> that represents <see cref="CompoundParameterType" />
        /// </summary>
        /// <returns>A <see cref="List"/> of <see cref="ParameterRowControlViewModel"/></returns>
        public List<ParameterRowControlViewModel> GenerateCompoundParameterRowViewModels()
        {
            var rowList = new List<ParameterRowControlViewModel>();

            foreach (var parameterTypeComponent in this.ParametersList)
            {
                var row = this.GenerateCompoundParameterRowViewModel(parameterTypeComponent.Value);
                rowList.Add(row);
            }

            return rowList;
        }

        /// <summary>
        /// Generates a row of <see cref="ParameterRowControlViewModel" />
        /// </summary>
        /// <param name="parameter">
        /// The <see cref="ParameterTypeComponent" /> used to generate a
        /// <see cref="CompoundParameterType" /> row
        /// </param>
        /// <returns><see cref="ParameterRowControlViewModel"/></returns>
        private ParameterRowControlViewModel GenerateCompoundParameterRowViewModel(ParameterTypeComponent parameter)
        {
            var index = parameter.Index;
            var actualValue = "-";
            var publishedValue = "-";

            if (index < this.ValueSet?.ActualValue.Count)
            {
                actualValue = this.ValueSet.ActualValue[index];
            }

            if (index < this.ValueSet?.Published.Count)
            {
                publishedValue = this.ValueSet.Published[index];
            }

            var row = new ParameterRowControlViewModel
            {
                Name = parameter.ParameterType.Name,
                ShortName = parameter.ParameterType.ShortName,
                Switch = this.ValueSet.ValueSwitch.ToString(),
                ActualValue = actualValue,
                PublishedValue = publishedValue,
                OwnerShortName = this.ValueSet?.Owner.ShortName,
                Description = "-",
                RowType = parameter.ClassKind.ToString()
            };

            return row;
        }
    }
}
