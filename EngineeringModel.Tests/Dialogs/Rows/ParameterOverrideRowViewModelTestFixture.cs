// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterOverrideRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels.Dialogs;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class ParameterOverrideRowViewModelTestFixture
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

        private Option option1;
        private Option option2;

        private ActualFiniteStateList stateList;
        private ActualFiniteState actualState1;
        private ActualFiniteState actualState2;
        private PossibleFiniteState state1;
        private PossibleFiniteState state2;
        private PossibleFiniteStateList posStateList;

        private Assembler assembler;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
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

            this.cptParameter.ValueSet.Add(new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = this.option1,
                ActualState = this.stateList.ActualState.First()
            });

            this.cptParameter.ValueSet.Add(new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = this.option1,
                ActualState = this.stateList.ActualState.Last()
            });

            this.cptParameter.ValueSet.Add(new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = this.option2,
                ActualState = this.stateList.ActualState.First()
            });

            this.cptParameter.ValueSet.Add(new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = this.option2,
                ActualState = this.stateList.ActualState.Last()
            });

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), null, this.uri)
            {
                Owner = this.activeDomain
            };

            this.elementDefinitionForUsage1 = new ElementDefinition(Guid.NewGuid(), null, this.uri);
            this.elementUsage1 = new ElementUsage(Guid.NewGuid(), null, this.uri) { ElementDefinition = this.elementDefinitionForUsage1 };

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

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.assembler = new Assembler(this.uri, this.messageBus);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatParameterOverrideRowWorksNoOptionNoState()
        {
            var value = new List<string> { "test" };

            var parameterValue = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            parameterValue.Manual = new ValueArray<string>(value);
            parameterValue.Reference = new ValueArray<string>(value);
            parameterValue.Computed = new ValueArray<string>(value);
            parameterValue.Published = new ValueArray<string>(value);

            this.parameter.ValueSet.Add(parameterValue);

            var poverride = new ParameterOverride(Guid.NewGuid(), null, this.uri) { Parameter = this.parameter };
            var valueset = new ParameterOverrideValueSet(Guid.NewGuid(), null, this.uri) { ParameterValueSet = parameterValue };
            valueset.Manual = new ValueArray<string>(value);
            poverride.ValueSet.Add(valueset);

            this.elementUsage1.ParameterOverride.Add(poverride);

            var row = new ParameterOverrideRowViewModel(poverride, this.session.Object, null);
            Assert.AreEqual("test", row.Manual);
            Assert.AreEqual("-", row.Reference);
            Assert.AreEqual(0, row.ContainedRows.Count);
        }

        [Test]
        public void VerifyThatParameterOverrideRowWorksWithOptionNoState()
        {
            var value = new List<string> { "test" };

            var set1 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            set1.ActualOption = this.option1;
            set1.Manual = new ValueArray<string>(value);
            set1.Reference = new ValueArray<string>(value);
            set1.Computed = new ValueArray<string>(value);
            set1.Published = new ValueArray<string>(value);

            var set2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            set2.ActualOption = this.option2;
            set2.Manual = new ValueArray<string>(value);
            set2.Reference = new ValueArray<string>(value);
            set2.Computed = new ValueArray<string>(value);
            set2.Published = new ValueArray<string>(value);

            this.parameter.ValueSet.Add(set1);
            this.parameter.ValueSet.Add(set2);

            this.parameter.IsOptionDependent = true;
            this.parameter.Scale = new OrdinalScale(Guid.NewGuid(), null, this.uri);

            var poverride = new ParameterOverride(Guid.NewGuid(), null, this.uri) { Parameter = this.parameter };
            var valueset1 = new ParameterOverrideValueSet(Guid.NewGuid(), null, this.uri) { ParameterValueSet = set1 };
            var valueset2 = new ParameterOverrideValueSet(Guid.NewGuid(), null, this.uri) { ParameterValueSet = set2 };

            valueset1.Manual = new ValueArray<string>(value);
            valueset2.Reference = new ValueArray<string>(value);

            poverride.ValueSet.Add(valueset1);
            poverride.ValueSet.Add(valueset2);

            this.elementUsage1.ParameterOverride.Add(poverride);

            var row = new ParameterOverrideRowViewModel(poverride, this.session.Object, null);
            Assert.AreEqual(2, row.ContainedRows.Count);

            var o1row =
                row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option1);

            var o2row =
                row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option2);

            Assert.AreEqual(o1row.Manual, "test");
            Assert.AreEqual(o2row.Reference, "test");
        }

        [Test]
        public void VerifyThatParameterOverrideRowWorksNoOptionWithState()
        {
            var value = new List<string> { "test" };

            var set1 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            set1.ActualState = this.actualState1;
            set1.Manual = new ValueArray<string>(value);
            set1.Reference = new ValueArray<string>(value);
            set1.Computed = new ValueArray<string>(value);
            set1.Published = new ValueArray<string>(value);

            var set2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            set2.ActualState = this.actualState2;
            set2.Manual = new ValueArray<string>(value);
            set2.Reference = new ValueArray<string>(value);
            set2.Computed = new ValueArray<string>(value);
            set2.Published = new ValueArray<string>(value);

            this.parameter.ValueSet.Add(set1);
            this.parameter.ValueSet.Add(set2);

            this.parameter.StateDependence = this.stateList;

            var poverride = new ParameterOverride(Guid.NewGuid(), null, this.uri) { Parameter = this.parameter };
            var valueset1 = new ParameterOverrideValueSet(Guid.NewGuid(), null, this.uri) { ParameterValueSet = set1 };
            var valueset2 = new ParameterOverrideValueSet(Guid.NewGuid(), null, this.uri) { ParameterValueSet = set2 };

            valueset1.Manual = new ValueArray<string>(value);
            valueset2.Reference = new ValueArray<string>(value);

            poverride.ValueSet.Add(valueset1);
            poverride.ValueSet.Add(valueset2);

            this.elementUsage1.ParameterOverride.Add(poverride);

            var row = new ParameterOverrideRowViewModel(poverride, this.session.Object, null);
            Assert.AreEqual(2, row.ContainedRows.Count);

            var s1row =
                row.ContainedRows.OfType<ParameterStateRowViewModel>().Single(x => x.ActualState == this.actualState1);

            var s2row =
                row.ContainedRows.OfType<ParameterStateRowViewModel>().Single(x => x.ActualState == this.actualState2);

            Assert.AreEqual(s1row.Manual, "test");
            Assert.AreEqual(s2row.Reference, "test");
        }

        [Test]
        public void VerifyThatParameterOverrideRowWorksWithOptionWithState()
        {
            var value = new List<string> { "test" };

            var set1 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            set1.ActualState = this.actualState1;
            set1.ActualOption = this.option1;
            set1.Manual = new ValueArray<string>(value);
            set1.Reference = new ValueArray<string>(value);
            set1.Computed = new ValueArray<string>(value);
            set1.Published = new ValueArray<string>(value);

            var set2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            set2.ActualState = this.actualState2;
            set2.ActualOption = this.option1;
            set2.Manual = new ValueArray<string>(value);
            set2.Reference = new ValueArray<string>(value);
            set2.Computed = new ValueArray<string>(value);
            set2.Published = new ValueArray<string>(value);

            var set3 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            set1.ActualState = this.actualState1;
            set1.ActualOption = this.option2;
            set1.Manual = new ValueArray<string>(value);
            set1.Reference = new ValueArray<string>(value);
            set1.Computed = new ValueArray<string>(value);
            set1.Published = new ValueArray<string>(value);

            var set4 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            set2.ActualState = this.actualState2;
            set2.ActualOption = this.option2;
            set2.Manual = new ValueArray<string>(value);
            set2.Reference = new ValueArray<string>(value);
            set2.Computed = new ValueArray<string>(value);
            set2.Published = new ValueArray<string>(value);

            this.parameter.ValueSet.Add(set1);
            this.parameter.ValueSet.Add(set2);
            this.parameter.ValueSet.Add(set3);
            this.parameter.ValueSet.Add(set4);

            this.parameter.StateDependence = this.stateList;
            this.parameter.IsOptionDependent = true;

            var poverride = new ParameterOverride(Guid.NewGuid(), null, this.uri) { Parameter = this.parameter };
            var valueset1 = new ParameterOverrideValueSet(Guid.NewGuid(), null, this.uri) { ParameterValueSet = set1 };
            var valueset2 = new ParameterOverrideValueSet(Guid.NewGuid(), null, this.uri) { ParameterValueSet = set2 };
            var valueset3 = new ParameterOverrideValueSet(Guid.NewGuid(), null, this.uri) { ParameterValueSet = set3 };
            var valueset4 = new ParameterOverrideValueSet(Guid.NewGuid(), null, this.uri) { ParameterValueSet = set4 };

            valueset1.Manual = new ValueArray<string>(value);
            valueset2.Reference = new ValueArray<string>(value);

            poverride.ValueSet.Add(valueset1);
            poverride.ValueSet.Add(valueset2);
            poverride.ValueSet.Add(valueset3);
            poverride.ValueSet.Add(valueset4);

            this.elementUsage1.ParameterOverride.Add(poverride);

            var row = new ParameterOverrideRowViewModel(poverride, this.session.Object, null);
            Assert.AreEqual(2, row.ContainedRows.Count);

            var o1row = row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option1);
            var o2row = row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option2);

            Assert.AreEqual(2, o1row.ContainedRows.Count);
            Assert.AreEqual(2, o2row.ContainedRows.Count);
        }
    }
}
