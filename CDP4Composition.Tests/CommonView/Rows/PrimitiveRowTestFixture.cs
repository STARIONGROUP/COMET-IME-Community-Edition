// -------------------------------------------------------------------------------------------------
// <copyright file="PrimitiveRowTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests
{
    using CDP4CommonView;
    using Moq;    
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="PrimitiveRow{T}"/> class.
    /// </summary>
    [TestFixture]
    public class PrimitiveRowTestFixture
    {
        [Test]
        public void VerifyThatPropertiesAreSetForPrimitiveRowOfTypeString()
        {
            var stringPrimitiveRow = new PrimitiveRow<string>();

            var index = 1;
            var value = "a string";

            stringPrimitiveRow.Index = index;
            stringPrimitiveRow.Value = value;

            Assert.AreEqual(index, stringPrimitiveRow.Index);
            Assert.AreEqual(value, stringPrimitiveRow.Value);
        }

        [Test]
        public void VerifyThatPropertiesAreSetForPrimitiveRowOfTypeBool()
        {
            var stringPrimitiveRow = new PrimitiveRow<bool>();

            var index = 1;
            var value = true;

            stringPrimitiveRow.Index = index;
            stringPrimitiveRow.Value = value;

            Assert.AreEqual(index, stringPrimitiveRow.Index);
            Assert.AreEqual(value, stringPrimitiveRow.Value);
        }

        [Test]
        public void VerifyThatPropertiesAreSetForPrimitiveRowOfTypeInt()
        {
            var stringPrimitiveRow = new PrimitiveRow<int>();

            var index = 1;
            var value = 42;

            stringPrimitiveRow.Index = index;
            stringPrimitiveRow.Value = value;

            Assert.AreEqual(index, stringPrimitiveRow.Index);
            Assert.AreEqual(value, stringPrimitiveRow.Value);
        }

        [Test]
        public void VerifyThatPropertiesAreSetForPrimitiveRowOfTypeObject()
        {
            var stringPrimitiveRow = new PrimitiveRow<object>();

            var index = 1;
            var value = new object();

            stringPrimitiveRow.Index = index;
            stringPrimitiveRow.Value = value;

            Assert.AreEqual(index, stringPrimitiveRow.Index);
            Assert.AreEqual(value, stringPrimitiveRow.Value);
        }
    }
}
