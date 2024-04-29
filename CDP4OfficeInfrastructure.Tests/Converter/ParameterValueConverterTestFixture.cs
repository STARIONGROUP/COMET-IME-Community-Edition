// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueConverterTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Tests.Converter
{
    using System;

    using CDP4Common.SiteDirectoryData;
    using CDP4OfficeInfrastructure.Converter;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterValueConverter"/> class
    /// </summary>
    [TestFixture]
    public class ParameterValueConverterTestFixture
    {
        private SimpleQuantityKind simpleQuantityKind;

        [SetUp]
        public void SetUp()
        {
            this.simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
        }

        [Test]
        public void VerifyThatDefaultReturnDefault()
        {
            var test = ParameterValueConverter.ConvertToObject(this.simpleQuantityKind, "-");
            Assert.AreEqual("-", test);
        }

        [Test]
        public void VerifyThattrueReturnTRUE()
        {
            var test = ParameterValueConverter.ConvertToObject(new BooleanParameterType(), "true");
            Assert.AreEqual("TRUE", test);
        }

        [Test]
        public void VerifyThatfalseReturnFalse()
        {
            var test = ParameterValueConverter.ConvertToObject(new BooleanParameterType(), "false");
            Assert.AreEqual("FALSE", test);
        }

        [Test]
        public void VerifyThatConvertForDatePtWorks()
        {
            var test = ParameterValueConverter.ConvertToObject(new DateParameterType(), "date");
            Assert.AreEqual("date", test);
        }

        [Test]
        public void VerifyThatConvertForDateTimePtWorks()
        {
            var test = ParameterValueConverter.ConvertToObject(new DateTimeParameterType(), "date");
            Assert.AreEqual("date", test);
        }

        [Test]
        public void VerifyThatConvertForEnumerationPtWorks()
        {
            var test = ParameterValueConverter.ConvertToObject(new EnumerationParameterType(), "date");
            Assert.AreEqual("date", test);
        }

        [Test]
        public void VerifyThatConvertForTimePtWorks()
        {
            var test = ParameterValueConverter.ConvertToObject(new TimeOfDayParameterType(), "date");
            Assert.AreEqual("date", test);
        }

        [Test]
        public void VerifyThatConvertForTextPtWorks()
        {
            var test = ParameterValueConverter.ConvertToObject(new TextParameterType(), "date");
            Assert.AreEqual("date", test);
        }

        [Test]
        public void VerifyThatConvertForQuantityKindWorks()
        {
            var test = (double)ParameterValueConverter.ConvertToObject(this.simpleQuantityKind, "1.2e5");
            Assert.AreEqual(1.2e5, test);
        }

        [Test]
        public void VerifyThatConvertForQuantityKindWorksOnWrongFormat()
        {
            var test = ParameterValueConverter.ConvertToObject(this.simpleQuantityKind, "yalayala");
            Assert.AreEqual("yalayala", test);
        }
    }
}
