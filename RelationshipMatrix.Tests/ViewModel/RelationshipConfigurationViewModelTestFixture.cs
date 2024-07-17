﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipConfigurationViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

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
            Assert.That(((ICommand)this.relationshipConfigurationViewModel.InspectRuleCommand).CanExecute(null), Is.False);

            this.relationshipConfigurationViewModel.PossibleRules.Add(this.rule);

            this.relationshipConfigurationViewModel.SelectedRule = this.relationshipConfigurationViewModel.PossibleRules.First();

            Assert.That(((ICommand)this.relationshipConfigurationViewModel.InspectRuleCommand).CanExecute(null), Is.True);
        }

        [Test]
        public async Task Verify_that_when_inspect_command_is_executed_navigation_service_is_invoked()
        {
            this.relationshipConfigurationViewModel.PossibleRules.Add(this.rule);

            this.relationshipConfigurationViewModel.SelectedRule = this.relationshipConfigurationViewModel.PossibleRules.First();

            await this.relationshipConfigurationViewModel.InspectRuleCommand.Execute();

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