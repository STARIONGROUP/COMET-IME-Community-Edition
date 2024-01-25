// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelModuleTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests
{
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;

    using CDP4EngineeringModel.Services;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="EngineeringModelModule"/> class
    /// </summary>
    [TestFixture]
    public class EngineeringModelModuleTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IFluentRibbonManager> fluentRibbonManager;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IParameterSubscriptionBatchService> parameterSubscriptionBatchService;
        private Mock<IParameterActualFiniteStateListApplicationBatchService> parameterActualFiniteStateListApplicationBatchService;
        private Mock<IChangeOwnershipBatchService> changeOwnershipBatchService;
        private Mock<ICDPMessageBus> messageBus;

        [SetUp]
        public void SetUp()
        {
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.fluentRibbonManager = new Mock<IFluentRibbonManager>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.parameterSubscriptionBatchService = new Mock<IParameterSubscriptionBatchService>();
            this.parameterActualFiniteStateListApplicationBatchService = new Mock<IParameterActualFiniteStateListApplicationBatchService>();
            this.changeOwnershipBatchService = new Mock<IChangeOwnershipBatchService>();
            this.messageBus = new Mock<ICDPMessageBus>();
        }

        [Test]
        public void VerifyThatServicesAreSetByConstructor()
        {
            var module = new EngineeringModelModule(this.fluentRibbonManager.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.thingDialogNavigationService.Object, null, this.parameterSubscriptionBatchService.Object, this.parameterActualFiniteStateListApplicationBatchService.Object, this.changeOwnershipBatchService.Object, this.messageBus.Object);

            Assert.That(this.fluentRibbonManager.Object, Is.EqualTo(module.RibbonManager));
            Assert.That(this.panelNavigationService.Object, Is.EqualTo(module.PanelNavigationService));
            Assert.That(this.dialogNavigationService.Object, Is.EqualTo(module.DialogNavigationService));
            Assert.That(this.thingDialogNavigationService.Object, Is.EqualTo(module.ThingDialogNavigationService));
            Assert.That(module.ParameterSubscriptionBatchService, Is.EqualTo(this.parameterSubscriptionBatchService.Object));
            Assert.That(module.ParameterActualFiniteStateListApplicationBatchService, Is.EqualTo(this.parameterActualFiniteStateListApplicationBatchService.Object));
            Assert.That(module.ChangeOwnershipBatchService, Is.EqualTo(this.changeOwnershipBatchService.Object));
            Assert.That(module.CDPMessageBus, Is.EqualTo(this.messageBus.Object));
        }
    }
}
