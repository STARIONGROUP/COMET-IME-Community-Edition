// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IImeValidationService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
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

namespace CDP4Composition.Services
{
    using CDP4Common.CommonData;
    using CDP4Common.Validation;

    using CDP4Composition.Mvvm;

    /// <summary>
    /// The purpose of the <see cref="IImeValidationService" /> is to check and report on the validity of a field in an object
    /// </summary>
    public interface IImeValidationService : IValidationService
    {
        /// <summary>
        /// Validates a property of a <see cref="DialogViewModelBase{T}" />.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="dialogViewModelBase">
        /// The dialog view model base.
        /// </param>
        /// <typeparam name="T">
        /// The <see cref="Thing" /> the <see cref="DialogViewModelBase{T}" /> is connected to.
        /// </typeparam>
        /// <returns>
        /// The <see cref="string" /> with the error text.
        /// </returns>
        string ValidateProperty<T>(string propertyName, DialogViewModelBase<T> dialogViewModelBase) where T : Thing;

        /// <summary>
        /// Validates a property of an object.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="instance">
        /// The instance.
        /// </param>
        /// <returns>
        /// The <see cref="string" /> with the error text.
        /// </returns>
        string ValidateObjectProperty(string propertyName, object instance);
    }
}
