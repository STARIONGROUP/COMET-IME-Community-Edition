// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace ProductTree.Tests.ProductTreeRows
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4ProductTree.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class ParameterGroupRowViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");
        private ParameterGroup group1;

        [SetUp]
        public void Setup()
        {
            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();

            this.group1 = new ParameterGroup(Guid.NewGuid(), null, this.uri) {Name = "group1"};
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new ParameterGroupRowViewModel(this.group1, this.session.Object, null);
            
            Assert.AreEqual("group1", vm.Name);
            Assert.AreEqual("group1", vm.ShortName);

        }
    }
}