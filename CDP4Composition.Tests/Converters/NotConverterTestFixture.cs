// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotConverterTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Threading;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using NUnit.Framework;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class NotConverterTestFixture
    {
        /// <summary>
        /// the <see cref="NotConverter"/> under test
        /// </summary>
        private NotConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new NotConverter();
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedString()
        {
            var brr = new BinaryRelationshipRule();
            
            Assert.Throws<InvalidCastException>(() => this.converter.Convert(brr.ClassKind, null, null, null));
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedBooleanValue()
        {
            var result = this.converter.Convert(true, null, null, null);
            Assert.IsNotNull(result);
            var shouldBeFalse = (bool)result;
            Assert.IsNotNull(shouldBeFalse);
            Assert.IsFalse(shouldBeFalse);

            result = this.converter.Convert(false, null, null, null);
            Assert.IsNotNull(result);
            var shouldBeTrue = (bool)result;
            Assert.IsNotNull(shouldBeTrue);
            Assert.IsTrue(shouldBeTrue);
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null)) ;
        }
    }
}
