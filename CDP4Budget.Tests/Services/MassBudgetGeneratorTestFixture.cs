// -------------------------------------------------------------------------------------------------
// <copyright file="MassBudgetGeneratorTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Budget.Tests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Budget.Services;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using Config;
    using NUnit.Framework;

    [TestFixture]
    public class MassBudgetGeneratorTestFixture : BudgetTestFixtureBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void VerifyThatGeneratorIsCorrect()
        {
            var generator = new MassBudgetGenerator();
            var resA = (SubSystemMassBudgetResult)generator.ComputeResult(this.MassBudgetConfig, this.MassBudgetConfig.Elements.Single(), this.option_A, this.domainOfExpertise)[0];
            var extraA = (ExtraContribution)generator.GetExtraMassContributions(this.MassBudgetConfig, this.MassBudgetConfig.Elements.Single(), this.option_A, this.domainOfExpertise)[0];
            var resB = (SubSystemMassBudgetResult)generator.ComputeResult(this.MassBudgetConfig, this.MassBudgetConfig.Elements.Single(), this.option_B, this.domainOfExpertise)[0];
            var extraB = (ExtraContribution)generator.GetExtraMassContributions(this.MassBudgetConfig, this.MassBudgetConfig.Elements.Single(), this.option_B, this.domainOfExpertise)[0];

            Assert.AreEqual(15000f, resA.DryMassFromEquipment); // 10 * 1000 + 10 * 500
            Assert.AreEqual(2000f, resA.DryMassFromSubSystem);
            Assert.AreEqual(25f, resA.DryMassMarginRatioFromEquipment);
            Assert.AreEqual(20f, resA.DryMassMarginRatioFromSubSystem);
            Assert.AreEqual(18750f, resA.DryMassWithMarginFromEquipment);

            Assert.AreEqual(20000f, resB.DryMassFromEquipment); // 10 * 2000 (eqt2 is excluded)
            Assert.AreEqual(3000f, resB.DryMassFromSubSystem);
            Assert.AreEqual(25f, resB.DryMassMarginRatioFromEquipment);
            Assert.AreEqual(20f, resB.DryMassMarginRatioFromSubSystem);
            Assert.AreEqual(25000f, resB.DryMassWithMarginFromEquipment);

            Assert.AreEqual(200f, extraA.TotalContribution);
            Assert.AreEqual(400f, extraA.TotalWithMargin);
            Assert.AreEqual(200f, extraB.TotalContribution);
            Assert.AreEqual(400f, extraB.TotalWithMargin);
        }
    }
}
