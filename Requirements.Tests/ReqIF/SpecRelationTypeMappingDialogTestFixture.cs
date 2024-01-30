// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpecRelationTypeMappingDialogTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Requirements.Tests.ReqIF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.ViewModels;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    using ReqIFSharp;

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal class SpecRelationTypeMappingDialogTestFixture
    {
        /// <summary>
        /// The <see cref="IDialogNavigationService"/>
        /// </summary>
        private Mock<IDialogNavigationService> dialogNavigationService;

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/>
        /// </summary>
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        /// <summary>
        /// The <see cref="ISession"/> in which are information shall be written
        /// </summary>
        private Mock<ISession> session;

        private Mock<IPermissionService> permissionService;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;
        private DomainOfExpertise domain;
        private EngineeringModel model;
        private Iteration iteration;
        private Person person;
        private Participant participant;

        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        private Assembler assembler;
        private Uri uri = new Uri("http://test.com");

        private ReqIF reqIf;
        private DatatypeDefinitionString stringDatadef;
        private SpecRelationType spectype;
        private AttributeDefinitionString attribute;

        private ParameterType pt;

        private SpecRelationTypeMappingDialogViewModel dialog;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.modelsetup.RequiredRdl.Add(this.mrdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };

            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationSetup);
            this.sitedir.Domain.Add(this.domain);
            this.model.Iteration.Add(this.iteration);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.sitedir.Person.Add(this.person);
            this.modelsetup.Participant.Add(this.participant);

            this.pt = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl.ParameterType.Add(this.pt);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.assembler.Cache.TryAdd(new CacheKey(this.sitedir.Iid, null), new Lazy<Thing>(() => this.sitedir));
            this.reqIf = new ReqIF();
            this.reqIf.Lang = "en";
            var corecontent = new ReqIFContent();
            this.reqIf.CoreContent = corecontent;
            this.stringDatadef = new DatatypeDefinitionString();
            this.spectype = new SpecRelationType();
            this.attribute = new AttributeDefinitionString() { Identifier = Guid.NewGuid().ToString(), DatatypeDefinition = this.stringDatadef };

            this.spectype.SpecAttributes.Add(this.attribute);

            corecontent.DataTypes.Add(this.stringDatadef);

            this.dialog = new SpecRelationTypeMappingDialogViewModel(new List<SpecRelationType> { this.spectype }, null, new Dictionary<DatatypeDefinition, DatatypeDefinitionMap> { { this.stringDatadef, new DatatypeDefinitionMap(this.stringDatadef, this.pt, null) } }, this.iteration, this.session.Object, this.thingDialogNavigationService.Object, "en");
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [Test]
        public async Task VerifyThatCreateCategoryWorks()
        {
            this.thingDialogNavigationService.Setup(
                x =>
                    x.Navigate(
                        It.IsAny<Category>(),
                        It.IsAny<IThingTransaction>(),
                        this.session.Object,
                        true,
                        ThingDialogKind.Create,
                        this.thingDialogNavigationService.Object,
                        null,
                        null)).Returns(true);

            await this.dialog.CreateCategoryCommand.Execute();

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<Category>(),
                It.IsAny<IThingTransaction>(),
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                null,
                null));
        }

        [Test]
        public async Task VerifyThatCreateRuleWorks()
        {
            this.thingDialogNavigationService.Setup(
                x =>
                    x.Navigate(
                        It.IsAny<BinaryRelationshipRule>(),
                        It.IsAny<IThingTransaction>(),
                        this.session.Object,
                        true,
                        ThingDialogKind.Create,
                        this.thingDialogNavigationService.Object,
                        null,
                        null)).Returns(true);

            await this.dialog.CreateBinaryRealationshipRuleCommand.Execute();

            this.thingDialogNavigationService.Verify(x => x.Navigate(It.IsAny<BinaryRelationshipRule>(),
                It.IsAny<IThingTransaction>(),
                this.session.Object,
                true,
                ThingDialogKind.Create,
                this.thingDialogNavigationService.Object,
                null,
                null));
        }

        [Test]
        public async Task VerifyThatCancelCommandWorks()
        {
            await this.dialog.CancelCommand.Execute();
            Assert.IsFalse(this.dialog.DialogResult.Result.Value);
        }

        [Test]
        public async Task VerifyThatBackCommandWorks()
        {
            await this.dialog.BackCommand.Execute();
            var res = this.dialog.DialogResult as RelationshipMappingDialogResult;
            Assert.IsNotNull(res);
            Assert.IsFalse(res.GoNext.Value);
            Assert.IsTrue(res.Result.Value);
        }

        [Test]
        public async Task VerifyThatExecuteNextWorks()
        {
            var specificationRow = this.dialog.SpecTypes.First();
            specificationRow.AttributeDefinitions.First().AttributeDefinitionMapKind = AttributeDefinitionMapKind.NAME;

            await this.dialog.OkCommand.Execute();
            var res = this.dialog.DialogResult as RelationshipMappingDialogResult;
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Result.Value);
            Assert.IsTrue(res.GoNext.Value);
            Assert.IsNotEmpty(res.Map);
        }

        [Test]
        public void VerifyThatExistingMapIsApplied()
        {
            var category = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var rule = new ParameterizedCategoryRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { Category = category };
            var binaryRelationshipRule = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { RelationshipCategory = category, TargetCategory = category, SourceCategory = category };
            var categoryVm = new CategoryComboBoxItemViewModel(category, true);

            Dictionary<SpecRelationType, SpecRelationTypeMap> specRelationTypeMaps = null;

            var datatypeDefinitionMaps = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap> { { this.stringDatadef, new DatatypeDefinitionMap(this.stringDatadef, this.pt, null) } };
            var newDialog = new SpecRelationTypeMappingDialogViewModel(new List<SpecRelationType> { this.spectype }, specRelationTypeMaps, datatypeDefinitionMaps, this.iteration, this.session.Object, this.thingDialogNavigationService.Object, "en");
            Assert.IsEmpty(newDialog.SpecTypes.SelectMany(x => x.SelectedCategories));
            Assert.IsNull(newDialog.SpecTypes[0].SelectedRules);
            Assert.IsNull(newDialog.SpecTypes[0].SelectedBinaryRelationshipRules);

            var row = newDialog.SpecTypes.First();
            row.PossibleBinaryRelationshipRules.Add(binaryRelationshipRule);
            row.PossibleCategories.Add(categoryVm);
            row.PossibleRules.Add(rule);

            specRelationTypeMaps = new Dictionary<SpecRelationType, SpecRelationTypeMap>()
            {
                {
                    this.spectype,
                    new SpecRelationTypeMap(
                        this.spectype,
                        new[] { rule },
                        new[] { category },
                        new List<AttributeDefinitionMap>() { new AttributeDefinitionMap(this.attribute, AttributeDefinitionMapKind.SHORTNAME) },
                        new[] { binaryRelationshipRule })
                }
            };

            newDialog.PopulateRelationTypeMapProperties(specRelationTypeMaps);

            Assert.IsNotEmpty(newDialog.SpecTypes.SelectMany(x => x.SelectedCategories));
            Assert.IsNotEmpty(newDialog.SpecTypes.SelectMany(x => x.SelectedRules));
            Assert.IsNotEmpty(newDialog.SpecTypes.SelectMany(x => x.SelectedBinaryRelationshipRules));
        }
    }
}
