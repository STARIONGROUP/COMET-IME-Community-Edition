// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterValueValidatorTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
            Assert.IsNullOrEmpty(ParameterValueValidator.Validate(true, boolPt));
            Assert.IsNullOrEmpty(ParameterValueValidator.Validate(false, boolPt));
            Assert.IsNullOrEmpty(ParameterValueValidator.Validate(null, boolPt));

            Assert.IsNotNullOrEmpty(ParameterValueValidator.Validate("hoho", boolPt));
        }
    }
}