// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImeValidationService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
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
    using System;
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
        /// The rule that requires a non-empty value that neither starts nor ends with whitespace.
        /// </summary>
        private const string NoSurroundingWhitespaceRule = @"^\S([\s\S]*\S)?$";

        /// <summary>
        /// The rule that requires a non-empty value that neither starts nor ends with whitespace and that does not start with a parenthesis.
        /// </summary>
        private const string NoParenthesisNoSurroundingWhitespaceRule = @"^[^()\s]([\s\S]*\S)?$";

        /// <summary>
        /// The names of the validation rules that accepted leading and trailing whitespace and are tightened to
        /// <see cref="NoSurroundingWhitespaceRule" />.
        /// </summary>
        private static readonly string[] NoSurroundingWhitespaceRuleNames =
        {
            "PersonShortName", "PersonGivenName", "PersonSurname", "TelephoneNumber", "UserPreference", "LanguageCode",
            "ForwardRelationshipName", "InverseRelationshipName", "Exponent", "Symbol", "ScaleValueDefinition",
            "ScaleReferenceQuantityValue", "Factor", "Modulus", "Value", "ConversionFactor"
        };

        /// <summary>
        /// The names of the validation rules that accepted trailing whitespace and are tightened to
        /// <see cref="NoParenthesisNoSurroundingWhitespaceRule" />.
        /// </summary>
        private static readonly string[] NoParenthesisNoSurroundingWhitespaceRuleNames =
        {
            "RDLName", "RDLShortName", "ModelSetupName", "FileRevisionName", "EnumerationValueDefinitionName"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ImeValidationService" /> class.
        /// </summary>
        public ImeValidationService()
        {
            // Allow a Requirement, RequirementsGroup or RequirementsSpecification Name to start with a digit.
            // Same intent as the default "Name" rule, but the first character may also be a digit.
            this.ValidationMap["RequirementName"] = new ValidationRule
            {
                PropertyName = nameof(DefinedThing.Name),
                Rule = @"^([\p{L}\d]|[\p{L}\d][^()]*[^()\s])$",
                ErrorText = "The Name must start with a letter or a digit and not contain any parentheses or trailing spaces."
            };

            foreach (var validationRuleName in NoSurroundingWhitespaceRuleNames)
            {
                this.TightenWhitespaceRule(validationRuleName, NoSurroundingWhitespaceRule, existingRule => $"{existingRule.ErrorText} Leading and trailing spaces are not allowed.");
            }

            foreach (var validationRuleName in NoParenthesisNoSurroundingWhitespaceRuleNames)
            {
                this.TightenWhitespaceRule(validationRuleName, NoParenthesisNoSurroundingWhitespaceRule, existingRule => $"The {existingRule.PropertyName} can not be empty, start with a parenthesis, or start or end with a space.");
            }
        }

        /// <summary>
        /// Replaces the <see cref="ValidationRule.Rule" /> of an existing <see cref="ValidationRule" /> with a rule that rejects
        /// leading and trailing whitespace.
        /// </summary>
        /// <param name="validationRuleName">
        /// The name under which the <see cref="ValidationRule" /> is registered in the <see cref="ValidationService.ValidationMap" />.
        /// </param>
        /// <param name="rule">
        /// The regular expression that replaces the registered one.
        /// </param>
        /// <param name="errorTextFactory">
        /// A function that computes the new error text based on the registered <see cref="ValidationRule" />.
        /// </param>
        private void TightenWhitespaceRule(string validationRuleName, string rule, Func<ValidationRule, string> errorTextFactory)
        {
            if (!this.ValidationMap.TryGetValue(validationRuleName, out var existingRule))
            {
                return;
            }

            this.ValidationMap[validationRuleName] = new ValidationRule
            {
                PropertyName = existingRule.PropertyName,
                Rule = rule,
                ErrorText = errorTextFactory(existingRule)
            };
        }

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
