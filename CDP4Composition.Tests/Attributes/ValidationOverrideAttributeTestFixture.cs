// -------------------------------------------------------------------------------------------------
// <copyright file="ValidationOverrideAttribute.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Attributes
{
    using System;

    using CDP4Composition.Attributes;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ValidationOverrideAttribute"/> class
    /// </summary>
    [TestFixture]
    public class ValidationOverrideAttributeTestFixture
    {
        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            var validationOverrideAttribute = new ValidationOverrideAttribute(true, "some random name");

            Assert.IsTrue(validationOverrideAttribute.IsValidationEnabled); 
            Assert.AreEqual("some random name", validationOverrideAttribute.ValidationOverrideName);
        }

        [Test]
        public void VerifyThatIfValidationEnabledValidationOverrideMayNotBeNull()
        {
            Assert.Throws<InvalidOperationException>(() => new ValidationOverrideAttribute(true, null));
        }
    }
}
