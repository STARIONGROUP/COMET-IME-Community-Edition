// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
        private Uri uri = new Uri("http://www.rheagroup.com");
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

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();            
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            
            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "system", ShortName = "SYS" };

            var participant = new Participant(Guid.NewGuid(), this.cache, this.uri);
            participant.Domain.Add(domainOfExpertise);

            var engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            engineeringModelSetup.ActiveDomain.Add(this.domainOfExpertise);
            var srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { Name = "testRDL", ShortName = "test" };
            var category = new Category(Guid.NewGuid(), this.cache, this.uri) { Name = "test Category", ShortName = "testCategory" };
            category.PermissibleClass.Add(ClassKind.ElementDefinition);
            srdl.DefinedCategory.Add(category);
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
    }
}