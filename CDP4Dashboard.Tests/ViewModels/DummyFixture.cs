// -------------------------------------------------------------------------------------------------
// <copyright file=DummyFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.ViewModels.Rows
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Tests several different ViewModels to check if <see cref="RequirementStateOfCompliance"/> is set correctly
    /// </summary>
    [TestFixture]
    internal class DummyFixture
    {
        private Mock<ISession> session;
        private readonly PropertyInfo revision = typeof(Thing).GetProperty("RevisionNumber");
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Mock<IPermissionService> permissionService;
        private Assembler assembler;
        private DomainOfExpertise domain;

        [SetUp]
        public void Setup()
        {
            this.assembler = new Assembler(this.uri);

            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "test" };
         }
    }
}
