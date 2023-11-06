// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ProductTree.Tests.ProductTreeRows
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;
    using CDP4Composition.Services.NestedElementTreeService;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;

    using CDP4ProductTree.Tests.ProductTreeRows;
    using CDP4ProductTree.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ElementDefinitionRowViewModelTestFixture
    {
        private Mock<INestedElementTreeService> nestedElementTreeService;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingCreator> thingCreator;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Mock<IServiceLocator> serviceLocator;

        private SiteDirectory siteDir;
        private EngineeringModel model;
        private Iteration iteration;
        private EngineeringModelSetup modelSetup;
        private IterationSetup iterationSetup;
        private Person person;
        private Participant participant;
        private Option option;
        private ElementDefinition elementDef;
        private ElementDefinition elementDef2;
        private Category category;
        private DomainOfExpertise domain;
        private ElementUsage elementUsage;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
        private readonly string nestedElementPath = "PATH";

        [SetUp]
        public void Setup()
        {
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" , ShortName = "dom"};

            this.serviceLocator = new Mock<IServiceLocator>();
            this.thingCreator = new Mock<IThingCreator>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingCreator>())
                .Returns(this.thingCreator.Object);

            this.nestedElementTreeService = new Mock<INestedElementTreeService>();
            this.nestedElementTreeService.Setup(x => x.GetNestedElementPath(It.IsAny<ElementDefinition>(), It.IsAny<Option>())).Returns(this.nestedElementPath);

            this.serviceLocator.Setup(x => x.GetInstance<INestedElementTreeService>()).Returns(this.nestedElementTreeService.Object);

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri);
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.category = new Category(Guid.NewGuid(), this.cache, this.uri);
            this.elementDef = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) {Owner = this.domain};
            this.elementDef.Category.Add(this.category);

            this.elementDef2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { ElementDefinition = this.elementDef2, Owner = this.domain};

            this.siteDir.Person.Add(this.person);
            this.siteDir.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.participant.Person = this.person;

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration.TopElement = this.elementDef;
            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(this.elementDef2);
            this.elementDef.ContainedElement.Add(this.elementUsage);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            Assert.AreEqual("domain", vm.OwnerName);
            Assert.AreEqual("dom", vm.OwnerShortName );

            Assert.AreEqual(1, vm.ContainedRows.Count);

            var expected = new List<Category>
            {
                this.category
            };

            CollectionAssert.AreEquivalent(expected, vm.Category);
        }

        [Test]
        public void VerifyThatGetPathWorks()
        {
            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            Assert.AreEqual(this.nestedElementPath, vm.GetPath());
        }

        [Test, TestCaseSource(typeof(MessageBusContainerCases), "GetCases")]
        public void VerifyThatUsagesAreAddedOrRemoved(IViewModelBase<Thing> container, string scenario)
        {
            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, container);

            var usage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            usage.ElementDefinition = this.elementDef2;

            var revisionProperty = typeof (ElementDefinition).GetProperty("RevisionNumber");
            revisionProperty.SetValue(this.elementDef, 50);
            this.elementDef.ContainedElement.Add(usage);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.Count);
            Assert.IsTrue(vm.ContainedRows.Any(x => x.Thing == usage));

            this.elementDef.ContainedElement.Remove(usage);
            revisionProperty.SetValue(this.elementDef, 100);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);
            Assert.AreEqual(1, vm.ContainedRows.Count);
            Assert.IsFalse(vm.ContainedRows.Any(x => x.Thing == usage));
        }



        [Test, TestCaseSource(typeof(MessageBusContainerCases), "GetCases")]
        public void VerifyThatGroupAreAddedCorrectly(IViewModelBase<Thing> container, string scenario)
        {
            var revisionProperty = typeof (ElementDefinition).GetProperty("RevisionNumber");

            var group1 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri);
            var group11 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { ContainingGroup = group1 };

            this.elementDef.ParameterGroup.Add(group1);
            this.elementDef.ParameterGroup.Add(group11);

            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, container);

            var group1row = vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Single();
            Assert.AreSame(group1, group1row.Thing);

            var group11row = group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Single();
            Assert.AreSame(group11, group11row.Thing);

            // move group11
            group11.ContainingGroup = null;
            revisionProperty.SetValue(group11, 10);

            CDPMessageBus.Current.SendObjectChangeEvent(group11, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreEqual(0, group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());

            // move group11 under group1
            group11.ContainingGroup = group1;
            revisionProperty.SetValue(group11, 20);

            CDPMessageBus.Current.SendObjectChangeEvent(group11, EventKind.Updated);
            Assert.AreEqual(1, vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreSame(group11, group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Single().Thing);

            // add group2 and move group11 under group2
            var group2 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri);
            group11.ContainingGroup = group2;
            this.elementDef.ParameterGroup.Add(group2);
            revisionProperty.SetValue(group11, 30);
            revisionProperty.SetValue(this.elementDef, 30);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);
            CDPMessageBus.Current.SendObjectChangeEvent(group11, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreEqual(0, group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());

            var group2row = vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Single(x => x.Thing == group2);
            Assert.AreEqual(1, group2row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());

            // remove group11
            this.elementDef.ParameterGroup.Remove(group11);
            revisionProperty.SetValue(this.elementDef, 40);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementDef, EventKind.Updated);
            Assert.AreEqual(2, vm.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreEqual(0, group1row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
            Assert.AreEqual(0, group2row.ContainedRows.OfType<ParameterGroupRowViewModel>().Count());
        }

        [Test]
        public void VerifyThatParametersAreCorrectlyHandled()
        {
            var revisionProperty = typeof(ElementDefinition).GetProperty("RevisionNumber");

            var group1 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri);
            var group11 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { ContainingGroup = group1 };

            this.elementDef.ParameterGroup.Add(group1);
            this.elementDef.ParameterGroup.Add(group11);

            var type1 = new EnumerationParameterType(Guid.NewGuid(), this.cache, this.uri) {Name = "type1"};
            var parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri) {ParameterType = type1, Owner = this.domain};
            var valueset = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            valueset.Published = new ValueArray<string>(new List<string>{"1"});
            valueset.Manual = new ValueArray<string>(new List<string> { "1" });
            valueset.ValueSwitch = ParameterSwitchKind.MANUAL;
            parameter1.ValueSet.Add(valueset);

            this.elementDef.Parameter.Add(parameter1);

            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            var param1row = vm.ContainedRows.OfType<ParameterRowViewModel>().Single();
            Assert.AreSame(parameter1, param1row.Thing);
        }

        [Test, TestCaseSource(typeof(MessageBusContainerCases), "GetCases")]
        public void VerifyThatOptionDependencyIsHandled(IViewModelBase<Thing> container, string scenario)
        {
            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, container);
            Assert.IsTrue(vm.ContainedRows.Select(x => x.Thing).Contains(this.elementUsage));

            var revisionProperty = typeof(ElementDefinition).GetProperty("RevisionNumber");
            revisionProperty.SetValue(this.elementUsage, 20);

            this.elementUsage.ExcludeOption.Add(this.option);

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementUsage, EventKind.Updated);
            Assert.IsFalse(vm.ContainedRows.Select(x => x.Thing).Contains(this.elementUsage));

            revisionProperty.SetValue(this.elementUsage, 30);
            this.elementUsage.ExcludeOption.Clear();

            CDPMessageBus.Current.SendObjectChangeEvent(this.elementUsage, EventKind.Updated);
            Assert.IsTrue(vm.ContainedRows.Select(x => x.Thing).Contains(this.elementUsage));
        }

        [Test]
        public void VerifyThatDragOverWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDef2);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Copy, dropinfo.Object.Effects);
        }

        [Test]
        public void VerifyThatDragOverWorks2()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var vm = new ElementUsageRowViewModel(this.elementUsage, this.option, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDef);

            dropinfo.SetupProperty(x => x.Effects);
            vm.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);
        }

        [Test]
        public async Task VerifyThatDropWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            var vm = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDef2);
            dropinfo.Setup(x => x.Effects).Returns(DragDropEffects.Copy);

            dropinfo.SetupProperty(x => x.Effects);
            await vm.Drop(dropinfo.Object);

            this.thingCreator.Verify(x => x.CreateElementUsage(It.IsAny<ElementDefinition>(), It.IsAny<ElementDefinition>(), It.IsAny<DomainOfExpertise>(), It.IsAny<ISession>()));
        }
    }

    /// <summary>
    /// Implementation of <see cref="IViewModelBase{Thing}"/> and <see cref="IHaveMessageBusHandler"/>
    /// </summary>
    internal class TestMessageBusHandlerContainerViewModel : IViewModelBase<Thing>, IHaveMessageBusHandler
    {
        /// <summary>
        /// The <see cref="MessageBusHandler"/>
        /// </summary>
        public MessageBusHandler MessageBusHandler { get; } = new MessageBusHandler();

        /// <summary>
        /// The <see cref="Thing"/>
        /// </summary>
        public Thing Thing { get; }

        /// <summary>
        /// Disposes the instance
        /// </summary>
        public void Dispose()
        {
        }
    }
}