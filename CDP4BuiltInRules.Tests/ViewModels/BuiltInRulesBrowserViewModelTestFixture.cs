// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRulesBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4BuiltInRules.ViewModels;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="BuiltInRulesBrowserViewModel"/>
    /// </summary>
    [TestFixture]
    public class BuiltInRulesBrowserViewModelTestFixture
    {
        private string builtInRuleName;
        private List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>> builtInRules;
        private TestBuiltInRule testBuiltInRule;
        private Mock<IBuiltInRuleMetaData> iBuiltInRuleMetaData;

        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IRuleVerificationService> ruleVerificationService;

        [SetUp]
        public void SetUp()
        {
            this.builtInRuleName = "shortnamerule";
            this.iBuiltInRuleMetaData = new Mock<IBuiltInRuleMetaData>();
            this.iBuiltInRuleMetaData.Setup(x => x.Author).Returns("RHEA");
            this.iBuiltInRuleMetaData.Setup(x => x.Name).Returns(this.builtInRuleName);
            this.iBuiltInRuleMetaData.Setup(x => x.Description).Returns("verifies that the shortnames are correct");

            this.testBuiltInRule = new TestBuiltInRule();

            this.builtInRules = new List<Lazy<IBuiltInRule, IBuiltInRuleMetaData>>();
            this.builtInRules.Add(new Lazy<IBuiltInRule, IBuiltInRuleMetaData>(() => this.testBuiltInRule, this.iBuiltInRuleMetaData.Object));

            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.ruleVerificationService = new Mock<IRuleVerificationService>();

            this.ruleVerificationService.Setup(x => x.BuiltInRules).Returns(this.builtInRules);
        }

        [Test]
        public void VerifyThatBuiltInRulesAreAddedToBrowser()
        {
            var viewmodel = new BuiltInRulesBrowserViewModel(this.ruleVerificationService.Object, this.dialogNavigationService.Object);

            var ruleViewModel = viewmodel.BuiltInRules.Single();

            Assert.AreEqual(this.testBuiltInRule, ruleViewModel.Rule);
            Assert.AreEqual("RHEA", ruleViewModel.Author);
            Assert.AreEqual(this.builtInRuleName, ruleViewModel.Name);
            Assert.AreEqual("verifies that the shortnames are correct", ruleViewModel.Description);
        }

        [Test]
        public void VerifyThatIfNoRowIsSelectedTheInspectCommandCanNotExecute()
        {
            var viewmodel = new BuiltInRulesBrowserViewModel(this.ruleVerificationService.Object, this.dialogNavigationService.Object);
            viewmodel.SelectedRule = null;

            Assert.IsFalse(viewmodel.InspectCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatIfRowIsSelectedInspectCommanCanExecute()
        {
            var viewmodel = new BuiltInRulesBrowserViewModel(this.ruleVerificationService.Object, this.dialogNavigationService.Object);
            var row = viewmodel.BuiltInRules.Single();
            viewmodel.SelectedRule = row;

            Assert.IsTrue(viewmodel.InspectCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatIfInspectCommanIsExecutedNaviationServiceIsInvoked()
        {
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<BuiltInRuleDialogViewModel>())).Returns(null as IDialogResult);

            var viewmodel = new BuiltInRulesBrowserViewModel(this.ruleVerificationService.Object, this.dialogNavigationService.Object);
            var row = viewmodel.BuiltInRules.Single();
            viewmodel.SelectedRule = row;

            viewmodel.InspectCommand.Execute(null);

            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<BuiltInRuleDialogViewModel>()));
        }

        [Test]
        public void VerifyThatStartDragWorks()
        {
            var draginfo = new Mock<IDragInfo>();
            var dragsource = new Mock<IDragSource>();
            draginfo.Setup(x => x.Payload).Returns(dragsource.Object);
            
            var viewmodel = new BuiltInRulesBrowserViewModel(this.ruleVerificationService.Object, this.dialogNavigationService.Object);
            viewmodel.StartDrag(draginfo.Object);

            dragsource.Verify(x => x.StartDrag(draginfo.Object));
        }
    }

    /// <summary>
    /// Test <see cref="BuiltInRule"/> class.
    /// </summary>
    internal class TestBuiltInRule : BuiltInRule
    {
        public override IEnumerable<RuleViolation> Verify(Iteration iteration)
        {
            throw new NotSupportedException();
        }
    }
}
