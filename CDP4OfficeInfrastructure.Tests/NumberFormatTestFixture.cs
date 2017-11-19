// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NumberFormatTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Tests
{
    using CDP4Common.SiteDirectoryData;

    using NUnit.Framework;

    /// <summary>
    /// Suite of test for the <see cref="NumberFormat"/> class
    /// </summary>
    [TestFixture]
    public class NumberFormatTestFixture
    {
        [Test]
        public void VerifyFormatOfBooleanParameterType()
        {
            var booleanParameterType = new BooleanParameterType();
            var format = NumberFormat.Format(booleanParameterType);

            Assert.AreEqual("@", format);
        }

        [Test]
        public void VerifyFormatOfCompoundParameterType()
        {
            var compoundParameterType = new CompoundParameterType();
            var format = NumberFormat.Format(compoundParameterType);

            Assert.AreEqual("@", format);
        }

        [Test]
        public void VerifyFormatOfDateParameterType()
        {
            var dateParameterType = new DateParameterType();
            var format = NumberFormat.Format(dateParameterType);

            Assert.AreEqual("yyyy-mm-dd", format);
        }

        [Test]
        public void VerifyFormatOfDateTimeParameterType()
        {
            var dateTimeParameterType = new DateTimeParameterType();
            var format = NumberFormat.Format(dateTimeParameterType);

            Assert.AreEqual("yyyy-mm-dd hh:mm:ss", format);
        }

        [Test]
        public void VerifyFormatOfEnumerationParameterType()
        {
            var enumerationParameterType = new EnumerationParameterType();
            var format = NumberFormat.Format(enumerationParameterType);

            Assert.AreEqual("@", format);
        }

        [Test]
        public void VerifyFormatOfQuantityKind()
        {
            var format = string.Empty;

            var simpleQuantityKind = new SimpleQuantityKind();
            format = NumberFormat.Format(simpleQuantityKind);

            Assert.AreEqual("@", format);

            var ratioscale = new RatioScale();
            ratioscale.NumberSet = NumberSetKind.INTEGER_NUMBER_SET;
            format = NumberFormat.Format(simpleQuantityKind, ratioscale);
            Assert.AreEqual("0", format);

            ratioscale.NumberSet = NumberSetKind.NATURAL_NUMBER_SET;
            format = NumberFormat.Format(simpleQuantityKind, ratioscale);
            Assert.AreEqual("general", format);
        }

        [Test]
        public void VerifyFormatOfTextParameterType()
        {
            var textParameterType = new TextParameterType();
            var format = NumberFormat.Format(textParameterType);

            Assert.AreEqual("@", format);
        }

        [Test]
        public void VerifyFormOfTimeOfDayParameterType()
        {
            var timeOfDayParameterType = new TimeOfDayParameterType();
            var format = NumberFormat.Format(timeOfDayParameterType);

            Assert.AreEqual("hh:mm:ss", format);
        }
    }    
}
