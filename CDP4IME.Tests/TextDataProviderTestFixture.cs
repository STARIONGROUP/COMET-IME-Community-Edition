// -------------------------------------------------------------------------------------------------
// <copyright file="TextDataProviderTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

using System.IO;

namespace CDP4IME.Tests
{
    using CDP4IME.Views;
    using NUnit.Framework;
    using System.Globalization;

    [TestFixture]
    public class TextDataProviderTestFixture
    {
        [Test]
        public void VerifyThatCorrectTextIsReturned()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Test.txt");
            
            var provider = new TestDataProvider();
            provider.Test(path);

            var res = provider.Result.ToString();
            Assert.AreEqual("Test", res);
        }

        [Test]
        public void VerifyThatErrorTextIsReturned()
        {
            var provider = new TestDataProvider();
            provider.Test("../../Error.txt");

            var res = provider.Result.ToString();
            Assert.IsTrue(res.ToString(CultureInfo.InvariantCulture).Contains("System.IO.FileNotFoundException"));
        }

        internal class TestDataProvider : TextDataProvider
        {
            public void Test(string uri)
            {
                base.Uri = uri;
                base.BeginQuery();
            }
        }
    }
}