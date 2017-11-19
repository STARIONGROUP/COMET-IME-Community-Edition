// -------------------------------------------------------------------------------------------------
// <copyright file="PropertyGridViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4PropertyGrid.Tests
{
    using System.Reactive.Concurrency;

    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4PropertyGrid.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class PropertyGridViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        
        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var person = new Person();
            var vm = new PropertyGridViewModel(person, this.session.Object);

            Assert.IsNotNull(vm.Thing);
            Assert.IsNotNull(vm.Caption);
            Assert.IsNotNull(vm.ToolTip);
        }
    }
}