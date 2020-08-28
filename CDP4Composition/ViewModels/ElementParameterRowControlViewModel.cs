// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementParameterRowControlViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Composition.ViewModels
{
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    /// <summary>
    /// Represents an element to use within the <see cref="ElementParametersControl"/>
    /// </summary>
    public class ElementParameterRowControlViewModel
    {
        /// <summary>
        /// Gets the Parameter Collection of this represented Element
        /// </summary>
        public ReactiveList<ParameterRowControlViewModel> Parameters { get; } = new ReactiveList<ParameterRowControlViewModel>();

        /// <summary>
        /// Gets the Actual Option
        /// </summary>
        public Option ActualOption { get; private set; }

        /// <summary>
        /// Gets the represented element Option
        /// </summary>
        public ElementBase Element { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ElementParameterRowControlViewModel"/>
        /// </summary>
        /// <param name="element">The represented element</param>
        /// <param name="option">The actual option</param>
        public ElementParameterRowControlViewModel(ElementBase element, Option option)
        {
            this.ActualOption = option;
            this.Element = element;
            this.UpdateProperties();
        }
        
        /// <summary>
        /// Update and set all this view model properties
        /// </summary>
        public void UpdateProperties()
        {
            if (this.Element is ElementUsage elementUsage)
            {
                this.Parameters.AddRange(elementUsage.ParameterOverride.Select(p => new ParameterRowControlViewModel(p, this.ActualOption)));
                this.Parameters.AddRange(elementUsage.ElementDefinition.Parameter.Select(p => new ParameterRowControlViewModel(p, this.ActualOption)));
            }
            else if (this.Element is ElementDefinition elementDefinition)
            {
                this.Parameters.AddRange(elementDefinition.Parameter.Select(p => new ParameterRowControlViewModel(p, this.ActualOption)));
            }
        }
    }
}
