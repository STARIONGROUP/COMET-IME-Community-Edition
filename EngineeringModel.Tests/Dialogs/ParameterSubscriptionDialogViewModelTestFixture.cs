// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    internal class ParameterSubscriptionDialogViewModelTestFixture
    {
        private ThingTransaction thingTransaction;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Parameter parameter;
        private Iteration iteration;
        private EngineeringModel model;
        private SiteDirectory sitedir;
        private SiteReferenceDataLibrary srdl;
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private ElementUsage elementUsageClone;
        private ParameterSubscription parameterSubscription;
        private Parameter parameterClone;
        private Uri uri;

        [SetUp]
        public void Setup()
        {
            this.uri = new Uri("http://www.rheagroup.com");
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            var testDomain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);
            var subscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri);
            subscription.Owner = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "Other Domain" };
            var paramValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            paramValueSet.Computed = new ValueArray<string>(new[] { "c" });
            paramValueSet.Manual = new ValueArray<string>(new[] { "m" });
            paramValueSet.Reference = new ValueArray<string>(new[] { "r" });
            paramValueSet.ValueSwitch = ParameterSwitchKind.COMPUTED;

            var paramSubscriptionValueSet = new ParameterSubscriptionValueSet(Guid.NewGuid(), this.cache, this.uri);
            paramSubscriptionValueSet.Manual = new ValueArray<string>(new[] { "m1" });
            paramSubscriptionValueSet.ValueSwitch = ParameterSwitchKind.REFERENCE;
            paramSubscriptionValueSet.SubscribedValueSet = paramValueSet;

            var testParamType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri);
            this.parameter = new Parameter { Owner = testDomain, ParameterType = testParamType };
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
            var testPerson = new Person(Guid.NewGuid(), null, null);
            var testParticipant = new Participant(Guid.NewGuid(), null, null) { Person = testPerson, SelectedDomain = testDomain };
            modelSetup.Participant.Add(testParticipant);
            this.parameterSubscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri) { Container = this.parameter };
            this.parameterSubscription.ValueSet.Add(paramSubscriptionValueSet);
            var elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri);
            elementDefinition.ContainedElement.Add(elementUsage);
            elementUsage.ElementDefinition = elementDefinition;
            this.model.Iteration.Add(this.iteration);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(testPerson);
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.parameterSubscription.Owner = subscription.Owner;

            var pt = new TextParameterType();
            this.parameter.ParameterType = pt;
            this.parameter.ParameterSubscription.Add(this.parameterSubscription);

            this.parameterClone = this.parameter.Clone(false);

            var transactionContext = TransactionContextResolver.ResolveContext(this.iteration);
            this.thingTransaction = new ThingTransaction(transactionContext, this.parameterClone);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [Test]
        public void VerifyThatPropertiesArePopulated()
        {
            var vm = new ParameterSubscriptionDialogViewModel(this.parameterSubscription, this.thingTransaction, this.session.Object, true,
                ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.parameterClone);

            Assert.IsNotNull(vm.SelectedParameterType);
            Assert.IsNotNull(vm.SelectedOwner);
            Assert.AreEqual(1, vm.ValueSet.Count);
            Assert.AreEqual(1, vm.PossibleOwner.Count);
            var valueSet = vm.ValueSet.First();

            Assert.AreEqual("m1", valueSet.Manual);
            Assert.AreEqual("r", valueSet.Reference);
            Assert.AreEqual("-", valueSet.Computed);
            Assert.AreEqual(ParameterSwitchKind.REFERENCE, valueSet.Switch);
            Assert.AreEqual("r", valueSet.Value);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            var dialogViewModel = new ParameterSubscriptionDialogViewModel();
            Assert.IsFalse(dialogViewModel.IsReadOnly);
        }

        [Test]
        public void VerifyUpdateOkExecute()
        {
            var vm = new ParameterSubscriptionDialogViewModel(this.parameterSubscription, this.thingTransaction, this.session.Object, true,
    ThingDialogKind.Create, this.thingDialogNavigationService.Object, this.parameterClone);

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
            vm.OkCommand.Execute(null);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
    }
}