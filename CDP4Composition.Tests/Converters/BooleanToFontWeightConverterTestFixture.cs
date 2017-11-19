// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BooleanToFontWeightConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Windows;
    using CDP4Composition.Converters;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="booleanToFontWeightConverter"/>
    /// </summary>
    [TestFixture]
    public class BooleanToFontWeightConverterTestFixture
    {
        private BooleanToFontWeightConverter booleanToFontWeightConverter;

        [SetUp]
        public void SetUp()
        {
            this.booleanToFontWeightConverter = new BooleanToFontWeightConverter();
        }

        [Test]
        public void VerifyThatTheConvertMethodReturnsTheExpectedResult()
        {
            Assert.AreEqual(FontWeights.Bold, this.booleanToFontWeightConverter.Convert(true, null, null, null));

            Assert.AreEqual(FontWeights.Normal, this.booleanToFontWeightConverter.Convert(false, null, null, null));
            Assert.AreEqual(FontWeights.Normal, this.booleanToFontWeightConverter.Convert(new object(), null, null, null));
        }

        [Test]
        public void VerifyThatConvertBackIsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => this.booleanToFontWeightConverter.ConvertBack(null, null, null, null));
        }
    }
}
