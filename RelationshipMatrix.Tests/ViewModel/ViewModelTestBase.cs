// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelTestBase.cs" company="Starion Group S.A.">
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

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;
    using CDP4Composition.ViewModels;
    using CDP4Composition.ViewModels.DialogResult;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CommonServiceLocator;

    using Moq;

    using ReactiveUI;

    public abstract class ViewModelTestBase
    {
        private Mock<IServiceLocator> serviceLocator;
        protected Mock<IPanelNavigationService> panelNavigationService;
        protected Mock<IThingDialogNavigationService> thingDialogNavigationService;
        protected Mock<IDialogNavigationService> dialogNavigationService;
        protected Mock<IPermissionService> permissionService;
        protected Mock<IPluginSettingsService> pluginService;
        private Mock<IFilterStringService> filterStringService;
        protected Mock<ISession> session;

        protected RelationshipMatrixPluginSettings settings;

        protected Assembler assembler;

        protected SiteDirectory sitedir;
        protected Person person;

        protected EngineeringModelSetup engineeringModelSetup;
        protected IterationSetup iterationSetup;
        protected ModelReferenceDataLibrary mrdl;
        protected SiteReferenceDataLibrary srdl;

        protected Participant participant;
        protected Uri uri = new Uri("http://starion.test.com");
        protected DomainOfExpertise domain;
        protected Category catEd1;
        protected Category catEd2;
        protected Category catEd3;
        protected Category catEd4;
        protected Category catRel;
        protected BinaryRelationshipRule rule;

        protected EngineeringModel model;
        protected Iteration iteration;
        protected ElementDefinition elementDef11;
        protected ElementDefinition elementDef12;
        protected ElementDefinition elementDef21;
        protected ElementDefinition elementDef22;
        protected ElementDefinition elementDef31;
        protected PropertyInfo rev = typeof(Thing).GetProperty("RevisionNumber");
        protected CDPMessageBus messageBus;

        public virtual void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.filterStringService = new Mock<IFilterStringService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ManageConfigurationsDialogViewModel<PluginSettings>>()))
                .Returns(new ManageConfigurationsResult(true));

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SavedConfigurationDialogViewModel<PluginSettings>>()))
                .Returns(new SavedConfigurationResult(true));

            this.permissionService = new Mock<IPermissionService>();
            this.pluginService = new Mock<IPluginSettingsService>();
            this.session = new Mock<ISession>();
            this.serviceLocator.Setup(x => x.GetInstance<IFilterStringService>()).Returns(this.filterStringService.Object);

            this.assembler = new Assembler(this.uri, this.messageBus);
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };
            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "domain", ShortName = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };

            this.catEd1 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat1", ShortName = "cat1" };
            this.catEd2 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat2", ShortName = "cat2" };
            this.catEd3 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat3sub1", ShortName = "cat3sub1", SuperCategory = new List<Category> { this.catEd1 } };
            this.catEd4 = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "cat4sub3", ShortName = "cat3sub4", SuperCategory = new List<Category> { this.catEd3 } };
            this.catRel = new Category(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "catrel", ShortName = "catrel" };

            this.catEd1.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.catEd2.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.catEd3.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.catEd4.PermissibleClass.Add(ClassKind.ElementDefinition);
            this.catRel.PermissibleClass.Add(ClassKind.BinaryRelationship);

            this.rule = new BinaryRelationshipRule(Guid.NewGuid(), this.assembler.Cache, this.uri) { SourceCategory = this.catEd1, TargetCategory = this.catEd2, RelationshipCategory = this.catRel, Name = "rel", ShortName = "rel" };

            this.sitedir.Model.Add(this.engineeringModelSetup);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);
            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl);
            this.engineeringModelSetup.Participant.Add(this.participant);
            this.engineeringModelSetup.ActiveDomain.Add(this.domain);

            this.srdl.DefinedCategory.Add(this.catEd1);
            this.srdl.DefinedCategory.Add(this.catEd2);
            this.srdl.DefinedCategory.Add(this.catEd3);
            this.srdl.DefinedCategory.Add(this.catEd4);
            this.srdl.DefinedCategory.Add(this.catRel);
            this.srdl.Rule.Add(this.rule);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.engineeringModelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };
            this.elementDef11 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "ed11", ShortName = "ed11", Owner = this.domain };
            this.elementDef12 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "ed12", ShortName = "ed12", Owner = this.domain };
            this.elementDef21 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "ed21", ShortName = "ed21", Owner = this.domain };
            this.elementDef22 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "ed22", ShortName = "ed22", Owner = this.domain };
            this.elementDef31 = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "ed31", ShortName = "ed31", Owner = this.domain };

            this.elementDef11.Category.Add(this.catEd1);
            this.elementDef12.Category.Add(this.catEd3);
            this.elementDef21.Category.Add(this.catEd2);
            this.elementDef22.Category.Add(this.catEd4);

            this.elementDef31.Category.Add(this.catEd1);
            this.elementDef31.Category.Add(this.catEd2);

            this.model.Iteration.Add(this.iteration);
            this.iteration.Element.Add(this.elementDef11);
            this.iteration.Element.Add(this.elementDef12);
            this.iteration.Element.Add(this.elementDef21);
            this.iteration.Element.Add(this.elementDef22);
            this.iteration.Element.Add(this.elementDef31);

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
            this.assembler.Cache.TryAdd(new CacheKey(this.catEd3.Iid, null), new Lazy<Thing>(() => this.catEd3));
            this.assembler.Cache.TryAdd(new CacheKey(this.catEd4.Iid, null), new Lazy<Thing>(() => this.catEd4));
            this.assembler.Cache.TryAdd(new CacheKey(this.catRel.Iid, null), new Lazy<Thing>(() => this.catRel));
            this.assembler.Cache.TryAdd(new CacheKey(this.rule.Iid, null), new Lazy<Thing>(() => this.rule));

            this.assembler.Cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));
            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.assembler.Cache.TryAdd(new CacheKey(this.elementDef11.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.elementDef11));
            this.assembler.Cache.TryAdd(new CacheKey(this.elementDef12.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.elementDef12));
            this.assembler.Cache.TryAdd(new CacheKey(this.elementDef21.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.elementDef21));
            this.assembler.Cache.TryAdd(new CacheKey(this.elementDef22.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.elementDef22));
            this.assembler.Cache.TryAdd(new CacheKey(this.elementDef31.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.elementDef31));

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.session.Setup(x => x.OpenIterations).Returns(
                new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });

            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.settings = new RelationshipMatrixPluginSettings();
            this.settings.PossibleClassKinds.Add(ClassKind.ElementDefinition);
            this.settings.PossibleClassKinds.Add(ClassKind.ElementUsage);
            this.pluginService.Setup(x => x.Read<RelationshipMatrixPluginSettings>(false)).Returns(this.settings);

            this.pluginService.Setup(x => x.Write(It.IsAny<RelationshipMatrixPluginSettings>()));
        }
    }
}
