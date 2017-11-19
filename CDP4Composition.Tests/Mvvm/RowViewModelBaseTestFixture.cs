// -------------------------------------------------------------------------------------------------
// <copyright file="RowViewModelBaseTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using System;

    using ReactiveUI;

    [TestFixture]
    public class RowViewModelBaseTestFixture
    {
        private SiteDirectory siteDir;
        private Person person;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;

        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.person = new Person(Guid.NewGuid(), this.cache, null);

            this.siteDir.Person.Add(this.person);

            this.navigation = new Mock<IPanelNavigationService>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatIdataErrorValidationWorks()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.ShortName = "a";
            Assert.IsNullOrEmpty(row["ShortName"]);
        }

        [Test]
        public void VerifyThatValidatePropertyWorks()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.ShortName = "a";
            Assert.IsNullOrEmpty(row.ValidateProperty("ShortName", "a"));
            Assert.IsNotNullOrEmpty(row.ValidateProperty("ShortName", "---"));
        }

        [Test]
        public void VerifyThatUpdatePersonMsgIsCaught()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            Assert.IsTrue(row.isUpdatePropertyCalled);

            row.isUpdatePropertyCalled = false;

            var rev = typeof (Thing).GetProperty("RevisionNumber");
            rev.SetValue(this.person, 50);

            CDPMessageBus.Current.SendObjectChangeEvent(this.person, EventKind.Updated);
            Assert.IsTrue(row.isUpdatePropertyCalled);
        }

        [Test]
        public void VerifyCreateCloneAndWriteWorks()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.CreateCloneAndWrite("abc", "ShortName");
            
            this.session.Verify(x => x.Write(It.Is<OperationContainer>(op => ((CDP4Common.DTO.Person)op.Operations.Single().ModifiedThing).ShortName == "abc")));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void VerifyThatCreateCloneThrows()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.CreateCloneAndWrite("abc", "Exception");
        }

        [Test]
        public void VerifyThatUpdatePropertyIsCalledOnDalError()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));
            var row = new RowTestClass(this.person, this.session.Object);

            row.isUpdatePropertyCalled = false;
            row.CreateCloneAndWrite("abc", "ShortName");

            Assert.IsTrue(row.HasError);
            Assert.IsNotNullOrEmpty(row.ErrorMsg);
            Assert.IsTrue(row.isUpdatePropertyCalled);

            row.Dispose();
        }

        [Test]
        public void VerifyThatRowTypeIsCorrect()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            Assert.AreEqual("Person", row.RowType);
        }

        [Test]
        public void VerifyThatComputeGenericRowWork()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.ComputeRow(new List<Person>{this.person});
            Assert.AreEqual(1, row.ContainedRows.Count);

            row.ComputeRow(new List<Person>());
            Assert.AreEqual(0, row.ContainedRows.Count);
        }

        internal class RowTestClass : RowViewModelBase<Person>
        {
            internal RowTestClass(Person person, ISession session)
                : base(person, session, null)
            {
                this.UpdateProperties();
            }

            public bool isUpdatePropertyCalled { get; set; }

            public string ShortName { get; set; }

            public void ComputeRow(IEnumerable<Person> currentList)
            {
                this.ComputeRows(currentList, this.ComputeTestRow);
            }

            /// <summary>
            /// The object changed event handler
            /// </summary>
            /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
            protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
            {
                base.ObjectChangeEventHandler(objectChange);
                this.UpdateProperties();
            }

            private void UpdateProperties()
            {
                this.isUpdatePropertyCalled = true;

            }

            private void ComputeTestRow(Person person)
            {
                var row = new RowTestClass(person, this.Session);
                this.ContainedRows.Add(row);
            }
        }
    }
}