// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
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

    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class ElementUsageDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private ThingTransaction thingTransaction;
        private readonly Uri uri = new Uri("http://test.com");

        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary srdl;
        private EngineeringModelSetup modelsetup;
        private ModelReferenceDataLibrary mrdl;
        private Category cat1;
        private Category cat2;
        private DomainOfExpertise domain1;

        private EngineeringModel model;
        private Iteration iteration;
        private Option option;
        private ElementDefinition definition1;
        private ElementDefinition definition2;
        private ElementUsage usage;
        private ElementUsage usageClone;
        private ElementDefinition definition1Clone;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.domain1 = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.cat1 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.cat1.PermissibleClass.Add(ClassKind.ElementUsage);
            this.cat2 = new Category(Guid.NewGuid(), this.cache, this.uri);

            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);
            this.siteDir.Model.Add(this.modelsetup);
            this.siteDir.Domain.Add(this.domain1);
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.mrdl.DefinedCategory.Add(this.cat2);
            this.srdl.DefinedCategory.Add(this.cat1);
            this.modelsetup.ActiveDomain.Add(this.domain1);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.definition1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            this.definition2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);

            this.usage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { ElementDefinition = this.definition2 };

            this.model.Iteration.Add(this.iteration);
            this.iteration.Option.Add(this.option);
            this.iteration.Element.Add(this.definition1);
            this.iteration.Element.Add(this.definition2);

            this.definition1.ContainedElement.Add(this.usage);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.cache.TryAdd(new CacheKey(this.definition1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.definition1));
            this.cache.TryAdd(new CacheKey(this.usage.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.usage));

            this.usageClone = this.usage.Clone(false);
            this.definition1Clone = this.definition1.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.definition1Clone);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new ElementUsageDialogViewModel(this.usageClone, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.definition1Clone);

            Assert.AreEqual(1, vm.PossibleExcludeOption.Count);
            Assert.AreEqual(1, vm.PossibleCategory.Count);
            Assert.AreEqual(1, vm.PossibleOwner.Count);

            vm.IncludeOption = new ReactiveList<Option>();
            Assert.AreEqual(1, vm.ExcludeOption.Count);
        }

        [Test]
        public void VerifyThatElementDefinitionCategoriesArePopulated()
        {
            var productCategory = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.definition2.Category.Add(productCategory);

            this.usageClone = this.usage.Clone(false);
            var vm = new ElementUsageDialogViewModel(this.usageClone, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.definition1Clone);

            Assert.That(vm.AppliedCategories.Any(x => x.Category == productCategory), Is.True);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new ElementUsageDialogViewModel());
        }

        [Test]
        public void VerifyThatExceptionIsThrownIfContainerIsNull()
        {
            Assert.Throws<InvalidOperationException>(() => new ElementUsageDialogViewModel(this.usageClone, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, null));
        }

        [Test]
        public async Task VerifyInspectReferencedElementDefinitionCommand()
        {
            var vm = new ElementUsageDialogViewModel(this.usageClone, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.definition1Clone);

            Assert.IsTrue(((ICommand)vm.InspectSelectedElementDefinitionCommand).CanExecute(null));
            await vm.InspectSelectedElementDefinitionCommand.Execute();
            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<ElementDefinition>(), It.IsAny<ThingTransaction>(), this.session.Object, false, ThingDialogKind.Inspect, this.thingDialogNavigationService.Object, It.IsAny<Thing>(), null));
        }
    }
}
