// ------------------------------------------------------------------------------------------------
// <copyright file="OrderHandlerServiceTestFixtureBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------


namespace CDP4Requirements.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using Utils;

    [TestFixture]
    public class OrderHandlerServiceTestFixtureBase
    {
        protected Mock<IPanelNavigationService> panelNavigationService;
        protected Mock<IThingDialogNavigationService> thingDialogNavigationService;
        protected Mock<IDialogNavigationService> dialogNavigationService;
        protected Mock<IPermissionService> permissionService;
        protected Mock<IPluginSettingsService> pluginService;
        protected Mock<ISession> session;

        protected Assembler assembler;

        protected SiteDirectory sitedir;
        protected Person person;

        protected EngineeringModelSetup engineeringModelSetup;
        protected IterationSetup iterationSetup;
        protected ModelReferenceDataLibrary mrdl;
        protected SiteReferenceDataLibrary srdl;

        protected Participant participant;
        protected Uri uri = new Uri("http://rhea.test.com");
        protected DomainOfExpertise domain;
        protected Category catEd1;
        protected Category catEd2;
        protected Category catRel;
        protected BinaryRelationshipRule rule;
        protected ParameterType orderType;

        protected EngineeringModel model;
        protected Iteration iteration;
        protected RequirementsSpecification spec1;
        protected RequirementsSpecification spec2;
        protected RequirementsSpecification spec3;
        protected RequirementsGroup grp1;
        protected RequirementsGroup grp2;
        protected RequirementsGroup grp3;
        protected RequirementsGroup grp4;
        protected Requirement req1;
        protected Requirement req2;
        protected Requirement req3;
        protected Requirement req4;
        protected Requirement req21;
        protected Requirement req22;
        protected Requirement req23;

        protected SimpleParameterValue value1;
        protected SimpleParameterValue value2;
        protected SimpleParameterValue value3;
        protected SimpleParameterValue value4;

        protected PropertyInfo rev = typeof(Thing).GetProperty("RevisionNumber");

        public virtual void Setup()
        {
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.pluginService = new Mock<IPluginSettingsService>();
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain", ShortName = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };

            this.orderType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "orderPt", ShortName = "orderPt" };

            this.catEd1 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "cat1" };
            this.catEd2 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "cat2" };
            this.catRel = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "catrel", ShortName = "catrel" };

            this.catEd1.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.catEd2.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.catRel.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.rule = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { SourceCategory = this.catEd1, TargetCategory = this.catEd2, RelationshipCategory = this.catRel, Name = "rel", ShortName = "rel" };

            this.sitedir.Model.Add(this.engineeringModelSetup);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);
            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl);
            this.engineeringModelSetup.Participant.Add(this.participant);

            this.srdl.DefinedCategory.Add(this.catEd1);
            this.srdl.DefinedCategory.Add(this.catEd2);
            this.srdl.DefinedCategory.Add(this.catRel);
            this.srdl.Rule.Add(this.rule);
            this.srdl.ParameterType.Add(this.orderType);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.engineeringModelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };
            this.spec1 = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "spec1", ShortName = "spec1", Owner = this.domain };
            this.spec2 = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "spec2", ShortName = "spec2" };
            this.spec3 = new RequirementsSpecification(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "spec3", ShortName = "spec3" };

            this.grp1 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri) {Name = "gr1", ShortName = "gr1", Owner = this.domain};
            this.grp2 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri) {Name = "gr2", ShortName = "gr2"};
            this.grp3 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri) {Name = "gr3", ShortName = "gr3"};
            this.grp4 = new RequirementsGroup(Guid.NewGuid(), this.assembler.Cache, this.uri) {Name = "gr4", ShortName = "gr4"};

            this.req1 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "req1", ShortName = "req1" };
            this.req2 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "req2", ShortName = "req2" };
            this.req3 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "req3", ShortName = "req3" };
            this.req4 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "req4", ShortName = "req4" };
            this.req21 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "req21", ShortName = "req21", Group = this.grp1 };
            this.req22 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "req22", ShortName = "req22", Group = this.grp1 };
            this.req23 = new Requirement(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "req23", ShortName = "req23", Group = this.grp1 };

            this.value1 = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = this.orderType, Value = new ValueArray<string>(new [] { "10000" })};
            this.value2 = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = this.orderType, Value = new ValueArray<string>(new [] { "20000" })};
            this.value3 = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = this.orderType, Value = new ValueArray<string>(new [] { "30000" })};
            this.value4 = new SimpleParameterValue(Guid.NewGuid(), this.assembler.Cache, this.uri) { ParameterType = this.orderType, Value = new ValueArray<string>(new [] { "40000" })};

            this.model.Iteration.Add(this.iteration);
            this.iteration.RequirementsSpecification.Add(this.spec1);
            this.iteration.RequirementsSpecification.Add(this.spec2);
            this.iteration.RequirementsSpecification.Add(this.spec3);
            this.spec1.Requirement.Add(this.req1);
            this.spec1.Requirement.Add(this.req2);
            this.spec1.Requirement.Add(this.req3);
            this.spec1.Requirement.Add(this.req4);

            this.spec2.Requirement.Add(this.req21);
            this.spec2.Requirement.Add(this.req22);
            this.spec2.Requirement.Add(this.req23);

            this.spec2.Group.Add(this.grp1);
            this.spec2.Group.Add(this.grp2);
            this.spec2.Group.Add(this.grp3);
            this.grp1.Group.Add(this.grp4);

            this.req1.ParameterValue.Add(this.value1);
            this.req2.ParameterValue.Add(this.value2);
            this.req3.ParameterValue.Add(this.value3);
            this.req4.ParameterValue.Add(this.value4);

            this.assembler.Cache.TryAdd(new CacheKey(this.sitedir.Iid, null), new Lazy<Thing>(() => this.sitedir));
            this.assembler.Cache.TryAdd(new CacheKey(this.engineeringModelSetup.Iid, null), new Lazy<Thing>(() => this.engineeringModelSetup));
            this.assembler.Cache.TryAdd(new CacheKey(this.srdl.Iid, null), new Lazy<Thing>(() => this.srdl));
            this.assembler.Cache.TryAdd(new CacheKey(this.iterationSetup.Iid, null), new Lazy<Thing>(() => this.iterationSetup));
            this.assembler.Cache.TryAdd(new CacheKey(this.mrdl.Iid, null), new Lazy<Thing>(() => this.mrdl));
            this.assembler.Cache.TryAdd(new CacheKey(this.person.Iid, null), new Lazy<Thing>(() => this.person));
            this.assembler.Cache.TryAdd(new CacheKey(this.domain.Iid, null), new Lazy<Thing>(() => this.domain));
            this.assembler.Cache.TryAdd(new CacheKey(this.participant.Iid, null), new Lazy<Thing>(() => this.participant));
            this.assembler.Cache.TryAdd(new CacheKey(this.catEd1.Iid, null), new Lazy<Thing>(() => this.catEd1));
            this.assembler.Cache.TryAdd(new CacheKey(this.catEd2.Iid, null), new Lazy<Thing>(() => this.catEd2));
            this.assembler.Cache.TryAdd(new CacheKey(this.catRel.Iid, null), new Lazy<Thing>(() => this.catRel));
            this.assembler.Cache.TryAdd(new CacheKey(this.rule.Iid, null), new Lazy<Thing>(() => this.rule));

            this.assembler.Cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));
            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.assembler.Cache.TryAdd(new CacheKey(this.spec1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.spec1));
            this.assembler.Cache.TryAdd(new CacheKey(this.spec2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.spec2));
            this.assembler.Cache.TryAdd(new CacheKey(this.spec3.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.spec3));
            this.assembler.Cache.TryAdd(new CacheKey(this.grp1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.grp1));
            this.assembler.Cache.TryAdd(new CacheKey(this.grp2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.grp2));
            this.assembler.Cache.TryAdd(new CacheKey(this.grp3.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.grp3));
            this.assembler.Cache.TryAdd(new CacheKey(this.grp4.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.grp4));
            this.assembler.Cache.TryAdd(new CacheKey(this.req1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.req1));
            this.assembler.Cache.TryAdd(new CacheKey(this.req2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.req2));
            this.assembler.Cache.TryAdd(new CacheKey(this.req3.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.req3));
            this.assembler.Cache.TryAdd(new CacheKey(this.req4.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.req4));
            this.assembler.Cache.TryAdd(new CacheKey(this.req21.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.req21));
            this.assembler.Cache.TryAdd(new CacheKey(this.req22.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.req22));
            this.assembler.Cache.TryAdd(new CacheKey(this.req23.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.req23));
            this.assembler.Cache.TryAdd(new CacheKey(this.value1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.value1));
            this.assembler.Cache.TryAdd(new CacheKey(this.value2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.value2));
            this.assembler.Cache.TryAdd(new CacheKey(this.value3.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.value3));
            this.assembler.Cache.TryAdd(new CacheKey(this.value4.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.value4));

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(
                new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });

            RequirementsModule.PluginSettings = new RequirementsModuleSettings();
            RequirementsModule.PluginSettings.OrderSettings = new OrderSettings();
            RequirementsModule.PluginSettings.OrderSettings.ParameterType = this.orderType.Iid;
        }
    }
}
