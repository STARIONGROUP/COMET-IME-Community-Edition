// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionDependentDataCollector.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.DataCollection
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// This class is a base class for classes that can be used in a Report Script that is <see cref="Option"/> dependent.
    /// It provides commonly used objects to the script editor.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class OptionDependentDataCollector : IterationDependentDataCollector, IOptionDependentDataCollector
    {
        /// <summary>
        /// Backing field for <see cref="SelectedOption"/>
        /// </summary>
        private Option selectedOption;

        /// <summary>
        /// Gets or sets the selected <see cref="Option"/>
        /// </summary>
        public Option SelectedOption
        {
            get => this.selectedOption ?? this.Iteration.DefaultOption ?? this.Iteration.Option.FirstOrDefault();
            private set => this.selectedOption = value;
        }

        /// <summary>
        /// Selects an <see cref="Option"/> and sets the <see cref="SelectedOption"/> property
        /// </summary>
        public void SelectOption()
        {
            if (this.Iteration != null) 
            {
                if (this.Iteration.Option.Count == 1)
                {
                    this.selectedOption = this.Iteration.Option.Single();
                }
                else
                {
                    var thingSelectorDialogService = ServiceLocator.Current.GetInstance<IThingSelectorDialogService>();
                    this.selectedOption = thingSelectorDialogService.SelectThing(this.Iteration.Option, new string[] {"ShortName", "Name"});
                }
            }
        }
    }
}
