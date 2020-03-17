// -------------------------------------------------------------------------------------------------
// <copyright file="CDP4ColorExtensionsTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;

    using CDP4Composition.Utilities;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CDP4ColorExtensions"/> class
    /// </summary>
    [TestFixture]
    public class CDP4ColorExtensionsTestFixture
    {
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void Verify_that_it_returns_correct_values(CDP4Color cdp4Color)
        {
            Assert.DoesNotThrow(() => cdp4Color.GetHexValue());
            Assert.IsInstanceOf<string>(cdp4Color.GetHexValue());
            Assert.DoesNotThrow(() => cdp4Color.GetBrush());
            Assert.IsInstanceOf<Brush>(cdp4Color.GetBrush());
        }

        private static IEnumerable<CDP4Color> TestCases()
        {
            foreach (var value in Enum.GetValues(typeof(CDP4Color)))
            {
                if (value is CDP4Color cdp4Color)
                {
                    yield return cdp4Color;
                }
                else
                {
                    throw new Exception($"Color {value} is not a valid {nameof(CDP4Color)}");
                }
            }
        }
    }
}
