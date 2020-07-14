// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace CDP4Grapher.Tests.ViewModels
{
    using System.Linq;
    using System.Threading;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Grapher.Tests.Data;
    using CDP4Grapher.ViewModels;

    using Moq;

    using NUnit.Framework;

    using Assert = NUnit.Framework.Assert;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class GrapherViewModelTestFixture : GrapherBaseTestData
    {
        private Mock<IThingDialogNavigationService> thingNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPluginSettingsService> pluginSettingService;
        private Mock<IHaveContextMenu> diagramControlContextMenu;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            this.thingNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.pluginSettingService = new Mock<IPluginSettingsService>();
            this.diagramControlContextMenu = new Mock<IHaveContextMenu>();
        }

        [Test]
        public void VerifyProperties()
        {
            var vm = new GrapherViewModel(this.Option, this.Session.Object, this.thingNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingService.Object)
            {
                DiagramContextMenuViewModel = this.diagramControlContextMenu.Object
            };

            Assert.IsNotEmpty(vm.GraphElements);
            Assert.IsNotNull(vm.DiagramContextMenuViewModel);

            Assert.IsTrue(vm.GraphElements.All(x =>
                x.NestedElementElement.Iid == this.TopElement.Iid ||
                x.NestedElementElement.Iid == this.ElementUsage1.Iid ||
                x.NestedElementElement.Iid == this.ElementUsage2.Iid ||
                x.NestedElementElement.Iid == this.ElementUsage3.Iid
            ));

            Assert.IsNotNull(vm.CurrentModel);
            Assert.IsNotNull(vm.CurrentIteration);
            Assert.IsNotNull(vm.CurrentOption);
        }

        [Test]
        public void VerifySubscription()
        {
            var vm = new GrapherViewModel(this.Option, this.Session.Object, this.thingNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingService.Object);
            this.EngineeringModelSetup.ShortName = "updatedShortName";
            CDPMessageBus.Current.SendObjectChangeEvent(((EngineeringModel)this.Option.TopContainer).EngineeringModelSetup, EventKind.Updated);
            Assert.AreEqual(this.EngineeringModelSetup.ShortName, ((EngineeringModel)this.Option.TopContainer).EngineeringModelSetup.ShortName);
        }
    }
}
