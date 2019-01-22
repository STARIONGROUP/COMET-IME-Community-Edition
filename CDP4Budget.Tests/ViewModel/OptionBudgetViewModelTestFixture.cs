// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionBudgetViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.Tests
{
    using System.Linq;
    using NUnit.Framework;
    using ViewModels;

    [TestFixture]
    public class OptionBudgetViewModelTestFixture : BudgetTestFixtureBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void VerifyViewModelInitializedWell()
        {
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domainOfExpertise);
            var vm = new OptionBudgetViewModel(this.option_A, this.MassBudgetConfig, this.session.Object, () => { });

            var budgetVm = (MassBudgetSummaryViewModel)vm.BudgetSummary.Single();
            Assert.AreEqual(18750f, budgetVm.DryTotal);
            Assert.AreEqual(19150f, budgetVm.WetTotal);

            var vmB = new OptionBudgetViewModel(this.option_B, this.MassBudgetConfig, this.session.Object, () => { });

            var budgetVmb = (MassBudgetSummaryViewModel)vmB.BudgetSummary.Single();
            Assert.AreEqual(25000f, budgetVmb.DryTotal);
            Assert.AreEqual(25400f, budgetVmb.WetTotal);
        }
    }
}