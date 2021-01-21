// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaveMappingResult.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4ReferenceDataMapper.Managers
{
    /// <summary>
    /// The result of saving parameter to state mappings to a table
    /// </summary>
    public class SaveMappingResult
    {
        /// <summary>
        /// Gets a value indicating whether the data was saved successfully
        /// </summary>
        public bool Result { get; }

        /// <summary>
        /// Gets a value indicating whether changes were necessary.
        /// </summary>
        public bool HasChanges { get; }

        /// <summary>
        /// Gets the error text in case an error occurred during saving parameter to state mappings
        /// </summary>
        public string ErrorText { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SaveMappingResult"/> class
        /// </summary>
        /// <param name="result">Indication whether the data was saved successfully</param>
        /// <param name="hasChanges">Indication whether changes were found</param>
        /// <param name="errorText">The error text in case of an savind was unsuccessfull</param>
        private SaveMappingResult(bool result, bool hasChanges, string errorText = "")
        {
            this.Result = result;
            this.HasChanges = hasChanges;
            this.ErrorText = errorText;
        }

        /// <summary>
        /// Create a <see cref="SaveMappingResult"/> that indicates a successfull save action
        /// </summary>
        /// <returns>The <see cref="SaveMappingResult"/></returns>
        public static SaveMappingResult CreateSuccesResult()
        {
            return new SaveMappingResult(true, true, string.Empty);
        }

        /// <summary>
        /// Create a <see cref="SaveMappingResult"/> that indicates an UNsuccessfull save action
        /// </summary>
        /// <returns>The <see cref="SaveMappingResult"/></returns>
        public static SaveMappingResult CreateErrorResult(string errorText)
        {
            return new SaveMappingResult(false, true, errorText);
        }

        public static SaveMappingResult CreateUnChangedResult()
        {
            return new SaveMappingResult(true, false, string.Empty);
        }
    }
}
