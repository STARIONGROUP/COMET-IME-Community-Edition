// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationalOperatorKindConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Linq;
    using System.Threading;

    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Converters;
    using NUnit.Framework;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class RelationalOperatorKindConverterTestFixture
    {
        /// <summary>
        /// the <see cref="RelationalOperatorKindConverter"/> under test
        /// </summary>
        private RelationalOperatorKindConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new RelationalOperatorKindConverter();
        }

        [Test]
        public void VerifyThatConvertingNullReturnsNull()
        {
            var converterResult = this.converter.Convert(null, null, null, null);
            Assert.IsNull(converterResult);
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedString()
        {
            foreach (var val in Enum.GetValues(typeof(RelationalOperatorKind)).Cast<RelationalOperatorKind>())
            {
                var expected = val.ToScientificNotationString();
                var converterResult1 = this.converter.Convert(val, null, null, null);
                var converterResult2 = this.converter.Convert(val.ToString(), null, null, null);
                var converterResult3 = this.converter.Convert((int)val, null, null, null);

                Assert.AreEqual(expected, converterResult1);
                Assert.AreEqual(expected, converterResult2);
                Assert.AreEqual(expected, converterResult3);
            }
        }

        [Test]
        public void VerifyThatConvertingUnknownObjectReturnsItsStringValue()
        {
            foreach (var val in new object[] { "Unknown", 1.1D, true })
            {
                var converterResult = this.converter.Convert(val, null, null, null);
                Assert.AreEqual(val.ToString(), converterResult);
            }
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}