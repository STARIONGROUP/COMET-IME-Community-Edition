// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleParameterValueRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementBrowser
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4Requirements.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class SimpleParameterValueRowViewModelTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");
        private Requirement requirement;
        private SimpleParameterValue simpleParameterValue;
        private DateParameterType testParameterType;

        [SetUp]
        public void Setup()
        {
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();

            this.requirement = new Requirement(Guid.NewGuid(), null, null);

            this.simpleParameterValue = new SimpleParameterValue(Guid.NewGuid(), null, null)
            {
                Scale = new CyclicRatioScale { Name = "a", ShortName = "e" },
                ParameterType = new BooleanParameterType { Name = "a", ShortName = "a" }
            };

            this.testParameterType = new DateParameterType(Guid.NewGuid(), null, null) { Name = "testPT", ShortName = "tpt" };
            this.simpleParameterValue.ParameterType = this.testParameterType;
            var values = new List<string> { "1", "2" };
            this.simpleParameterValue.Value = new ValueArray<string>(values);

            this.requirement.ParameterValue.Add(this.simpleParameterValue);

            this.permissionService.Setup(x => x.CanWrite(ClassKind.Term, It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new SimpleParameterValueRowViewModel(this.simpleParameterValue, this.session.Object, null);

            Assert.AreEqual(this.testParameterType.Name, vm.Name);
            Assert.AreEqual($"{this.testParameterType.ShortName} [{vm.Scale.ShortName}]", vm.ShortName);
            vm.Scale = null;
            Assert.AreEqual(this.testParameterType.ShortName, vm.ShortName);
            Assert.That(vm.Definition, Is.Not.Null.Or.Empty);
            Assert.AreEqual("1, 2", vm.Definition);
        }
    }
}