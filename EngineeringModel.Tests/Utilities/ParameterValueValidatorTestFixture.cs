// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueValidatorTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Utilities
{
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using NUnit.Framework;

    [TestFixture]
    internal class ParameterValueValidatorTestFixture
    {
        [Test]
        public void VerifyThatValidatorWorksForBooleanParameterType()
        {
            var boolPt = new BooleanParameterType();
            
            Assert.That(ParameterValueValidator.Validate(true, boolPt), Is.Null.Or.Empty);
            Assert.That(ParameterValueValidator.Validate(false, boolPt), Is.Null.Or.Empty);
            Assert.That(ParameterValueValidator.Validate(null, boolPt), Is.Null.Or.Empty);            
            Assert.That(ParameterValueValidator.Validate("hoho", boolPt), Is.Not.Null.Or.Empty);
        }
    }
}