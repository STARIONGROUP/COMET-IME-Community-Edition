// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
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

    using CDP4EngineeringModel.ViewModels.Dialogs;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class MultiRelationshipDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> dialogNavigationService;
        private Mock<IPermissionService> permissionService;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private SiteDirectory siteDir;
        private SiteReferenceDataLibrary srdl;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;
        private EngineeringModel model;
        private Iteration iteration;
        private ModelReferenceDataLibrary mrdl;

        private DomainOfExpertise domain;

        private Category relationshipCat;
        private Category requirementCat1;
        private Category requirementCat2;

        private RequirementsSpecification reqSpec;
        private Requirement req1;
        private Requirement req2;

        private readonly Uri uri = new Uri("http://test.com");
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.dialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            this.siteDir.Domain.Add(this.domain);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup.RequiredRdl.Add(this.mrdl);
            this.modelsetup.ActiveDomain.Add(this.domain);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.siteDir.SiteReferenceDataLibrary.Add(this.srdl);

            this.relationshipCat = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.relationshipCat.PermissibleClass.Add(ClassKind.BinaryRelationship);
            this.requirementCat1 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.requirementCat1.PermissibleClass.Add(ClassKind.Requirement);
            this.requirementCat2 = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.requirementCat2.PermissibleClass.Add(ClassKind.Requirement);

            this.srdl.DefinedCategory.Add(this.relationshipCat);
            this.srdl.DefinedCategory.Add(this.requirementCat1);
            this.srdl.DefinedCategory.Add(this.requirementCat2);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationSetup };
            this.model.Iteration.Add(this.iteration);

            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.RequirementsSpecification.Add(this.reqSpec);

            this.req1 = new Requirement(Guid.NewGuid(), this.cache, this.uri);
            this.req1.Category.Add(this.requirementCat1);
            this.req1.ClassKind = ClassKind.ActionItem;
            this.req2 = new Requirement(Guid.NewGuid(), this.cache, this.uri);
            this.req2.Category.Add(this.requirementCat2);
            this.req2.ClassKind = ClassKind.ActionItem;

            this.reqSpec.Requirement.Add(this.req1);
            this.reqSpec.Requirement.Add(this.req2);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            var assembler = new Assembler(this.uri, this.messageBus);
            this.session.Setup(x => x.Assembler).Returns(assembler);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public async Task VerifyThatDialogWorks()
        {
            var clone = this.iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            var transaction = new ThingTransaction(transactionContext, clone);

            var vm = new MultiRelationshipDialogViewModel(new MultiRelationship(), transaction, this.session.Object,
                true, ThingDialogKind.Create, this.dialogNavigationService.Object, clone);

            Assert.AreEqual(0, vm.RelatedThingRows.Count);
            Assert.AreEqual(null, vm.Name);
            Assert.AreEqual(null, vm.SelectedRelatedThingRow);

            vm.Category = new ReactiveList<Category> { this.relationshipCat };
            Assert.AreEqual(1, vm.Category.Count);

            Assert.IsTrue(vm.PossibleOwner.Contains(this.domain));
            vm.SelectedOwner = this.domain;

            Assert.That(((ICommand)vm.CreateThingEntryCommand).CanExecute(null), Is.True);
            Assert.That(((ICommand)vm.DeleteRowCommand).CanExecute(null), Is.True);
            Assert.That(((ICommand)vm.OkCommand).CanExecute(null), Is.True);
            Assert.That(((ICommand)vm.CancelCommand).CanExecute(null), Is.True);
            Assert.That(vm.OkCanExecute, Is.True);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new MultiRelationshipDialogViewModel());
        }
    }
}
