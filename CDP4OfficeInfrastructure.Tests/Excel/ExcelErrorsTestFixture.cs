// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExcelErrorsTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.Tests.Excel
{
    using System;
    using CDP4OfficeInfrastructure.Excel;
    using NetOffice.ExcelApi.Enums;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ExcelErrors"/> class.
    /// </summary>
    [TestFixture]
    public class ExcelErrorsTestFixture
    {
        [Test]
        public void VerifyThatIsXLCVErrReturnsTrueForIntandFalsOtherwise()
        {
            Assert.IsTrue(ExcelErrors.IsXLCVErr((int)XlCVError.xlErrDiv0));
            Assert.IsTrue(ExcelErrors.IsXLCVErr((int)XlCVError.xlErrNA));
            Assert.IsTrue(ExcelErrors.IsXLCVErr((int)XlCVError.xlErrName));
            Assert.IsTrue(ExcelErrors.IsXLCVErr((int)XlCVError.xlErrNull));
            Assert.IsTrue(ExcelErrors.IsXLCVErr((int)XlCVError.xlErrNum));
            Assert.IsTrue(ExcelErrors.IsXLCVErr((int)XlCVError.xlErrRef));
            Assert.IsTrue(ExcelErrors.IsXLCVErr((int)XlCVError.xlErrValue));

            var str = string.Empty;
            Assert.IsFalse(ExcelErrors.IsXLCVErr(str));

            double d = 10f;
            Assert.IsFalse(ExcelErrors.IsXLCVErr(d));

            float f = 10f;
            Assert.IsFalse(ExcelErrors.IsXLCVErr(f));

            DateTime date = DateTime.Parse("2012-12-12");
            Assert.IsFalse(ExcelErrors.IsXLCVErr(date));

            DateTime dateTime = DateTime.Parse("2012-12-12T23:11:11.001");
            Assert.IsFalse(ExcelErrors.IsXLCVErr(dateTime));
        }
    }
}
