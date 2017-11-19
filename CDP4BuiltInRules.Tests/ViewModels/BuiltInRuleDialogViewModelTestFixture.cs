// -------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4BuiltInRules.Tests.ViewModels
{
    using CDP4BuiltInRules.ViewModels;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="BuiltInRuleDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class BuiltInRuleDialogViewModelTestFixture
    {
        private ElementDefinitionShortNameRule elementDefinitionShortNameRule;
        private BuiltInRuleDialogViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.elementDefinitionShortNameRule = new ElementDefinitionShortNameRule();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewModel = new BuiltInRuleDialogViewModel(this.elementDefinitionShortNameRule);

            Assert.AreEqual("CDP4BuiltInRules", viewModel.LibraryName);

            Assert.AreEqual("RHEA", viewModel.Author);
            Assert.AreEqual("ElementDefinitionShortName", viewModel.Name);
            Assert.AreEqual("A rule that verifies whether the shortname property of all ElementDefinition objects in an Iteration is valid", viewModel.Description);
        }
    }
}
