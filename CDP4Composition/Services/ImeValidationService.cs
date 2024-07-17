// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImeValidationService.cs" company="Starion Group S.A.">
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
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text.RegularExpressions;

    using CDP4Common.CommonData;
    using CDP4Common.Validation;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;

    /// <summary>
    /// The purpose of the <see cref="ImeValidationService" /> is to check and report on the validity of a field in an object
    /// </summary>
    [Export(typeof(IImeValidationService))]
    public class ImeValidationService : ValidationService, IImeValidationService
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
        public string ValidateProperty<T>(string propertyName, DialogViewModelBase<T> dialogViewModelBase) where T : Thing
        {
            // try to get a primary rule match
            var result = this.ValidationMap.TryGetValue(propertyName, out var rule);

            var property = dialogViewModelBase.GetType().GetProperty(propertyName);

            // if no property exists just return null in sign of ignorance
            if (property == null)
            {
                return null;
            }

            // check for an active override
            var attribute = property.GetCustomAttributes(typeof(ValidationOverrideAttribute), true).Cast<ValidationOverrideAttribute>().SingleOrDefault();

            if (attribute != null)
            {
                if (!attribute.IsValidationEnabled)
                {
                    return null;
                }

                // get the override rule from the table. If found override the rule.
                result = this.ValidationMap.TryGetValue(attribute.ValidationOverrideName, out rule);
            }

            // if no rule exists just return null in sign of ignorance
            if (!result)
            {
                return null;
            }

            // get the value, if the value is null set to empty string (assume user entered no value to begin with) and check against that
            var propertyValue = property.GetValue(dialogViewModelBase) ?? string.Empty;

            var validationPass = Regex.IsMatch(propertyValue.ToString(), rule.Rule);

            if (validationPass)
            {
                if (dialogViewModelBase.ValidationErrors.Contains(rule))
                {
                    // remove rule if it exists in the viewmodel
                    dialogViewModelBase.ValidationErrors.Remove(rule);
                }

                return null;
            }

            if (!dialogViewModelBase.ValidationErrors.Contains(rule))
            {
                dialogViewModelBase.ValidationErrors.Add(rule);
            }

            return rule.ErrorText;
        }

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
        public string ValidateObjectProperty(string propertyName, object instance)
        {
            // try to get a primary rule match
            var property = instance.GetType().GetProperty(propertyName);

            // if no property exists just return null in sign of ignorance
            if (property == null)
            {
                return null;
            }

            // get the value, if the value is null set to empty string (assume user entered no value to begin with) and check against that
            var propertyValue = property.GetValue(instance) ?? string.Empty;
            return this.ValidateProperty(propertyName, propertyValue);
        }
    }
}
