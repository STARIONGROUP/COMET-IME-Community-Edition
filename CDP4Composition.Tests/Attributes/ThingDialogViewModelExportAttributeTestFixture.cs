// -------------------------------------------------------------------------------------------------
// <copyright file="ThingDialogViewModelExportAttributeTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Attributes
{
    using CDP4Common.CommonData;

    using CDP4Composition.Attributes;
    using NUnit.Framework;

    /// <summary>
    /// Suite of test for the <see cref="ThingDialogViewModelExportAttribute"/> attribute
    /// </summary>
    [TestFixture]
    public class ThingDialogViewModelExportAttributeTestFixture
    {
        [Test]
        public void VerifyClassKindAttributeIsSet()
        {
            var classKind = ClassKind.SimpleUnit;
            var attribute = new ThingDialogViewModelExportAttribute(classKind);
            Assert.AreEqual(classKind, attribute.ClassKind);
        }
    }
}
