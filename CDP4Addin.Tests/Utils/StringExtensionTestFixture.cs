// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensionTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4AddinCE.Tests.Utils
{
    using CDP4AddinCE.Utils;
    using Moq;

    using NetOffice.OfficeApi.Enums;

    using NUnit.Framework;

    /// <summary>
    /// suite of tests for the <see cref="StringExtension"/> class
    /// </summary>
    [TestFixture]
    public class StringExtensionTestFixture
    {
        [Test]
        public void VerifyThatToDockPositionReturnsExpectedResults()
        {
            var left = "LeftPanel";
            Assert.AreEqual(MsoCTPDockPosition.msoCTPDockPositionLeft, StringExtension.ToDockPosition(left));

            var right = "RightPanel";
            Assert.AreEqual(MsoCTPDockPosition.msoCTPDockPositionRight, StringExtension.ToDockPosition(right));

            var bottom = "BottomPanel";
            Assert.AreEqual(MsoCTPDockPosition.msoCTPDockPositionBottom, StringExtension.ToDockPosition(bottom));

            var anystring = "somestring";
            Assert.AreEqual(MsoCTPDockPosition.msoCTPDockPositionLeft, StringExtension.ToDockPosition(anystring));
        }
    }
}
