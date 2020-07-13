namespace CDP4Composition.Tests.Reporting
{
    using CDP4Composition.Reporting;

    using NUnit.Framework;

    [TestFixture]
    public class ParameterTypeShortNameAttributeTestFixture
    {
        [Test]
        public void TestParameterTypeShortNameAttribute()
        {
            var attribute = new ParameterTypeShortNameAttribute("shortName");
            Assert.AreEqual(attribute.ShortName, "shortName");
        }
    }
}
