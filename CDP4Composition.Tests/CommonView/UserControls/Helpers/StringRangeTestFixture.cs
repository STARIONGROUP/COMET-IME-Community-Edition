// -------------------------------------------------------------------------------------------------
// <copyright file="StringRangeTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests.UserControls.Helpers
{
    using NUnit.Framework;
    using CDP4CommonView.UserControls;

    /// <summary>
    /// Suite of tests for the <see cref="StringRange"/> class.
    /// </summary>
    [TestFixture]
    public class StringRangeTestFixture
    {
        private int start = 1;
        private int length = 10;

        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            var stringRange = new StringRange(this.start, this.length);

            Assert.AreEqual(this.start, stringRange.Start);
            Assert.AreEqual(this.length, stringRange.Length);
        }
    }
}
