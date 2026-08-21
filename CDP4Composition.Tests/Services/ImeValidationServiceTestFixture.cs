// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImeValidationServiceTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Services
{
    using CDP4Composition.Services;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ImeValidationService"/> class
    /// </summary>
    [TestFixture]
    public class ImeValidationServiceTestFixture
    {
        /// <summary>
        /// The names of the validation rules that must reject leading and trailing whitespace.
        /// </summary>
        private static readonly string[] WhitespaceSensitiveRuleNames =
        {
            "PersonShortName", "PersonGivenName", "PersonSurname",
            "RDLName", "RDLShortName", "ModelSetupName", "FileRevisionName", "EnumerationValueDefinitionName",
            "TelephoneNumber", "UserPreference", "LanguageCode", "ForwardRelationshipName", "InverseRelationshipName",
            "Exponent", "Symbol", "ScaleValueDefinition", "ScaleReferenceQuantityValue", "Factor", "Modulus",
            "Value", "ConversionFactor"
        };

        private ImeValidationService validationService;

        [SetUp]
        public void SetUp()
        {
            this.validationService = new ImeValidationService();
        }

        [Test]
        public void VerifyThatTrailingWhitespaceIsRejected([ValueSource(nameof(WhitespaceSensitiveRuleNames))] string ruleName)
        {
            Assert.That(this.validationService.ValidateProperty(ruleName, "first.last "), Is.Not.Null, $"rule {ruleName} accepted a trailing space");
            Assert.That(this.validationService.ValidateProperty(ruleName, "first.last\t"), Is.Not.Null, $"rule {ruleName} accepted a trailing tab");
        }

        [Test]
        public void VerifyThatLeadingWhitespaceIsRejected([ValueSource(nameof(WhitespaceSensitiveRuleNames))] string ruleName)
        {
            Assert.That(this.validationService.ValidateProperty(ruleName, " first.last"), Is.Not.Null, $"rule {ruleName} accepted a leading space");
        }

        [Test]
        public void VerifyThatEmptyValueIsRejected([ValueSource(nameof(WhitespaceSensitiveRuleNames))] string ruleName)
        {
            Assert.That(this.validationService.ValidateProperty(ruleName, string.Empty), Is.Not.Null);
            Assert.That(this.validationService.ValidateProperty(ruleName, "   "), Is.Not.Null);
            Assert.That(this.validationService.ValidateProperty(ruleName, null), Is.Not.Null);
        }

        [Test]
        public void VerifyThatValidValueIsAccepted([ValueSource(nameof(WhitespaceSensitiveRuleNames))] string ruleName)
        {
            Assert.That(this.validationService.ValidateProperty(ruleName, "first.last"), Is.Null);
            Assert.That(this.validationService.ValidateProperty(ruleName, "a"), Is.Null);
        }

        [Test]
        public void VerifyThatInnerWhitespaceRemainsAccepted()
        {
            Assert.That(this.validationService.ValidateProperty("PersonGivenName", "Jan Willem"), Is.Null);
            Assert.That(this.validationService.ValidateProperty("RDLName", "Generic RDL"), Is.Null);
        }

        [Test]
        public void VerifyThatRdlRulesStillRejectALeadingParenthesis()
        {
            Assert.That(this.validationService.ValidateProperty("RDLName", "(Generic RDL)"), Is.Not.Null);
            Assert.That(this.validationService.ValidateProperty("RDLShortName", "(RDL)"), Is.Not.Null);
        }

        [Test]
        public void VerifyThatUnchangedRulesAreNotAffected()
        {
            Assert.That(this.validationService.ValidateProperty("ShortName", "Bat"), Is.Null);
            Assert.That(this.validationService.ValidateProperty("Name", "Battery"), Is.Null);
            Assert.That(this.validationService.ValidateProperty("Name", "Battery "), Is.Not.Null);
            Assert.That(this.validationService.ValidateProperty("EmailAddress", "john.doe@stariongroup.eu"), Is.Null);
        }
    }
}
