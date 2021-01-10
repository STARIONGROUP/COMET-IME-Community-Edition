// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveRow.cs" company="RHEA System S.A.">
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
    using System.ComponentModel;

    using CDP4Composition.Services;

    using ReactiveUI;

    /// <summary>
    /// The rows used for ordered primitive type property
    /// </summary>
    /// <typeparam name="T">The type of value</typeparam>
    public class PrimitiveRow<T> : ReactiveObject, IDataErrorInfo
    {
        /// <summary>
        /// backing field for <see cref="Index"/>
        /// </summary>
        private long index;

        /// <summary>
        /// backing field for <see cref="Value"/>
        /// </summary>
        private T value;

        /// <summary>
        /// Gets or sets the index of the value
        /// </summary>
        public long Index
        {
            get { return this.index; }
            set { this.RaiseAndSetIfChanged(ref this.index, value); }
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public T Value
        {
            get { return this.value; }
            set { this.RaiseAndSetIfChanged(ref this.value, value); }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        /// <remarks>
        /// Used by the view through the IDataErrorInfo interface to validate a field
        /// </remarks>
        public string this[string columnName]
        {
            get { return ValidationService.ValidateProperty(columnName, this); }
        }

        /// <summary>
        /// Gets or sets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// Gets or sets an error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error { get; set; }
    }
}
