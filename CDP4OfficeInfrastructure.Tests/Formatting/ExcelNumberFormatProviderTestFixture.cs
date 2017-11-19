// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExcelNumberFormatProviderTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace CDP4OfficeInfrastructure.Tests.Formatting
{
    using System;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ExcelNumberFormatProvider"/>
    /// </summary>
    [TestFixture]
    public class ExcelNumberFormatProviderTestFixture
    {
        [Test]
        public void VerifyThatExpectedNumberFormatIsReturned()
        {
            var excelNumberFormatInfo = ExcelNumberFormatProvider.CreateExcelNumberFormatInfo(false, ".", ",");

            Assert.AreEqual(".", excelNumberFormatInfo.NumberDecimalSeparator);
            Assert.AreEqual(",", excelNumberFormatInfo.NumberGroupSeparator);
        }

        [SetCulture("fr-FR")]
        [Test]
        public void VerifyThatIfSystemSeparatorsAreUsedExpectedNumberFormatInfoIsReturned()
        {
            var excelNumberFormatInfo = ExcelNumberFormatProvider.CreateExcelNumberFormatInfo(true);

            Assert.AreEqual(",", excelNumberFormatInfo.NumberDecimalSeparator);
        }


        [Test]
        public void VerifyThatArgumentExceptionsAreThrown()
        {
            Assert.Throws<ArgumentException>(() => ExcelNumberFormatProvider.CreateExcelNumberFormatInfo(false, null, "."));
            Assert.Throws<ArgumentException>(() => ExcelNumberFormatProvider.CreateExcelNumberFormatInfo(false, string.Empty, "."));            
            
            Assert.Throws<ArgumentException>(() => ExcelNumberFormatProvider.CreateExcelNumberFormatInfo(false, ".", null));
            Assert.Throws<ArgumentException>(() => ExcelNumberFormatProvider.CreateExcelNumberFormatInfo(false, ".", string.Empty));            
        }
    }
}
