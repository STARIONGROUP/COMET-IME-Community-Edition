// -------------------------------------------------------------------------------------------------
// <copyright file="SpecificationTypeMappingDialogTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.ReqIF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using ReqIFDal;
    using ReqIFSharp;
    using ViewModels;

    [TestFixture]
    internal class SpecificationTypeMappingDialogTestFixture
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
        private SpecificationType spectype;
        private AttributeDefinitionString attribute;

        private ParameterType pt;

        private SpecificationTypeMappingDialogViewModel dialog;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.assembler = new Assembler(this.uri);

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
            this.reqIf = new ReqIF();
            this.reqIf.Lang = "en";
            var corecontent = new ReqIFContent();
            this.reqIf.CoreContent.Add(corecontent);
            this.stringDatadef = new DatatypeDefinitionString();
            this.spectype = new SpecificationType();
            this.attribute = new AttributeDefinitionString() {DatatypeDefinition = this.stringDatadef};

            this.spectype.SpecAttributes.Add(this.attribute);

            corecontent.DataTypes.Add(this.stringDatadef);

            this.dialog = new SpecificationTypeMappingDialogViewModel(new List<SpecType> { this.spectype }, new Dictionary<DatatypeDefinition, DatatypeDefinitionMap> { { this.stringDatadef, new DatatypeDefinitionMap(this.stringDatadef, this.pt, null) } }, null, this.iteration, this.session.Object, this.thingDialogNavigationService.Object, "en");
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
        }

        [Test]
        public void VerifyThatCreateCategoryWorks()
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

            this.dialog.CreateCommand.Execute(null);

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
        public void VerifyThatCancelCommandWorks()
        {
            this.dialog.CancelCommand.Execute(null);
            Assert.IsFalse(this.dialog.DialogResult.Result.Value);
        }

        [Test]
        public void VerifyThatBackCommandWorks()
        {
            this.dialog.BackCommand.Execute(null);
            var res = this.dialog.DialogResult as SpecificationTypeMappingDialogResult;
            Assert.IsNotNull(res);
            Assert.IsFalse(res.GoNext.Value);
            Assert.IsTrue(res.Result.Value);

        }

        [Test]
        public void VerifyThatExecuteNextWorks()
        {
            var specificationRow = this.dialog.SpecTypes.First();
            specificationRow.AttributeDefinitions.First().AttributeDefinitionMapKind = AttributeDefinitionMapKind.NAME;

            this.dialog.NextCommand.Execute(null);
            var res = this.dialog.DialogResult as SpecificationTypeMappingDialogResult;
            Assert.IsNotNull(res);
            Assert.IsTrue(res.Result.Value);
            Assert.IsTrue(res.GoNext.Value);
            Assert.IsNotEmpty(res.SpecificationTypeMap);
        }

    }
}