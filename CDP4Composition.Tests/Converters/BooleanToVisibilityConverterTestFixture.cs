// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BooleanToVisibilityConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Windows;
    using CDP4Composition.Converters;
    using NUnit.Framework;

    [TestFixture]
    public class BooleanToVisibilityConverterTestFixture
    {
        private BooleanToVisibilityConverter booleanToVisibilityConverter;

        [SetUp]
        public void SetUp()
        {
            this.booleanToVisibilityConverter = new BooleanToVisibilityConverter();
        }

        [Test]
        public void VerifyThatConvertReturnsExpectedResult()
        {
            Assert.AreEqual(Visibility.Collapsed, this.booleanToVisibilityConverter.Convert(null, null, null, null));

            Assert.AreEqual(Visibility.Visible, this.booleanToVisibilityConverter.Convert(true, null, null, null));
            Assert.AreEqual(Visibility.Collapsed, this.booleanToVisibilityConverter.Convert(false, null, null, null));
        }

        [Test]
        public void VerifyThatConvertBackIsNotSupported()
        {
            Assert.Throws<NotSupportedException>(() => this.booleanToVisibilityConverter.ConvertBack(null, null, null, null));
        }
    }
}
