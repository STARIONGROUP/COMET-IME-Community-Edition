// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSubscriptionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs
{ 
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels.Dialogs;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    internal class ParameterSubscriptionRowViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialognavigationService;
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");
        private Participant participant;
        private Person person;
        private DomainOfExpertise activeDomain;
        private DomainOfExpertise someotherDomain;
        private EngineeringModel model;
        private EngineeringModelSetup modelsetup;
        private QuantityKind qqParamType;
        private EnumerationParameterType enumPt;
        private EnumerationValueDefinition enum1;
        private EnumerationValueDefinition enum2;
        private CompoundParameterType cptType;
        private Iteration iteration;
        private ElementDefinition elementDefinition;
        private ElementDefinition elementDefinitionForUsage1;
        private ElementUsage elementUsage1;

        private Parameter parameter;
        private Parameter cptParameter;
        private ParameterSubscription subscription;

        private Option option1;
        private Option option2;

        private ActualFiniteStateList stateList;
        private ActualFiniteState actualState1;
        private ActualFiniteState actualState2;
        private PossibleFiniteState state1;
        private PossibleFiniteState state2;
        private PossibleFiniteStateList posStateList;

        private ParameterValueSet valueset1;
        private ParameterValueSet valueset2;
        private ParameterValueSet valueset3;
        private ParameterValueSet valueset4;


        [SetUp]
        public void Setup()
        {
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.thingDialognavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.stateList = new ActualFiniteStateList(Guid.NewGuid(), null, this.uri);
            this.state1 = new PossibleFiniteState(Guid.NewGuid(), null, this.uri) { Name = "state1" };
            this.state2 = new PossibleFiniteState(Guid.NewGuid(), null, this.uri) { Name = "state2" };

            this.posStateList = new PossibleFiniteStateList(Guid.NewGuid(), null, this.uri);
            this.posStateList.PossibleState.Add(this.state1);
            this.posStateList.PossibleState.Add(this.state2);
            this.posStateList.DefaultState = this.state1;

            this.actualState1 = new ActualFiniteState(Guid.NewGuid(), null, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.state1 },
                Kind = ActualFiniteStateKind.MANDATORY
            };

            this.actualState2 = new ActualFiniteState(Guid.NewGuid(), null, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.state2 },
                Kind = ActualFiniteStateKind.MANDATORY
            };

            this.stateList.ActualState.Add(this.actualState1);
            this.stateList.ActualState.Add(this.actualState2);

            this.stateList.PossibleFiniteStateList.Add(this.posStateList);

            this.option1 = new Option(Guid.NewGuid(), null, this.uri) { Name = "option1" };
            this.option2 = new Option(Guid.NewGuid(), null, this.uri) { Name = "option2" };

            this.qqParamType = new SimpleQuantityKind(Guid.NewGuid(), null, this.uri)
            {
                Name = "PTName",
                ShortName = "PTShortName"
            };

            this.enum1 = new EnumerationValueDefinition(Guid.NewGuid(), null, this.uri) { Name = "enum1" };
            this.enum2 = new EnumerationValueDefinition(Guid.NewGuid(), null, this.uri) { Name = "enum2" };
            this.enumPt = new EnumerationParameterType(Guid.NewGuid(), null, this.uri);
            this.enumPt.ValueDefinition.Add(this.enum1);
            this.enumPt.ValueDefinition.Add(this.enum2);

            this.cptType = new CompoundParameterType(Guid.NewGuid(), null, this.uri)
            {
                Name = "APTName",
                ShortName = "APTShortName"
            };

            this.cptType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), null, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.qqParamType,
                ShortName = "c1"
            });

            this.cptType.Component.Add(new ParameterTypeComponent(Guid.NewGuid(), null, this.uri)
            {
                Iid = Guid.NewGuid(),
                ParameterType = this.enumPt,
                ShortName = "c2"
            });

            this.activeDomain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "active", ShortName = "active" };
            this.someotherDomain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "other", ShortName = "other" };

            this.parameter = new Parameter(Guid.NewGuid(), null, this.uri)
            {
                Owner = this.activeDomain,
                ParameterType = this.qqParamType
            };

            this.cptParameter = new Parameter(Guid.NewGuid(), null, this.uri)
            {
                Owner = this.activeDomain,
                ParameterType = this.cptType,
                IsOptionDependent = true,
                StateDependence = this.stateList
            };

            this.valueset1 = new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = this.option1,
                ActualState = this.stateList.ActualState.First()
            };

            this.valueset2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = this.option1,
                ActualState = this.stateList.ActualState.Last()
            };

            this.valueset3 = new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = this.option2,
                ActualState = this.stateList.ActualState.First()
            };

            this.valueset4 = new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = this.option2,
                ActualState = this.stateList.ActualState.Last()
            };

            this.cptParameter.ValueSet.Add(this.valueset1);
            this.cptParameter.ValueSet.Add(this.valueset2);
            this.cptParameter.ValueSet.Add(this.valueset3);
            this.cptParameter.ValueSet.Add(this.valueset4);

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), null, this.uri)
            {
                Owner = this.activeDomain
            };
            this.elementDefinitionForUsage1 = new ElementDefinition(Guid.NewGuid(), null, this.uri);
            this.elementUsage1 = new ElementUsage(Guid.NewGuid(), null, this.uri){ElementDefinition = this.elementDefinitionForUsage1};

            this.elementDefinition.ContainedElement.Add(this.elementUsage1);

            this.elementDefinitionForUsage1.Parameter.Add(this.parameter);
            this.elementDefinitionForUsage1.Parameter.Add(this.cptParameter);

            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iteration.Element.Add(this.elementDefinition);
            this.iteration.Element.Add(this.elementDefinitionForUsage1);

            this.iteration.Option.Add(this.option1);
            this.iteration.Option.Add(this.option2);

            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.model.Iteration.Add(this.iteration);

            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "test", Surname = "test" };
            this.participant = new Participant(Guid.NewGuid(), null, this.uri) { Person = this.person, SelectedDomain = this.activeDomain };
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.modelsetup.Participant.Add(this.participant);
            this.model.EngineeringModelSetup = this.modelsetup;

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatUpdateValueSetWorksCompoundOptionState()
        {
            this.subscription = new ParameterSubscription(Guid.NewGuid(), null, this.uri);
            this.cptParameter.ParameterSubscription.Add(subscription);

            this.subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), null, this.uri)
            {
                SubscribedValueSet = this.valueset1
            });
            this.subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), null, this.uri)
            {
                SubscribedValueSet = this.valueset2
            });
            this.subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), null, this.uri)
            {
                SubscribedValueSet = this.valueset3
            });
            this.subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), null, this.uri)
            {
                SubscribedValueSet = this.valueset4
            });

            var row = new ParameterSubscriptionRowViewModel(this.subscription, this.session.Object, null);
            var option1Row =
                row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option1);

            var option2Row =
                row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option2);

            var o1s1Row = option1Row.ContainedRows.OfType<ParameterStateRowViewModel>()
                    .Single(x => x.ActualState == this.actualState1);
            var o1s2Row = option1Row.ContainedRows.OfType<ParameterStateRowViewModel>()
                    .Single(x => x.ActualState == this.actualState2);
            var o2s1Row = option2Row.ContainedRows.OfType<ParameterStateRowViewModel>()
                    .Single(x => x.ActualState == this.actualState1);
            var o2s2Row = option2Row.ContainedRows.OfType<ParameterStateRowViewModel>()
                    .Single(x => x.ActualState == this.actualState2);

            var o1s1c1Row = o1s1Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().First();
            var o1s1c2Row = o1s1Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().Last();
            var o1s2c1Row = o1s2Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().First();
            var o1s2c2Row = o1s2Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().Last();
            var o2s1c1Row = o2s1Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().First();
            var o2s1c2Row = o2s1Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().Last();
            var o2s2c1Row = o2s2Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().First();
            var o2s2c2Row = o2s2Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().Last();

            // option row            
            Assert.IsFalse(option1Row.IsEditable());
            Assert.IsFalse(option2Row.IsEditable());

            // state row            
            Assert.IsFalse(o1s1Row.IsEditable());
            Assert.IsFalse(o1s2Row.IsEditable());
            Assert.IsFalse(o2s1Row.IsEditable());
            Assert.IsFalse(o2s2Row.IsEditable());

            // component row            
            Assert.IsTrue(o1s1c1Row.IsEditable());
            Assert.IsTrue(o1s2c1Row.IsEditable());
            Assert.IsTrue(o2s1c1Row.IsEditable());
            Assert.IsTrue(o2s2c1Row.IsEditable());
            
            o1s1c1Row.Switch = ParameterSwitchKind.REFERENCE;
            o1s1c2Row.Switch = ParameterSwitchKind.REFERENCE;
            o1s2c1Row.Switch = ParameterSwitchKind.REFERENCE;
            o1s2c2Row.Switch = ParameterSwitchKind.REFERENCE;
            o2s1c1Row.Switch = ParameterSwitchKind.REFERENCE;
            o2s1c2Row.Switch = ParameterSwitchKind.REFERENCE;
            o2s2c1Row.Switch = ParameterSwitchKind.REFERENCE;
            o2s2c2Row.Switch = ParameterSwitchKind.REFERENCE;

            o1s1c2Row.Manual = new ReactiveList<EnumerationValueDefinition> { this.enum1 };
            o1s2c2Row.Manual = new ReactiveList<EnumerationValueDefinition> { this.enum1 };
            o2s1c2Row.Manual = new ReactiveList<EnumerationValueDefinition> { this.enum2 };
            o2s2c2Row.Manual = new ReactiveList<EnumerationValueDefinition> { this.enum2 };

            var o1s1Set = this.subscription.ValueSet.Single(x => x.ActualOption == this.option1 && x.ActualState == this.actualState1);
            var o1s2Set = this.subscription.ValueSet.Single(x => x.ActualOption == this.option1 && x.ActualState == this.actualState2);
            var o2s1Set = this.subscription.ValueSet.Single(x => x.ActualOption == this.option2 && x.ActualState == this.actualState1);
            var o2s2Set = this.subscription.ValueSet.Single(x => x.ActualOption == this.option2 && x.ActualState == this.actualState2);

            row.UpdateValueSets(this.subscription);

            Assert.AreEqual(o1s1Set.ValueSwitch, o1s1c1Row.Switch);
            Assert.AreEqual(o1s2Set.ValueSwitch, o1s2c1Row.Switch);
            Assert.AreEqual(o2s1Set.ValueSwitch, o2s1c1Row.Switch);
            Assert.AreEqual(o2s2Set.ValueSwitch, o2s2c1Row.Switch);

            Assert.AreEqual(o1s1Set.Manual[0], o1s1c1Row.Manual);
            Assert.AreEqual(o1s2Set.Manual[0], o1s2c1Row.Manual);
            Assert.AreEqual(o2s1Set.Manual[0], o2s1c1Row.Manual);
            Assert.AreEqual(o2s2Set.Manual[0], o2s2c1Row.Manual);

            Assert.AreEqual(o1s1Set.Manual[1], ValueSetConverter.ToValueSetString(o1s1c2Row.Manual, o1s1c2Row.ParameterType));
            Assert.AreEqual(o1s2Set.Manual[1], ValueSetConverter.ToValueSetString(o1s2c2Row.Manual, o1s2c2Row.ParameterType));
            Assert.AreEqual(o2s1Set.Manual[1], ValueSetConverter.ToValueSetString(o2s1c2Row.Manual, o2s1c2Row.ParameterType));
            Assert.AreEqual(o2s2Set.Manual[1], ValueSetConverter.ToValueSetString(o2s2c2Row.Manual, o2s2c2Row.ParameterType));
        }

        [Test]
        public void VerifyThatRowIsBuiltCorrectlyCompoundOptionNoState()
        {
            this.cptParameter.StateDependence = null;

            this.cptParameter.ValueSet.Clear();
            this.valueset1.ActualState = null;
            this.valueset3.ActualState = null;
            this.cptParameter.ValueSet.Add(this.valueset1);
            this.cptParameter.ValueSet.Add(this.valueset3);

            this.subscription = new ParameterSubscription(Guid.NewGuid(), null, this.uri);
            this.cptParameter.ParameterSubscription.Add(subscription);

            this.subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), null, this.uri)
            {
                SubscribedValueSet = this.valueset1
            });
            this.subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), null, this.uri)
            {
                SubscribedValueSet = this.valueset3
            });
            
            var row = new ParameterSubscriptionRowViewModel(this.subscription, this.session.Object, null);
            var option1Row = row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option1);
            var option2Row = row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option2);

            var o1c1Row = option1Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().First();
            var o1c2Row = option1Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().Last();
            var o2c1Row = option2Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().First();
            var o2c2Row = option2Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().Last();

            // option row
            Assert.IsFalse(option1Row.IsEditable());
            Assert.IsFalse(option1Row.IsEditable());

            // component row
            Assert.IsTrue(o1c1Row.IsEditable());
            Assert.IsTrue(o1c2Row.IsEditable());
            Assert.IsTrue(o2c1Row.IsEditable());
            Assert.IsTrue(o2c2Row.IsEditable());
            
            o1c1Row.Switch = ParameterSwitchKind.MANUAL;
            o1c2Row.Switch = ParameterSwitchKind.MANUAL;
            o2c1Row.Switch = ParameterSwitchKind.MANUAL;
            o2c2Row.Switch = ParameterSwitchKind.MANUAL;

            o1c2Row.Manual = new ReactiveList<EnumerationValueDefinition> { this.enum1 };
            o2c2Row.Manual = new ReactiveList<EnumerationValueDefinition> { this.enum2 };

            var o1Set = this.subscription.ValueSet.Single(x => x.ActualOption == this.option1);
            var o2Set = this.subscription.ValueSet.Single(x => x.ActualOption == this.option2);

            row.UpdateValueSets(this.subscription);

            Assert.AreEqual(o1Set.ValueSwitch, o1c1Row.Switch);
            Assert.AreEqual(o2Set.ValueSwitch, o2c2Row.Switch);

            Assert.AreEqual(o1Set.Manual[0], o1c1Row.Manual);
            Assert.AreEqual(o2Set.Manual[0], o2c1Row.Manual);

            Assert.AreEqual(o1Set.Manual[1], ValueSetConverter.ToValueSetString(o1c2Row.Manual, o1c2Row.ParameterType));
            Assert.AreEqual(o2Set.Manual[1], ValueSetConverter.ToValueSetString(o2c2Row.Manual, o2c2Row.ParameterType));
        }
        
        [Test]
        public void VerifyThatRowIsBuiltCorrectlyCompoundNoOptionWithState()
        {
            this.cptParameter.IsOptionDependent = false;

            this.cptParameter.ValueSet.Clear();
            this.valueset1.ActualOption = null;
            this.valueset2.ActualOption = null;
            this.cptParameter.ValueSet.Add(this.valueset1);
            this.cptParameter.ValueSet.Add(this.valueset2);

            this.subscription = new ParameterSubscription(Guid.NewGuid(), null, this.uri);
            this.cptParameter.ParameterSubscription.Add(subscription);

            this.subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), null, this.uri)
            {
                SubscribedValueSet = this.valueset1
            });
            this.subscription.ValueSet.Add(new ParameterSubscriptionValueSet(Guid.NewGuid(), null, this.uri)
            {
                SubscribedValueSet = this.valueset2
            });

            var row = new ParameterSubscriptionRowViewModel(this.subscription, this.session.Object, null);
            var s1Row =
                row.ContainedRows.OfType<ParameterStateRowViewModel>().Single(x => x.ActualState == this.actualState1);

            var s2Row =
                row.ContainedRows.OfType<ParameterStateRowViewModel>().Single(x => x.ActualState == this.actualState2);

            var s1c1Row = s1Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().First();
            var s1c2Row = s1Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().Last();
            var s2c1Row = s2Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().First();
            var s2c2Row = s2Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().Last();

            // state row            
            Assert.IsFalse(s1Row.IsEditable());
            Assert.IsFalse(s1Row.IsEditable());

            // component row            
            Assert.IsTrue(s1c1Row.IsEditable());
            Assert.IsTrue(s1c2Row.IsEditable());
            Assert.IsTrue(s2c1Row.IsEditable());
            Assert.IsTrue(s2c2Row.IsEditable());
            
            s1c1Row.Switch = ParameterSwitchKind.MANUAL;
            s1c2Row.Switch = ParameterSwitchKind.MANUAL;
            s2c1Row.Switch = ParameterSwitchKind.MANUAL;
            s2c2Row.Switch = ParameterSwitchKind.MANUAL;

            s1c2Row.Manual = new ReactiveList<EnumerationValueDefinition> { this.enum1 };
            s2c2Row.Manual = new ReactiveList<EnumerationValueDefinition> { this.enum2 };

            var s1Set = this.subscription.ValueSet.Single(x => x.ActualState == this.actualState1);
            var s2Set = this.subscription.ValueSet.Single(x => x.ActualState == this.actualState2);

            row.UpdateValueSets(this.subscription);

            Assert.AreEqual(s1Set.ValueSwitch, s1c1Row.Switch);
            Assert.AreEqual(s2Set.ValueSwitch, s2c2Row.Switch);

            Assert.AreEqual(s1Set.Manual[0], s1c1Row.Manual);
            Assert.AreEqual(s2Set.Manual[0], s2c1Row.Manual);

            Assert.AreEqual(s1Set.Manual[1], ValueSetConverter.ToValueSetString(s1c2Row.Manual, s1c2Row.ParameterType));
            Assert.AreEqual(s2Set.Manual[1], ValueSetConverter.ToValueSetString(s2c2Row.Manual, s2c2Row.ParameterType));
        }
    }
}