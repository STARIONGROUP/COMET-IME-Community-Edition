namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Threading;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using NUnit.Framework;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class CamelCaseToSpaceConverterTestFixture
    {
        /// <summary>
        /// the <see cref="ThingToIconUriConverter"/> under test
        /// </summary>
        private CamelCaseToSpaceConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new CamelCaseToSpaceConverter();
        }

        [Test]
        public void VerifyThatConvertingNullReturnsNull()
        {
            var string1 = this.converter.Convert(null, null, null, null);
            Assert.IsNull(string1);
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedString()
        {
            const string Binrelstring = "Binary Relationship Rule";
            var brr = new BinaryRelationshipRule();
            var converterResult = this.converter.Convert(brr.ClassKind, null, null, null);

            Assert.AreEqual(Binrelstring, converterResult);
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedAbbriviationString()
        {
            const string binrelstring = "ESA";
            var brr = "ESA";
            var converterResult = this.converter.Convert(brr, null, null, null);

            Assert.AreEqual(binrelstring, converterResult);
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}
