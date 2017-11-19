// -------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRulesRibbonPageViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Tests.ViewModels
{
    using CDP4BuiltInRules.ViewModels;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="BuiltInRulesRibbonPageViewModel"/>
    /// </summary>
    [TestFixture]
    public class BuiltInRulesRibbonPageViewModelTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IRuleVerificationService> ruleVerificationService;

        [SetUp]
        public void SetUp()
        {
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.ruleVerificationService = new Mock<IRuleVerificationService>();
        }

        [Test]
        public void VerifyThatBuiltInRulesRibbonPageViewModelCanBeConstructed()
        {
            var viewmodel = new BuiltInRulesRibbonPageViewModel(this.panelNavigationService.Object, this.dialogNavigationService.Object, this.ruleVerificationService.Object);

            Assert.IsTrue(viewmodel.OpenBrowser.CanExecute(null));
        }
    }
}
