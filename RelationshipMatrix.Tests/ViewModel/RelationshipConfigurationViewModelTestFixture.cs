// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipConfigurationViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using System;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4RelationshipMatrix.Settings;
    using CDP4RelationshipMatrix.ViewModels;
    using NUnit.Framework;
    using Moq;    

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipConfigurationViewModel"/>
    /// </summary>
    [TestFixture]
    public class RelationshipConfigurationViewModelTestFixture : ViewModelTestBase
    {
        private RelationshipConfigurationViewModel relationshipConfigurationViewModel;
        private RelationshipConfiguration relationshipConfiguration;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            this.relationshipConfiguration = new RelationshipConfiguration
            {
                SelectedRule = this.rule.Iid
            };

            this.relationshipConfigurationViewModel = new RelationshipConfigurationViewModel(this.session.Object, this.thingDialogNavigationService.Object, this.iteration, this.UpdateAction, null, this.relationshipConfiguration, ClassKind.Requirement, ClassKind.ElementUsage);
        }

        /// <summary>
        /// Builds the relationship matrix
        /// </summary>
        private void UpdateAction()
        {
            Console.WriteLine("update action");
        }

        [Test]
        public void Verify_that_the_rule_inspect_command_is_active_when_rule_is_selected()
        {
            Assert.IsFalse(this.relationshipConfigurationViewModel.InspectRuleCommand.CanExecute(null));

            this.relationshipConfigurationViewModel.PossibleRules.Add(this.rule);

            this.relationshipConfigurationViewModel.SelectedRule = this.relationshipConfigurationViewModel.PossibleRules.First();

            Assert.IsTrue(this.relationshipConfigurationViewModel.InspectRuleCommand.CanExecute(null)); 
        }

        [Test]
        public void Verify_that_when_inspect_command_is_executed_navigation_service_is_invoked()
        {
            this.relationshipConfigurationViewModel.PossibleRules.Add(this.rule);

            this.relationshipConfigurationViewModel.SelectedRule = this.relationshipConfigurationViewModel.PossibleRules.First();

            this.relationshipConfigurationViewModel.InspectRuleCommand.Execute(null);

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<Rule>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }

        [Test]
        public void Verify_that_relationship_configuration_can_be_created()
        {
            this.relationshipConfigurationViewModel.PossibleRules.Add(this.rule);

            this.relationshipConfigurationViewModel.SelectedRule = this.relationshipConfigurationViewModel.PossibleRules.First();

            var relConfig = new RelationshipConfiguration(this.relationshipConfigurationViewModel);

            Assert.AreEqual(this.rule.Iid, relConfig.SelectedRule);
        }
    }
}