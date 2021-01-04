// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRowViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor.RowModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    /// <summary>
    /// Row class representing a <see cref="ElementDefinition"/> as a plain object
    /// </summary>
    public class ElementDefinitionRowViewModel : CDP4CommonView.ElementDefinitionRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionRowViewModel"/> class.
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/></param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ElementDefinitionRowViewModel(ElementDefinition elementDefinition, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(elementDefinition, session, containerViewModel)
        {
        }

        /// <summary>
        /// Override string for custom display
        /// </summary>
        /// <returns>Custom string appearance</returns>
        public override string ToString()
        {
            return $"{this.Name}({this.Owner.Name})";
        }
    }
}
