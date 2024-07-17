// -------------------------------------------------------------------------------------------------
// <copyright file="ThingDialogViewExportAttributeTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Attributes
{
    using CDP4Common.CommonData;

    using CDP4Composition.Attributes;
    using NUnit.Framework;

    /// <summary>
    /// Suite of test for the <see cref="ThingDialogViewExportAttribute"/> attribute
    /// </summary>
    [TestFixture]
    public class ThingDialogViewExportAttributeTestFixture
    {
        [Test]
        public void VerifyClassKindAttributeIsSet()
        {
            var classKind = ClassKind.SimpleUnit;
            var attribute = new ThingDialogViewExportAttribute(classKind);
            Assert.AreEqual(classKind, attribute.ClassKind);
        }
    }
}
