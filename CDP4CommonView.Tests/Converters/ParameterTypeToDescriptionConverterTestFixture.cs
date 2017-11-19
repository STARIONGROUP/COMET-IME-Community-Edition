// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeToDescriptionConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests.Converters
{
    using System;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView.Converters;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterTypeToDescriptionConverter"/>
    /// </summary>
    [TestFixture]
    public class ParameterTypeToDescriptionConverterTestFixture
    {
        private ParameterTypeToDescriptionConverter parameterTypeToDescriptionConverter;

        [SetUp]
        public void SetUp()
        {
            this.parameterTypeToDescriptionConverter = new ParameterTypeToDescriptionConverter();
        }

        [Test]
        public void VerifyParameterTypeToDescriptionConverter()
        {
            string shortName = "d";
            string name = "date";

            var dateParameterType = new DateParameterType() { ShortName = shortName, Name = name };
            var result = (string)this.parameterTypeToDescriptionConverter.Convert(dateParameterType, null, null, null);

            Assert.AreEqual("d - date", result);            
        }

        [Test]
        public void VerifyThatIfNotParameterTypeProvidedTheValueIsReturned()
        {
            var person = new Person(Guid.NewGuid(), null, null);
            var result = this.parameterTypeToDescriptionConverter.Convert(person, null, null, null);

            Assert.AreEqual(person, result);
        }

        [Test]
        public void VerifyThatConvertBackThrowsNotSupportedException()
        {
            var somestring = string.Empty;
            Assert.Throws<NotSupportedException>(() => this.parameterTypeToDescriptionConverter.ConvertBack(somestring, null, null, null));
        }
    }
}
