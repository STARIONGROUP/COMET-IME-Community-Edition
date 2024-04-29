// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateDependenceRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    /// <summary>
    /// Represents an element parameter to use in <see cref="ParameterRowControlViewModel" /> in the grapher
    /// </summary>
    public class StateDependenceRowViewModel : ParameterRowControlViewModel
    {
        /// <summary>
        /// Initializes a new instance of <see cref="StateDependenceRowViewModel" />
        /// </summary>
        /// <param name="actualFiniteStates">The <see cref="ContainerList{T}" /> of <see cref="ActualFiniteStates" /></param>
        /// <param name="valueSets">The <see cref="IEnumerable{T}" /> of <see cref="IValueSet" /></param>
        public StateDependenceRowViewModel(ContainerList<ActualFiniteState> actualFiniteStates, IEnumerable<IValueSet> valueSets)
        {
            this.ActualFiniteStates = actualFiniteStates;
            this.ValueSets = valueSets;
        }

        /// <summary>
        /// Gets or sets the <see cref="ContainerList{T}" /> of <see cref="ActualFiniteStates" />
        /// </summary>
        public ContainerList<ActualFiniteState> ActualFiniteStates { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IEnumerable{T}" /> of <see cref="IValueSet" />
        /// </summary>
        public IEnumerable<IValueSet> ValueSets { get; private set; }

        /// <summary>
        /// Generates a list of <see cref="ParameterRowControlViewModel" />
        /// </summary>
        /// <returns>A <see cref="List{T}" /> of <see cref="ParameterRowControlViewModel" /></returns>
        public List<ParameterRowControlViewModel> GenerateStateRows()
        {
            var rowList = new List<ParameterRowControlViewModel>();

            foreach (var state in this.ActualFiniteStates)
            {
                var row = this.GenerateViewModelRow(state);
                rowList.Add(row);
            }

            return rowList;
        }

        /// <summary>
        /// Generates a row of <see cref="ParameterRowControlViewModel" /> based on the current <see cref="ActualFiniteState" />
        /// </summary>
        /// <param name="actualFiniteState">The current <see cref="ActualFiniteState" /></param>
        /// <returns></returns>
        private ParameterRowControlViewModel GenerateViewModelRow(ActualFiniteState actualFiniteState)
        {
            var valueSet = this.GetValueSet(actualFiniteState.Iid);
            var actualValue = "-";
            var publishedValue = "-";

            if (valueSet is ParameterValueSet parameterValueSet)
            {
                actualValue = this.FormatValueString(parameterValueSet.ActualValue);
                publishedValue = this.FormatValueString(parameterValueSet.Published);
            }

            var row = new ParameterRowControlViewModel
            {
                Name = actualFiniteState.Name,
                ShortName = actualFiniteState.ShortName,
                OwnerShortName = actualFiniteState.Owner?.ShortName,
                RowType = actualFiniteState.ClassKind.ToString(),
                Switch = valueSet.ValueSwitch.ToString(),
                ActualValue = actualValue,
                PublishedValue = publishedValue
            };

            return row;
        }

        /// <summary>
        /// Gets the matching <see cref="IValueSet" /> for a given <see cref="ActualFiniteState" /> Iid
        /// </summary>
        /// <param name="iid">The <see cref="Guid" /> for the current <see cref="ActualFiniteState" /></param>
        /// <returns></returns>
        private IValueSet GetValueSet(Guid iid)
        {
            var valueSet = this.ValueSets.FirstOrDefault(x => x.ActualState.Iid == iid);
            return valueSet;
        }
    }
}
