// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDialogViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary,
//              Rowan de Voogt
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    
    using CDP4Dal;    
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    
    using CDP4EngineeringModel.ViewModels;
    
    using Moq;
    
    using NUnit.Framework;
    
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ElementDefinitionDialogViewModel"/>
    /// </summary>
    [TestFixture]
    public class ElementDefinitionDialogViewModelTestFixture
    {
        private Uri uri = new Uri("https://www.stariongroup.eu");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private IThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Iteration iterationClone;
        private EngineeringModel engineeringModel;
        private DomainOfExpertise domainOfExpertise;

        private ElementDefinition elementDefinition;
        private CDPMessageBus messageBus;
        private Assembler assembler;
        private SimpleQuantityKind parameterType;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.assembler = new Assembler(this.uri, this.messageBus);

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            
            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "system", ShortName = "SYS" };

            var person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.session.Setup(x => x.ActivePerson).Returns(person);

            var participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = person };
            participant.Domain.Add(domainOfExpertise);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            engineeringModelSetup.Participant.Add(participant);
            engineeringModelSetup.ActiveDomain.Add(this.domainOfExpertise);
            var srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "testRDL", ShortName = "test" };
            var category = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "test Category", ShortName = "testCategory" };
            category.PermissibleClass.Add(ClassKind.ElementDefinition);
            srdl.DefinedCategory.Add(category);
            this.parameterType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "mass", ShortName = "m" };
            srdl.ParameterType.Add(this.parameterType);
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = srdl };
            engineeringModelSetup.RequiredRdl.Add(mrdl);
            srdl.DefinedCategory.Add(new Category(Guid.NewGuid(), this.cache, this.uri));
            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.EngineeringModelSetup = engineeringModelSetup;
            var iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.Iteration.Add(iteration);
            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            iteration.Element.Add(this.elementDefinition);
            
            this.cache.TryAdd(new CacheKey(iteration.Iid, null), new Lazy<Thing>(() => iteration));
            this.iterationClone = iteration.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.iterationClone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);

            var openIterations = new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>();
            openIterations.Add(iteration, new Tuple<DomainOfExpertise, Participant>(domainOfExpertise, participant));

            this.session.Setup(x => x.OpenIterations).Returns(openIterations);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatDefaultConstructorIsAvailable()
        {
            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel();
            Assert.IsNotNull(elementDefinitionDialogViewModel);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var name = "name";
            var shortname = "shortname";
            
            this.elementDefinition.Name = name;
            this.elementDefinition.ShortName = shortname;
            this.elementDefinition.Owner = this.domainOfExpertise;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.AreEqual(name, elementDefinitionDialogViewModel.Name);
            Assert.AreEqual(shortname, elementDefinitionDialogViewModel.ShortName);
            Assert.AreEqual(this.domainOfExpertise, elementDefinitionDialogViewModel.SelectedOwner);
            Assert.AreSame(this.iterationClone, elementDefinitionDialogViewModel.Container);
            Assert.IsFalse(elementDefinitionDialogViewModel.IsTopElement);
            Assert.IsTrue(elementDefinitionDialogViewModel.PossibleCategory.Any());
        }

        [Test]
        public void VerifyThatIsTopElementReturnsTrueIfElementDefinitionIsTopElelement()
        {
            this.iterationClone.TopElement = this.elementDefinition;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.IsTrue(elementDefinitionDialogViewModel.IsTopElement);
        }

        [Test]
        public async Task VerifyOkExecuteWhenNotTopElement()
        {
            var name = "name";
            var shortname = "shortname";

            this.elementDefinition.Name = name;
            this.elementDefinition.ShortName = shortname;
            this.elementDefinition.Owner = this.domainOfExpertise;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);
            await elementDefinitionDialogViewModel.OkCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public async Task VerifyOkExecuteWhenTopElement()
        {
            var name = "name";
            var shortname = "shortname";

            this.elementDefinition.Name = name;
            this.elementDefinition.ShortName = shortname;
            this.elementDefinition.Owner = this.domainOfExpertise;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);
            elementDefinitionDialogViewModel.IsTopElement = true;
            await elementDefinitionDialogViewModel.OkCommand.Execute();

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatParameterRowsShowsGroupsAndParametersButNotUsages()
        {
            this.elementDefinition.Owner = this.domainOfExpertise;

            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.parameterType,
                Owner = this.domainOfExpertise
            };

            this.elementDefinition.Parameter.Add(parameter);

            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "group" };
            this.elementDefinition.ParameterGroup.Add(parameterGroup);

            var referencedElementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "ref", ShortName = "ref", Owner = this.domainOfExpertise };
            this.engineeringModel.Iteration.First().Element.Add(referencedElementDefinition);

            var elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "usage",
                ShortName = "usage",
                Owner = this.domainOfExpertise,
                ElementDefinition = referencedElementDefinition
            };

            this.elementDefinition.ContainedElement.Add(elementUsage);

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);

            var rowThings = elementDefinitionDialogViewModel.ParameterRows.Select(x => x.Thing).ToList();

            Assert.That(rowThings, Does.Contain(parameter));
            Assert.That(rowThings, Does.Contain(parameterGroup));
            Assert.That(rowThings, Does.Not.Contain(elementUsage));
        }

        [Test]
        public void VerifyThatAParameterIsNestedUnderItsGroup()
        {
            this.elementDefinition.Owner = this.domainOfExpertise;

            var parameterGroup = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "group" };
            this.elementDefinition.ParameterGroup.Add(parameterGroup);

            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.parameterType,
                Owner = this.domainOfExpertise,
                Group = parameterGroup
            };

            this.elementDefinition.Parameter.Add(parameter);

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);

            var groupRow = elementDefinitionDialogViewModel.ParameterRows.Single(x => Equals(x.Thing, parameterGroup));

            Assert.That(elementDefinitionDialogViewModel.ParameterRows.Select(x => x.Thing), Does.Not.Contain(parameter));
            Assert.That(groupRow.ContainedRows.Select(x => x.Thing), Does.Contain(parameter));
        }

        [Test]
        public async Task VerifyThatCreateParameterStagesAParameterAndShowsItInTheTree()
        {
            this.elementDefinition.Owner = this.domainOfExpertise;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.That(elementDefinitionDialogViewModel.SelectedParameterTypeToAdd, Is.EqualTo(this.parameterType));

            await elementDefinitionDialogViewModel.CreateParameterTreeCommand.Execute();

            Assert.That(this.elementDefinition.Parameter, Has.Count.EqualTo(1));
            Assert.That(this.elementDefinition.Parameter.Single().ParameterType, Is.EqualTo(this.parameterType));
            Assert.That(elementDefinitionDialogViewModel.ParameterRows.Select(x => x.Thing), Does.Contain(this.elementDefinition.Parameter.Single()));
        }

        [Test]
        public void VerifyThatUsedParameterTypesAreExcludedFromTheAddList()
        {
            this.elementDefinition.Owner = this.domainOfExpertise;

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.That(elementDefinitionDialogViewModel.PossibleParameterTypesToAdd, Does.Contain(this.parameterType));

            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.parameterType,
                Owner = this.domainOfExpertise
            };

            this.elementDefinition.Parameter.Add(parameter);

            var dialogWithUsedType = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);

            Assert.That(dialogWithUsedType.PossibleParameterTypesToAdd, Does.Not.Contain(this.parameterType));
        }

        [Test]
        public async Task VerifyThatDeleteParameterRemovesItFromTheTree()
        {
            this.elementDefinition.Owner = this.domainOfExpertise;

            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.parameterType,
                Owner = this.domainOfExpertise
            };

            this.elementDefinition.Parameter.Add(parameter);

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);

            elementDefinitionDialogViewModel.SelectedParameterTreeRow = elementDefinitionDialogViewModel.ParameterRows.Single(x => Equals(x.Thing, parameter));

            await elementDefinitionDialogViewModel.DeleteParameterTreeCommand.Execute();

            Assert.That(this.elementDefinition.Parameter, Does.Not.Contain(parameter));
            Assert.That(elementDefinitionDialogViewModel.ParameterRows.Select(x => x.Thing), Does.Not.Contain(parameter));
        }

        [Test]
        public async Task VerifyThatEditParameterTreeCommandEditsWithinTheDialogTransaction()
        {
            this.elementDefinition.Owner = this.domainOfExpertise;

            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                ParameterType = this.parameterType,
                Owner = this.domainOfExpertise
            };

            this.elementDefinition.Parameter.Add(parameter);

            IThingTransaction usedTransaction = null;

            this.thingDialogNavigationService
                .Setup(x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), It.IsAny<ISession>(), false, ThingDialogKind.Update, It.IsAny<IThingDialogNavigationService>(), It.IsAny<Thing>(), It.IsAny<IEnumerable<Thing>>()))
                .Callback<Thing, IThingTransaction, ISession, bool, ThingDialogKind, IThingDialogNavigationService, Thing, IEnumerable<Thing>>((t, tr, s, r, k, n, c, ch) => usedTransaction = tr)
                .Returns(true);

            var elementDefinitionDialogViewModel = new ElementDefinitionDialogViewModel(this.elementDefinition, this.thingTransaction, this.session.Object, true, ThingDialogKind.Update, this.thingDialogNavigationService.Object, this.iterationClone);

            elementDefinitionDialogViewModel.SelectedParameterTreeRow = elementDefinitionDialogViewModel.ParameterRows.Single(row => Equals(row.Thing, parameter));

            await elementDefinitionDialogViewModel.EditParameterTreeCommand.Execute();

            // the edit is routed through the child dialog as a non-root (isRoot = false) navigation, so the change
            // is part of this dialog's transaction chain rather than an independent live write.
            this.thingDialogNavigationService.Verify(
                x => x.Navigate(It.IsAny<Thing>(), It.IsAny<IThingTransaction>(), It.IsAny<ISession>(), false, ThingDialogKind.Update, It.IsAny<IThingDialogNavigationService>(), It.IsAny<Thing>(), It.IsAny<IEnumerable<Thing>>()),
                Times.Once);

            Assert.That(usedTransaction, Is.Not.Null);
        }
    }
}