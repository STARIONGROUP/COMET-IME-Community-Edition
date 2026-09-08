// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerationParameterTypeDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace BasicRdl.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class EnumerationParameterTypeDialogViewModelTestFixture
    {
        private EnumerationParameterTypeDialogViewModel viewmodel;
        private EnumerationParameterType enumerationParameterType;
        private SiteDirectory siteDir;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            var person = new Person(Guid.NewGuid(), null, null) { Container = this.siteDir };
            this.session.Setup(x => x.ActivePerson).Returns(person);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, null);
            this.siteDir.Person.Add(person);
            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, null) { Name = "testRDL", ShortName = "test" };
            this.enumerationParameterType = new EnumerationParameterType(Guid.NewGuid(), null, null) { Name = "enumerationParameterType", ShortName = "cat" };
            var testValueDefinition = new EnumerationValueDefinition { Name = "definition1", ShortName = "def" };
            this.enumerationParameterType.ValueDefinition.Add(testValueDefinition);

            this.siteDir.SiteReferenceDataLibrary.Add(rdl);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDir);
            this.transaction = new ThingTransaction(transactionContext, null);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.OpenReferenceDataLibraries).Returns(new HashSet<ReferenceDataLibrary>(this.siteDir.SiteReferenceDataLibrary));

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new EnumerationParameterTypeDialogViewModel(this.enumerationParameterType, this.transaction, this.session.Object, true, ThingDialogKind.Create, this.navigation.Object);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.enumerationParameterType.Name);
            Assert.AreEqual(this.viewmodel.ShortName, this.enumerationParameterType.ShortName);
            Assert.AreEqual(this.viewmodel.IsDeprecated, this.enumerationParameterType.IsDeprecated);
            Assert.AreEqual(this.viewmodel.Symbol, this.enumerationParameterType.Symbol);
            Assert.IsNotEmpty(this.viewmodel.ValueDefinition);
            Assert.IsNotEmpty(this.viewmodel.PossibleContainer);
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            Assert.AreEqual(1, this.viewmodel.ValueDefinition.Count);
            Assert.IsTrue(((ICommand)this.viewmodel.OkCommand).CanExecute(null));
            this.viewmodel.ValueDefinition.Clear();
            Assert.IsFalse(((ICommand)this.viewmodel.OkCommand).CanExecute(null));
        }

        [Test]
        public void VerifyThatGuidanceIsShownWhenValueDefinitionListIsEmpty()
        {
            Assert.AreEqual(1, this.viewmodel.ValueDefinition.Count);
            Assert.IsFalse(this.viewmodel.IsValueDefinitionListEmpty);

            this.viewmodel.ValueDefinition.Clear();
            Assert.IsTrue(this.viewmodel.IsValueDefinitionListEmpty);
        }

        [Test]
        public void VerifyDialogValidation()
        {
            Assert.AreEqual(0, this.viewmodel.ValidationErrors.Count);
            Assert.That(this.viewmodel["Symbol"], Is.Not.Null.Or.Not.Empty);

            this.viewmodel.Symbol = "something";
            Assert.That(this.viewmodel["Symbol"], Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new EnumerationParameterTypeDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsDeprecated);
        }

        [Test]
        public void VerifValueDefinitionCommands()
        {
            Assert.IsTrue(((ICommand)this.viewmodel.CreateValueDefinitionCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.InspectValueDefinitionCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.EditValueDefinitionCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.DeleteValueDefinitionCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.MoveUpValueDefinitionCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.viewmodel.MoveDownValueDefinitionCommand).CanExecute(null));

            this.viewmodel.SelectedValueDefinition = this.viewmodel.ValueDefinition.First();

            Assert.IsTrue(((ICommand)this.viewmodel.InspectValueDefinitionCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.viewmodel.EditValueDefinitionCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.viewmodel.DeleteValueDefinitionCommand).CanExecute(null));

            Assert.IsTrue(((ICommand)this.viewmodel.MoveUpValueDefinitionCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.viewmodel.MoveDownValueDefinitionCommand).CanExecute(null));
        }

        [Test]
        public async Task VerifyInspectValueDefinition()
        {
            this.viewmodel.SelectedValueDefinition = this.viewmodel.ValueDefinition.First();
            Assert.IsTrue((this.viewmodel.InspectValueDefinitionCommand as ICommand).CanExecute(null));
            await this.viewmodel.InspectValueDefinitionCommand.Execute();
            this.navigation.Verify(x => x.Navigate(It.IsAny<EnumerationValueDefinition>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.navigation.Object, It.IsAny<Thing>(), null));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task VerifyThatDeletingAValueDefinitionDoesNotReorderTheRemainingOnes(int indexToDelete)
        {
            var dialog = this.CreateCachedEnumerationParameterTypeDialog(out var parameterTypeClone, out var valueDefinitions);

            var expectedSortKeys = parameterTypeClone.ValueDefinition.SortedItems.Select(x => new { x.Key, x.Value.Iid }).ToList();
            var deletedValueDefinition = valueDefinitions[indexToDelete];

            dialog.SelectedValueDefinition = dialog.ValueDefinition.Single(x => x.Thing == deletedValueDefinition);
            await dialog.DeleteValueDefinitionCommand.Execute();

            // the generated Delete command delivers its handler asynchronously, wait for the row of the deleted EnumerationValueDefinition to disappear
            Assert.That(() => dialog.ValueDefinition.Select(x => x.Thing).Contains(deletedValueDefinition), Is.False.After(2000, 25), "the EnumerationValueDefinition was not deleted");

            await dialog.OkCommand.Execute();

            // the sort keys of the OrderedItemList may not change, a reordered EnumerationValueDefinition that is deleted in the
            // same transaction cannot be resolved by the data-source, see https://github.com/STARIONGROUP/COMET-IME-Community-Edition/issues/1475
            Assert.That(parameterTypeClone.ValueDefinition.SortedItems.Select(x => new { x.Key, x.Value.Iid }).ToList(), Is.EqualTo(expectedSortKeys));
        }

        [Test]
        public async Task VerifyThatReorderingAfterADeleteKeepsTheSortKeyOfTheDeletedValueDefinition()
        {
            var dialog = this.CreateCachedEnumerationParameterTypeDialog(out var parameterTypeClone, out var valueDefinitions);

            var sortKeys = parameterTypeClone.ValueDefinition.SortedItems.Keys.ToList();
            var deletedValueDefinition = valueDefinitions[1];

            dialog.SelectedValueDefinition = dialog.ValueDefinition.Single(x => x.Thing == deletedValueDefinition);
            await dialog.DeleteValueDefinitionCommand.Execute();

            Assert.That(() => dialog.ValueDefinition.Select(x => x.Thing).Contains(deletedValueDefinition), Is.False.After(2000, 25), "the EnumerationValueDefinition was not deleted");

            dialog.SelectedValueDefinition = dialog.ValueDefinition.Single(x => x.Thing == valueDefinitions[2]);
            await dialog.MoveUpValueDefinitionCommand.Execute();

            Assert.That(() => dialog.ValueDefinition.First().Thing, Is.EqualTo(valueDefinitions[2]).After(2000, 25), "the EnumerationValueDefinition was not moved up");

            await dialog.OkCommand.Execute();

            // the deleted EnumerationValueDefinition keeps its sort key, the remaining ones are laid out over the other sort keys
            // in the order in which they appear in the dialog
            var expected = new[]
            {
                new { Key = sortKeys[0], valueDefinitions[2].Iid },
                new { Key = sortKeys[1], deletedValueDefinition.Iid },
                new { Key = sortKeys[2], valueDefinitions[0].Iid }
            };

            Assert.That(parameterTypeClone.ValueDefinition.SortedItems.Select(x => new { x.Key, x.Value.Iid }).ToList(), Is.EqualTo(expected));
        }

        /// <summary>
        /// Creates a <see cref="EnumerationParameterTypeDialogViewModel"/> in Update mode for a cached <see cref="EnumerationParameterType"/>
        /// that contains three <see cref="EnumerationValueDefinition"/>s. The <see cref="Thing"/>s are present in the cache, this makes the
        /// <see cref="ThingTransaction"/> record them as updates and deletes, as it does when the dialog is opened from a browser.
        /// </summary>
        /// <param name="parameterTypeClone">
        /// The clone of the <see cref="EnumerationParameterType"/> that is the subject of the returned dialog view-model
        /// </param>
        /// <param name="valueDefinitions">
        /// The original <see cref="EnumerationValueDefinition"/>s, in the order in which they are contained by the <see cref="EnumerationParameterType"/>
        /// </param>
        /// <returns>
        /// The <see cref="EnumerationParameterTypeDialogViewModel"/>
        /// </returns>
        private EnumerationParameterTypeDialogViewModel CreateCachedEnumerationParameterTypeDialog(out EnumerationParameterType parameterTypeClone, out List<EnumerationValueDefinition> valueDefinitions)
        {
            var uri = new Uri("http://test.com");
            var cache = new Assembler(uri, new CDPMessageBus()).Cache;

            var rdl = new SiteReferenceDataLibrary(Guid.NewGuid(), cache, uri) { Name = "cachedRDL", ShortName = "cachedRDL", Container = this.siteDir };
            var parameterType = new EnumerationParameterType(Guid.NewGuid(), cache, uri) { Name = "enumeration", ShortName = "enumeration", Symbol = "-" };

            valueDefinitions = new[] { "first", "middle", "last" }
                .Select(name => new EnumerationValueDefinition(Guid.NewGuid(), cache, uri) { Name = name, ShortName = name })
                .ToList();

            foreach (var valueDefinition in valueDefinitions)
            {
                parameterType.ValueDefinition.Add(valueDefinition);
            }

            rdl.ParameterType.Add(parameterType);
            this.siteDir.SiteReferenceDataLibrary.Add(rdl);

            foreach (var thing in new Thing[] { rdl, parameterType }.Concat(valueDefinitions))
            {
                cache.TryAdd(thing.CacheKey, new Lazy<Thing>(() => thing));
            }

            var rdlClone = rdl.Clone(false);
            parameterTypeClone = parameterType.Clone(false);
            var thingTransaction = new ThingTransaction(TransactionContextResolver.ResolveContext(this.siteDir), rdlClone);

            return new EnumerationParameterTypeDialogViewModel(parameterTypeClone, thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.navigation.Object, rdlClone);
        }
    }
}
