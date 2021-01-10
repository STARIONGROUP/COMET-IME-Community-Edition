// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TermRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition.
//    This is an auto-generated class. Any manual changes to this file will be overwritten!
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or (at your option) any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   This is an auto-generated class. Any manual changes on this file will be overwritten!
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using ReactiveUI;

    /// <summary>
    /// Extended hand-coded part for the auto-generated <see cref="TermRowViewModel"/>
    /// </summary>
    public partial class TermRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="DefinitionValue"/>
        /// </summary>
        private string definitionValue;

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        /// <remarks>
        /// The unique definition in for a language
        /// </remarks>
        public string DefinitionValue
        {
            get { return this.definitionValue; }
            set { this.RaiseAndSetIfChanged(ref this.definitionValue, value); }
        }
    }
}
