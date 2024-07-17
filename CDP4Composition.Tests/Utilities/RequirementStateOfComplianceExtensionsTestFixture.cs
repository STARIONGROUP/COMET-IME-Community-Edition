// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementStateOfComplianceExtensionsTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2020 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;

    using CDP4Composition.Utilities;

    using CDP4RequirementsVerification;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CDP4ColorExtensions"/> class
    /// </summary>
    [TestFixture]
    public class RequirementStateOfComplianceExtensionsTestFixture
    {
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void Verify_that_it_returns_correct_values(RequirementStateOfCompliance requirementStateOfCompliance)
        {
            Assert.DoesNotThrow(() => requirementStateOfCompliance.GetHexColorValue());
            Assert.IsInstanceOf<string>(requirementStateOfCompliance.GetHexColorValue());
            Assert.DoesNotThrow(() => requirementStateOfCompliance.GetBrush());
            Assert.IsInstanceOf<Brush>(requirementStateOfCompliance.GetBrush());
        }

        private static IEnumerable<RequirementStateOfCompliance> TestCases()
        {
            foreach (var value in Enum.GetValues(typeof(RequirementStateOfCompliance)))
            {
                if (value is RequirementStateOfCompliance requirementStateOfCompliance)
                {
                    yield return requirementStateOfCompliance;
                }
                else
                {
                    throw new Exception($"Color {value} is not a valid {nameof(RequirementStateOfCompliance)}");
                }
            }
        }
    }
}
