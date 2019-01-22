// -------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.SiteRdlBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4SiteDirectory.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="SiteRdlRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class SiteRdlRowViewModelTestFixture
    {
        private Assembler assembler;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary rdl1;
        private SiteReferenceDataLibrary rdl2;
        private SiteReferenceDataLibrary rdl21;
        private ModelReferenceDataLibrary mrdl;
        private Uri uri;
        private Mock<ISession> session;
        private Person person;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private PropertyInfo revInfo = typeof(Thing).GetProperty("RevisionNumber");

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "test" };

            this.rdl1 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "rdl1", ShortName = "1" };
            this.rdl2 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "rdl2", ShortName = "2" };
            this.rdl21 = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "rdl21", ShortName = "21", RequiredRdl = this.rdl2 };
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri)
                            {
                                Name = "Model RDL",
                                ShortName = "mrdl",
                                RequiredRdl = this.rdl21
                            };

            this.siteDir.SiteReferenceDataLibrary.Add(this.rdl1);
            this.siteDir.SiteReferenceDataLibrary.Add(this.rdl2);
            this.siteDir.SiteReferenceDataLibrary.Add(this.rdl21);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [Test]
        public void VerifyThatObjectChangeMessageIsCaught()
        {
            var row = new SiteRdlRowViewModel(this.rdl21, this.session.Object, null);
            Assert.IsTrue(row.CanClose);
            Assert.AreEqual("rdl2", row.RequiredRdlName);
            Assert.AreEqual("2", row.RequiredRdlShortName);

            this.rdl2.Name = "rdl2.1";
            this.rdl2.ShortName = "2.1";
            
            this.revInfo.SetValue(this.rdl21, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(this.rdl21, EventKind.Updated);
            Assert.AreEqual("rdl2.1", row.RequiredRdlName);
            Assert.AreEqual("2.1", row.RequiredRdlShortName);
        }
    }
}
