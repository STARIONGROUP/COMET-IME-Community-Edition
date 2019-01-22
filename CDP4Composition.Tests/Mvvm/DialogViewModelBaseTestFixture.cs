// -------------------------------------------------------------------------------------------------
// <copyright file="DialogViewModelBaseTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm
{
    using System;
    using System.Windows;
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class DialogViewModelBaseTestFixture
    {
        private Uri uri;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;
        private Mock<IDal> dal;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory siteDir;
        private Person person;
        private ThingTransaction transaction;
        private SiteDirectory clone;

        private string siteDirectoryId = "609d5d8f-f209-4905-967e-796970fefd84";
        private string personId = "f1cbaf64-afa6-4467-97e5-5d98803f2848";

        [SetUp]
        public void Setup()
        {
            this.uri = new Uri("http://www.rheagroup.com");
            this.session = new Mock<ISession>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.permissionService = new Mock<IPermissionService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            this.dal = new Mock<IDal>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>())
                .Returns(this.navigation.Object);

            this.siteDir = new SiteDirectory(Guid.Parse(this.siteDirectoryId), this.cache, this.uri) { Name = "site directory" };
            this.person = new Person(Guid.Parse(this.personId), null, this.uri);
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));

            this.siteDir.Person.Add(this.person);
            this.clone = this.siteDir.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, this.clone);

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(this.dal.Object);
            this.dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifThatThingUriReturnsExpectedResult()
        {
            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, true, ThingDialogKind.Update, this.navigation.Object, this.clone);

            Assert.AreEqual("http://www.rheagroup.com:80/SiteDirectory/609d5d8f-f209-4905-967e-796970fefd84/person/f1cbaf64-afa6-4467-97e5-5d98803f2848", testdialog.ThingUri);
        }

        [Test, Apartment(ApartmentState.STA)]
        public void VerifThatCopyUriCommandWorks()
        {
            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, true, ThingDialogKind.Update, this.navigation.Object, this.clone);
            testdialog.CopyUriCommand.Execute(true);
            Assert.AreEqual(testdialog.ThingUri,Clipboard.GetText());
        }

        [Test]
        public void VerifThatThingUriReturnsExpectedResult2()
        {
            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, this.clone);

            Assert.AreEqual("N/A", testdialog.ThingUri);
        }

        [Test, Apartment(ApartmentState.STA)]
        public void VerifThatJsonExportCommandsWork()
        {
            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, true, ThingDialogKind.Update, this.navigation.Object, this.clone);
            testdialog.ShallowExportCommand.Execute(true);
            Assert.IsTrue(Clipboard.GetText().Contains(this.personId));
            testdialog.DeepExportCommand.Execute(true);
            Assert.IsTrue(Clipboard.GetText().Contains(this.personId));
        }

        [Test]
        public void VerifyThatIfContainmentIsNotSetThingUriContainsErrorMessage()
        {
            var guid = Guid.Parse("e7443063-a51f-49ba-8add-c3f296a24636");

            var otherPerson = new Person(guid, this.cache, this.uri);

            var testdialog = new TestDialogViewModel(otherPerson, this.transaction, this.session.Object, true, ThingDialogKind.Inspect, this.navigation.Object, this.clone);

            var expectedMessage = "The container of Person with iid e7443063-a51f-49ba-8add-c3f296a24636 is null, the route cannot be computed.";

            Assert.AreEqual(expectedMessage, testdialog.ThingUri);
        }

        [Test]
        public void VerifyThatExecuteOkOnRootWork()
        {
            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object, this.clone);

            testdialog.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatExecuteOkOnNotRootWork()
        {
            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.clone);

            testdialog.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);
        }

        [Test]
        public void VerifyThatExecuteCancelWork()
        {
            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.clone);

            testdialog.CancelCommand.Execute(null);
            Assert.IsFalse(testdialog.DialogResult.Value);
            Assert.AreEqual(1, transaction.UpdatedThing.Count);
            Assert.AreEqual(0, transaction.AddedThing.Count());
            Assert.AreEqual(0, transaction.DeletedThing.Count());
        }

        [Test]
        public void VerifyThatCanExecuteOkCommandWorks()
        {
            // test the validation binding OkCanExecute

            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.clone);

            testdialog.Name = "";            
            Assert.That(testdialog["Name"], Is.Not.Null.Or.Not.Empty);

            Assert.IsFalse(testdialog.OkCommand.CanExecute(null));

            testdialog.Name = "ara";
            Assert.That(testdialog["Name"], Is.Null.Or.Empty);

            Assert.IsTrue(testdialog.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatExecuteCreateWorks()
        {
            this.navigation.Setup(
                x =>
                    x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), It.IsAny<ISession>(), It.IsAny<bool>(),
                        It.IsAny<ThingDialogKind>(), this.navigation.Object, It.IsAny<Thing>(), It.IsAny<IEnumerable<Thing>>())).Returns(true);

            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.clone);

            testdialog.CreateCommand.Execute(null);

            this.navigation.Verify(x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), It.IsAny<ISession>(), It.IsAny<bool>(),
                        It.IsAny<ThingDialogKind>(), this.navigation.Object, It.IsAny<Thing>(), It.IsAny<IEnumerable<Thing>>()));

            Assert.IsTrue(testdialog.PopulateCalled);
        }

        [Test]
        public void VerifyThatExecuteCreateWorksOnCancel()
        {
            this.navigation.Setup(
                x =>
                    x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), It.IsAny<ISession>(), It.IsAny<bool>(),
                        It.IsAny<ThingDialogKind>(), this.navigation.Object, It.IsAny<Thing>(), It.IsAny<IEnumerable<Thing>>())).Returns(false);

            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.clone);

            testdialog.CreateCommand.Execute(null);

            this.navigation.Verify(x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), It.IsAny<ISession>(), It.IsAny<bool>(),
                        It.IsAny<ThingDialogKind>(), this.navigation.Object, It.IsAny<Thing>(), It.IsAny<IEnumerable<Thing>>()));

            Assert.IsFalse(testdialog.PopulateCalled);
        }

        [Test]
        public void VerifyThatIfSpellDictionaryCultureIsChangedThatSpellChekerLanguageIsUpdated()
        {
            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.clone);
            Assert.IsNotNull(testdialog.SpellChecker);

        }

        [Test]
        public void VeriufyThatMoveUpDownWorks()
        {
            var testdialog = new TestDialogViewModel(this.person, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.clone);
            var row1 = new OrderedRow(new Person(Guid.Empty, null, null), this.session.Object);
            var row2 = new OrderedRow(new Person(Guid.Empty, null, null), this.session.Object);
            var row3 = new OrderedRow(new Person(Guid.Empty, null, null), this.session.Object);
            var row4 = new OrderedRow(new Person(Guid.Empty, null, null), this.session.Object);
            testdialog.OrderedRows.Add(row1);
            testdialog.OrderedRows.Add(row2);
            testdialog.OrderedRows.Add(row3);
            testdialog.OrderedRows.Add(row4);

            testdialog.MoveDown(row1);

            Assert.AreSame(row2, testdialog.OrderedRows[0]);
            Assert.AreSame(row1, testdialog.OrderedRows[1]);
            Assert.AreSame(row3, testdialog.OrderedRows[2]);
            Assert.AreSame(row4, testdialog.OrderedRows[3]);

            testdialog.MoveDown(row1);
            testdialog.MoveDown(row1);

            Assert.AreSame(row2, testdialog.OrderedRows[0]);
            Assert.AreSame(row3, testdialog.OrderedRows[1]);
            Assert.AreSame(row4, testdialog.OrderedRows[2]);
            Assert.AreSame(row1, testdialog.OrderedRows[3]);

            testdialog.MoveUp(row4);
            testdialog.MoveUp(row4);

            Assert.AreSame(row4, testdialog.OrderedRows[0]);
            Assert.AreSame(row2, testdialog.OrderedRows[1]);
            Assert.AreSame(row3, testdialog.OrderedRows[2]);
            Assert.AreSame(row1, testdialog.OrderedRows[3]);
        }
    }

    internal class OrderedRow : RowViewModelBase<Person>
    {
        public OrderedRow(Person person, ISession session)
            : base(person, session, null)
        {
        }
    }

    internal class TestDialogViewModel : DialogViewModelBase<Person>
    {
        private string name;

        public TestDialogViewModel(Person person, IThingTransaction transaction, ISession session, bool isRoot,
            ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNav, Thing container)
            : base(person, transaction, session, isRoot, dialogKind, thingDialogNav, container, null)
        {
            this.UpdateTransactionCalled = false;
            this.PopulateCalled = false;
            this.CreateCommand = ReactiveCommand.Create();
            this.CreateCommand.Subscribe(_ => this.ExecuteCreateCommand<TelephoneNumber>(this.Populate));
            this.Phone = new ContainerList<TelephoneNumber>(this.Thing);
            this.OrderedRows = new ReactiveList<OrderedRow>();
        }

        public ContainerList<TelephoneNumber> Phone { get; private set; } 
        public bool UpdateTransactionCalled;
        public bool PopulateCalled;

        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        public ReactiveCommand<object> CreateCommand { get; private set; }

        public ReactiveList<OrderedRow> OrderedRows { get; private set; } 

        protected override void UpdateTransaction()
        {
            this.UpdateTransactionCalled = true;
        }

        private void Populate()
        {
            this.PopulateCalled = true;
        }

        public void MoveDown(OrderedRow row)
        {
            this.ExecuteMoveDownCommand(this.OrderedRows, row);
        }

        public void MoveUp(OrderedRow row)
        {
            this.ExecuteMoveUpCommand(this.OrderedRows, row);
        }
    }
}