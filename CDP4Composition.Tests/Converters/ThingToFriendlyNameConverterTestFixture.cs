// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingToFriendlyNameConverterTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Converters
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Converters;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ThingToFriendlyNameConverter"/> class
    /// </summary>
    [TestFixture]
    public class ThingToFriendlyNameConverterTestFixture
    {
        private ThingToFriendlyNameConverter converter;

        private ElementDefinition elementDefinition;

        private ParametricConstraint parametricConstraint;

        private ParametricConstraint emptyParametricConstraint;

        [SetUp]
        public void SetUp()
        {
            this.converter = new ThingToFriendlyNameConverter();

            this.elementDefinition = new ElementDefinition
            {
                Name = "Battery",
                ShortName = "BAT"
            };

            var relationalExpression = new RelationalExpression
            {
                ParameterType = new SimpleQuantityKind { ShortName = "mass" }
            };

            this.parametricConstraint = new ParametricConstraint();
            this.parametricConstraint.Expression.Add(relationalExpression);

            this.emptyParametricConstraint = new ParametricConstraint();
        }

        [Test]
        public void Verify_that_a_ParametricConstraint_is_converted_to_its_expression_string()
        {
            var result = this.converter.Convert(this.parametricConstraint, null, null, null) as string;

            Assert.That(result, Does.Contain("mass"));
            Assert.That(result, Does.Not.Contain("not implemented"));
        }

        [Test]
        public void Verify_that_a_ParametricConstraint_is_prefixed_with_the_owning_Requirement_shortname()
        {
            var requirement = new Requirement { ShortName = "REQ1" };
            requirement.ParametricConstraint.Add(this.parametricConstraint);

            var result = this.converter.Convert(this.parametricConstraint, null, null, null) as string;

            Assert.That(result, Does.StartWith("REQ1:"));
            Assert.That(result, Does.Contain("mass"));
        }

        [Test]
        public void Verify_that_an_empty_ParametricConstraint_falls_back_to_its_ClassKind()
        {
            Assert.AreEqual("ParametricConstraint", this.converter.Convert(this.emptyParametricConstraint, null, null, null));
        }

        [Test]
        public void Verify_that_a_ShortNamedThing_is_converted_to_its_UserFriendlyShortName()
        {
            Assert.AreEqual(this.elementDefinition.UserFriendlyShortName, this.converter.Convert(this.elementDefinition, null, null, null));
        }

        [Test]
        public void Verify_that_the_Name_parameter_yields_the_UserFriendlyName()
        {
            Assert.AreEqual(this.elementDefinition.UserFriendlyName, this.converter.Convert(this.elementDefinition, null, "Name", null));
        }

        [Test]
        public void Verify_that_a_null_value_is_converted_to_an_empty_string()
        {
            Assert.AreEqual(string.Empty, this.converter.Convert(null, null, null, null));
        }

        [Test]
        public void Verify_that_ConvertBack_is_not_supported()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}
