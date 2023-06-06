// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;

    using CDP4EngineeringModel.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class ParameterGroupDialogViewModelTestFixture
    {
        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.session = new Mock<ISession>();
            this.testEM = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.testIteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.testEM.Iteration.Add(this.testIteration);
            this.testED = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            this.parameterGroup = new ParameterGroup { Name = "1" };
            this.testIteration.Element.Add(this.testED);

            this.cache.TryAdd(new CacheKey(this.testED.Iid, this.testIteration.Iid), new Lazy<Thing>(() => this.testED));
            var clone = this.testED.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.testIteration);
            this.transaction = new ThingTransaction(transactionContext, clone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            this.viewmodel = new ParameterGroupDialogViewModel(this.parameterGroup, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone);
        }

        private ParameterGroupDialogViewModel viewmodel;
        private ParameterGroup parameterGroup;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;

        private ElementDefinition testED;
        private EngineeringModel testEM;
        private Iteration testIteration;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private readonly Uri uri = new Uri("http://test.com");

        [Test]
        public void VerifyThatCyclicParameterGroupsWork()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "ElemDef", ShortName = "ED" };
            this.testIteration.Element.Add(elementDefinition);
            var pg1 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "1" };
            elementDefinition.ParameterGroup.Add(pg1);
            var pg2 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "2", ContainingGroup = pg1 };
            elementDefinition.ParameterGroup.Add(pg2);
            var pg3 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "3", ContainingGroup = pg2 };
            elementDefinition.ParameterGroup.Add(pg3);
            var pg4 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "4", ContainingGroup = pg3 };
            elementDefinition.ParameterGroup.Add(pg4);
            pg1.ContainingGroup = pg4;
            var pg5 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "5" };
            elementDefinition.ParameterGroup.Add(pg5);
            var pg6 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "6", ContainingGroup = pg5 };
            elementDefinition.ParameterGroup.Add(pg6);
            this.cache.TryAdd(new CacheKey(elementDefinition.Iid, this.testIteration.Iid), new Lazy<Thing>(() => elementDefinition));

            var clone = elementDefinition.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.testIteration);
            this.transaction = new ThingTransaction(transactionContext, clone);

            var vm1 = new ParameterGroupDialogViewModel(pg1, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone);
            Assert.AreEqual(2, vm1.PossibleGroups.Count);
            Assert.IsTrue(vm1.PossibleGroups.Any(g => g.DisplayedName == pg5.Name));
            Assert.IsTrue(vm1.PossibleGroups.Any(g => g.DisplayedName == pg6.Name));
            Assert.IsFalse(vm1.PossibleGroups.Any(g => g.DisplayedName == pg1.Name));

            var vm2 = new ParameterGroupDialogViewModel(pg2, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone);
            Assert.AreEqual(2, vm2.PossibleGroups.Count);
            Assert.IsTrue(vm2.PossibleGroups.Any(g => g.DisplayedName == pg5.Name));
            Assert.IsTrue(vm2.PossibleGroups.Any(g => g.DisplayedName == pg6.Name));
            Assert.IsFalse(vm2.PossibleGroups.Any(g => g.DisplayedName == pg1.Name));

            var vm3 = new ParameterGroupDialogViewModel(pg3, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone);
            Assert.AreEqual(2, vm3.PossibleGroups.Count);
            Assert.IsTrue(vm3.PossibleGroups.Any(g => g.DisplayedName == pg5.Name));
            Assert.IsTrue(vm3.PossibleGroups.Any(g => g.DisplayedName == pg6.Name));
            Assert.IsFalse(vm3.PossibleGroups.Any(g => g.DisplayedName == pg1.Name));

            var vm4 = new ParameterGroupDialogViewModel(pg4, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone);
            Assert.AreEqual(2, vm4.PossibleGroups.Count);
            Assert.IsTrue(vm4.PossibleGroups.Any(g => g.DisplayedName == pg5.Name));
            Assert.IsTrue(vm4.PossibleGroups.Any(g => g.DisplayedName == pg6.Name));
            Assert.IsFalse(vm4.PossibleGroups.Any(g => g.DisplayedName == pg1.Name));
        }

        [Test]
        public async Task VerifyThatExceptionAreCaught()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));

            this.viewmodel.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));

            Assert.IsNotNull(this.viewmodel.WriteException);
            Assert.AreEqual("test", this.viewmodel.WriteException.Message);
        }

        [Test]
        public async Task VerifyThatOkCommandWorks()
        {
            this.viewmodel.OkCommand.Execute(null);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
            Assert.IsNull(this.viewmodel.WriteException);
            Assert.IsTrue(this.viewmodel.DialogResult.Value);
        }

        [Test]
        public void VerifyThatParameterGroupsAreAcyclic()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Name = "ElemDef", ShortName = "ED" };
            this.testIteration.Element.Add(elementDefinition);
            var pg1 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "1" };
            elementDefinition.ParameterGroup.Add(pg1);
            var pg2 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "2", ContainingGroup = pg1 };
            elementDefinition.ParameterGroup.Add(pg2);
            var pg3 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "3", ContainingGroup = pg2 };
            elementDefinition.ParameterGroup.Add(pg3);
            var pg4 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "4", ContainingGroup = pg3 };
            elementDefinition.ParameterGroup.Add(pg4);
            var pg5 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "5" };
            elementDefinition.ParameterGroup.Add(pg5);
            var pg6 = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "6", ContainingGroup = pg5 };
            elementDefinition.ParameterGroup.Add(pg6);
            this.cache.TryAdd(new CacheKey(elementDefinition.Iid, this.testIteration.Iid), new Lazy<Thing>(() => elementDefinition));

            var clone = elementDefinition.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.testIteration);
            this.transaction = new ThingTransaction(transactionContext, clone);

            var vm1 = new ParameterGroupDialogViewModel(pg1, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone);
            Assert.AreEqual(2, vm1.PossibleGroups.Count);

            var vm2 = new ParameterGroupDialogViewModel(pg2, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone);
            Assert.AreEqual(3, vm2.PossibleGroups.Count);

            var vm3 = new ParameterGroupDialogViewModel(pg3, this.transaction, this.session.Object, true, ThingDialogKind.Create, null, clone);
            Assert.AreEqual(4, vm3.PossibleGroups.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new ParameterGroupDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsReadOnly);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            Assert.AreEqual(this.viewmodel.Name, this.parameterGroup.Name);
            Assert.IsNull(this.viewmodel.SelectedContainingGroup);
        }
    }
}
