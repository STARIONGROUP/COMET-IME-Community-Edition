// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PersonDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;

    using CDP4SiteDirectory.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class PersonDialogViewModelTestFixture
    {
        private Uri uri;
        private Person person;
        private SiteDirectory siteDir;
        private Mock<ISession> session;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory clone;

        [SetUp]
        public void Setup()
        {
            this.uri = new Uri("http://www.rheagroup.com");
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "personRole", Password = "123" };
            this.siteDir.Person.Add(this.person);

            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
            this.cache.TryAdd(new CacheKey(this.person.Iid, null), new Lazy<Thing>(() => this.person));

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.clone = this.siteDir.Clone(false);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void Teardown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatValidationWorksOnShortName()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            vm.ShortName = "a";
            vm.GivenName = "b";
            vm.Surname = "c";

            Assert.That(vm["ShortName"], Is.Null.Or.Empty);
            Assert.That(vm.ValidationErrors.Count, Is.EqualTo(0));
            Assert.That(vm.OkCommand.CanExecute(null), Is.True);

            vm.ShortName = string.Empty;
            Assert.That(vm["ShortName"], Is.Not.Null.Or.Empty);

            Assert.That(vm.ValidationErrors.Count, Is.EqualTo(1));
            Assert.That(vm.OkCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void VerifyThatValidationWorksOnGivenName()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            vm.ShortName = "a";
            vm.GivenName = "b";
            vm.Surname = "c";

            Assert.That(vm["GivenName"], Is.Null.Or.Empty);
            Assert.That(vm.ValidationErrors.Count, Is.EqualTo(0));
            Assert.That(vm.OkCommand.CanExecute(null), Is.True);

            vm.GivenName = string.Empty;
            Assert.That(vm["GivenName"], Is.Not.Null.Or.Empty);

            Assert.That(vm.ValidationErrors.Count, Is.EqualTo(1));
            Assert.That(vm.OkCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void VerifyThatValidationWorksOnSurame()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            vm.ShortName = "a";
            vm.GivenName = "b";
            vm.Surname = "c";

            Assert.That(vm["Surname"], Is.Null.Or.Empty);
            Assert.That(vm.ValidationErrors.Count, Is.EqualTo(0));
            Assert.That(vm.OkCommand.CanExecute(null), Is.True);

            vm.Surname = string.Empty;
            Assert.That(vm["Surname"], Is.Not.Null.Or.Empty);

            Assert.That(vm.ValidationErrors.Count, Is.EqualTo(1));
            Assert.That(vm.OkCommand.CanExecute(null), Is.False);
        }
        
        [Test]
        public void VerifyThatValidationWorksOnPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            Assert.AreEqual(0, vm.ValidationErrors.Count);

            // assert 2 errors messages are added when password update is activated
            vm.PwdEditIsChecked = true;

            Assert.That(vm["Password"], Is.Not.Null.Or.Empty);
            Assert.That(vm["PasswordConfirmation"], Is.Not.Null.Or.Empty);

            Assert.AreEqual(2, vm.ValidationErrors.Count);

            // assert that the errors are removed upon good password match
            vm.Password = "123";
            vm.PasswordConfirmation = "123";

            Assert.That(vm["Password"], Is.Null.Or.Empty);
            Assert.That(vm["PasswordConfirmation"], Is.Null.Or.Empty);

            Assert.AreEqual(0, vm.ValidationErrors.Count);

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
        }

        [Test]
        public void VerifyThatUpdateTransactionDoesnotUpdatePassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            vm.OkCommand.Execute(null);
            var personclone = transaction.UpdatedThing.Select(x => x.Value).OfType<Person>().Single();

            Assert.AreEqual(this.person.Password, personclone.Password);
        }

        [Test]
        public void VerifyThatUpdateTransactionUpdatesPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            vm.PwdEditIsChecked = true;
            vm.Password = "456";

            vm.OkCommand.Execute(null);
            var personclone = transaction.UpdatedThing.Select(x => x.Value).OfType<Person>().Single();

            Assert.AreEqual("456", personclone.Password);
        }

        [Test]
        public void VerifyThatSetDefaultTelephoneNumberWorks()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);
            var pers = new Person(Guid.NewGuid(), this.cache, this.uri);
            var phone = new TelephoneNumber(Guid.NewGuid(), this.cache, this.uri);

            pers.TelephoneNumber.Add(phone);

            var vm = new PersonDialogViewModel(pers, transaction, this.session.Object, true,
                ThingDialogKind.Create, null, this.clone);

            vm.SelectedTelephoneNumber = vm.TelephoneNumber.Single();
            vm.SetDefaultTelephoneNumberCommand.Execute(null);

            Assert.AreSame(vm.SelectedDefaultTelephoneNumber, phone);
        }

        [Test]
        public void VerifyThatSetDefaultEmailAddressWorks()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var pers = new Person(Guid.NewGuid(), this.cache, this.uri);
            var email = new EmailAddress(Guid.NewGuid(), this.cache, this.uri);

            pers.EmailAddress.Add(email);

            var vm = new PersonDialogViewModel(pers, transaction, this.session.Object, true,
                ThingDialogKind.Create, null, this.clone);

            vm.SelectedEmailAddress = vm.EmailAddress.Single();
            vm.SetDefaultEmailAddressCommand.Execute(null);

            Assert.AreSame(vm.SelectedDefaultEmailAddress, email);
        }

        [Test]
        public void VerifyThatDefaultConstructorDoesNotThrowException()
        {
            var personDialogViewModel = new PersonDialogViewModel();
            Assert.IsNotNull(personDialogViewModel);
        }

        [Test]
        public void VerifyThatPasswordWarningIsDisplayedWhenCreatingUserWithoutPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Create, null, this.clone);

            vm.PwdEditIsChecked = false;

            Assert.IsTrue(vm.ShoudDisplayPasswordNotSetWarning);
        }

        [Test]
        public void VerifyThatPasswordWarningIsNotDisplayedWhenCreatingUserWithPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Create, null, this.clone);

            vm.PwdEditIsChecked = true;

            Assert.IsFalse(vm.ShoudDisplayPasswordNotSetWarning);
        }

        [Test]
        public void VerifyThatPasswordWarningIsNotDisplayedWhenEditingUserWithoutPassword()
        {
            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            var transaction = new ThingTransaction(transactionContext, this.clone);

            var vm = new PersonDialogViewModel(this.person.Clone(false), transaction, this.session.Object, true,
                ThingDialogKind.Update, null, this.clone);

            vm.PwdEditIsChecked = false;

            Assert.IsFalse(vm.ShoudDisplayPasswordNotSetWarning);
        }
    }
}
