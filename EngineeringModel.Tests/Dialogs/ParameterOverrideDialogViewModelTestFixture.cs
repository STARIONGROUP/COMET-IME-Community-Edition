// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideDialogViewModelTestFixture.cs" company="RHEA System S.A.">
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
    internal class ParameterOverrideDialogViewModelTestFixture
    {
        private Uri uri;
        private ThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Parameter parameter;
        private Iteration iteration;
        private EngineeringModel model;
        private SiteDirectory sitedir;
        private SiteReferenceDataLibrary srdl;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private ElementUsage elementUsageClone;
        private ParameterOverride parameterOverride;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("http://www.rheagroup.com");
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            var testDomain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            var subscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri);
            subscription.Owner = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "Other Domain" };
            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            paramValueSet.Computed = new ValueArray<string>(new[] { "c" });
            paramValueSet.Manual = new ValueArray<string>(new[] { "m" });
            paramValueSet.Reference = new ValueArray<string>(new[] { "r" });
            paramValueSet.ValueSwitch = ParameterSwitchKind.COMPUTED;

            var paramOverrideValueSet = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri);
            paramOverrideValueSet.Computed = new ValueArray<string>(new[] { "c1" });
            paramOverrideValueSet.Manual = new ValueArray<string>(new[] { "m1" });
            paramOverrideValueSet.Reference = new ValueArray<string>(new[] { "r1" });
            paramOverrideValueSet.ValueSwitch = ParameterSwitchKind.REFERENCE;
            paramOverrideValueSet.ParameterValueSet = paramValueSet;

            var testParamType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri);
            this.parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { Owner = testDomain, ParameterType = testParamType };
            this.parameter.ValueSet.Add(paramValueSet);
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            elementDefinition.Parameter.Add(this.parameter);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            var option1 = new Option(Guid.NewGuid(), this.cache, this.uri) { Name = "opt1" };
            var option2 = new Option(Guid.NewGuid(), this.cache, this.uri) { Name = "opt2" };
            this.iteration.Option.Add(option1);
            this.iteration.Option.Add(option2);
            this.iteration.Element.Add(elementDefinition);
            var actualFiniteStateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri);
            actualFiniteStateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri));
            actualFiniteStateList.ActualState.Add(new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri));
            this.iteration.ActualFiniteStateList.Add(actualFiniteStateList);
            var modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            modelSetup.ActiveDomain.Add(testDomain);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = modelSetup };
            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri);
            this.srdl.Scale.Add(new RatioScale(Guid.NewGuid(), this.cache, this.uri));
            this.srdl.ParameterType.Add(testParamType);
            var mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri) { RequiredRdl = this.srdl };
            this.model.EngineeringModelSetup.RequiredRdl.Add(mrdl);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            var testPerson = new Person(Guid.NewGuid(), this.cache, this.uri);
            var testParticipant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = testPerson, SelectedDomain = testDomain };
            modelSetup.Participant.Add(testParticipant);
            this.parameterOverride = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri) { Parameter = this.parameter };
            this.parameterOverride.ValueSet.Add(paramOverrideValueSet);
            this.parameterOverride.ParameterSubscription.Add(subscription);
            var elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri);
            elementUsage.ParameterOverride.Add(this.parameterOverride);
            elementDefinition.ContainedElement.Add(elementUsage);
            elementUsage.ElementDefinition = elementDefinition;
            this.model.Iteration.Add(this.iteration);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(testPerson);
            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.elementUsageClone = elementUsage.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.elementUsageClone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var vm = new ParameterOverrideDialogViewModel(this.parameterOverride, this.thingTransaction, this.session.Object, true,
                ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementUsageClone);

            Assert.IsNotNull(vm.SelectedParameterType);
            Assert.IsNotNull(vm.SelectedOwner);
            Assert.AreEqual(1, vm.ValueSet.Count);
            Assert.AreEqual(1, vm.PossibleOwner.Count);
            var valueSet = vm.ValueSet.First();

            Assert.AreEqual("m1", valueSet.Manual);
            Assert.AreEqual("r1", valueSet.Reference);
            Assert.AreEqual("c1", valueSet.Computed);
            Assert.AreEqual(ParameterSwitchKind.REFERENCE, valueSet.Switch);
            Assert.AreEqual("r1", valueSet.Value);
            Assert.AreEqual(1, vm.ParameterSubscription.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new ParameterOverrideDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsReadOnly);
        }

        [Test]
        public void VerifyUpdateOkCanExecute()
        {
            var vm = new ParameterOverrideDialogViewModel(this.parameterOverride, this.thingTransaction, this.session.Object, true,
                ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementUsageClone);

            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
            var owner = vm.SelectedOwner;

            vm.SelectedOwner = null;
            Assert.IsFalse(((ICommand)vm.OkCommand).CanExecute(null));
            vm.SelectedOwner = owner;
        }

        [Test]
        public async Task VerifyUpdateOkExecute()
        {
            var vm = new ParameterOverrideDialogViewModel(this.parameterOverride, this.thingTransaction, this.session.Object, true,
                ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.elementUsageClone);

            Assert.IsTrue(((ICommand)vm.OkCommand).CanExecute(null));
            await vm.OkCommand.Execute();
        }
    }
}
