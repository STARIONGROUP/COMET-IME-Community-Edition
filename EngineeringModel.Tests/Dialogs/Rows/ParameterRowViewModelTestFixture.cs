// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterRowViewModelTestFixture.cs" company="RHEA System S.A.">
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
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels.Dialogs;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class ParameterRowViewModelTestFixture
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
        private ActualFiniteState actualState3;
        private PossibleFiniteState state1;
        private PossibleFiniteState state2;
        private PossibleFiniteState state3;
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
            this.state3 = new PossibleFiniteState(Guid.NewGuid(), null, this.uri) { Name = "state3" };

            this.posStateList = new PossibleFiniteStateList(Guid.NewGuid(), null, this.uri);
            this.posStateList.PossibleState.Add(this.state1);
            this.posStateList.PossibleState.Add(this.state2);
            this.posStateList.PossibleState.Add(this.state3);
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

            this.actualState3 = new ActualFiniteState(Guid.NewGuid(), null, this.uri)
            {
                PossibleState = new List<PossibleFiniteState> { this.state3 },
                Kind = ActualFiniteStateKind.FORBIDDEN
            };

            this.stateList.ActualState.Add(this.actualState1);
            this.stateList.ActualState.Add(this.actualState2);
            this.stateList.ActualState.Add(this.actualState3);

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

            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option1, this.actualState1));
            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option1, this.actualState2));
            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option1, this.actualState3));
            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option2, this.actualState1));
            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option2, this.actualState2));
            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option2, this.actualState3));

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

        /// <summary>
        /// Creates and returns a new parameter value set for the given option and state
        /// </summary>
        /// <param name="option">
        /// The option.
        /// </param>
        /// <param name="state">
        /// The state.
        /// </param>
        /// <returns>
        /// The <see cref="ParameterValueSet"/>.
        /// </returns>
        private ParameterValueSet GetNewParameterValueSet(Option option, ActualFiniteState state)
        {
            return new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                ActualOption = option,
                ActualState = state,
                Computed = new ValueArray<string>(new List<string> { "-" }),
                Manual = new ValueArray<string>(new List<string> { "-" }),
                Reference = new ValueArray<string>(new List<string> { "-" }),
                Formula = new ValueArray<string>(new List<string> { "-" }),
                Published = new ValueArray<string>(new List<string> { "-" }),
                ValueSwitch = ParameterSwitchKind.COMPUTED
            };
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRowBuiltCorrectly()
        {
            var row = new ParameterRowViewModel(this.cptParameter, this.session.Object, null);
            var option1Row = row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option1);
            var option2Row = row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option2);

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

            o1s1c2Row.Reference = new ReactiveList<EnumerationValueDefinition> { this.enum1 };
            o1s2c2Row.Reference = new ReactiveList<EnumerationValueDefinition> { this.enum1 };
            o2s1c2Row.Reference = new ReactiveList<EnumerationValueDefinition> { this.enum2 };
            o2s2c2Row.Reference = new ReactiveList<EnumerationValueDefinition> { this.enum2 };

            var o1s1Set = this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option1 && x.ActualState == this.actualState1);
            var o1s2Set = this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option1 && x.ActualState == this.actualState2);
            var o1s3Set = this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option1 && x.ActualState == this.actualState3);
            var o2s1Set = this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option2 && x.ActualState == this.actualState1);
            var o2s2Set = this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option2 && x.ActualState == this.actualState2);
            var o2s3Set = this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option2 && x.ActualState == this.actualState3);

            row.UpdateValueSets(o1s1Set);
            row.UpdateValueSets(o1s2Set);
            row.UpdateValueSets(o1s3Set);
            row.UpdateValueSets(o2s1Set);
            row.UpdateValueSets(o2s2Set);
            row.UpdateValueSets(o2s3Set);

            Assert.AreEqual(o1s1Set.ValueSwitch, o1s1c1Row.Switch);
            Assert.AreEqual(o1s2Set.ValueSwitch, o1s2c1Row.Switch);
            Assert.AreEqual(o2s1Set.ValueSwitch, o2s1c1Row.Switch);
            Assert.AreEqual(o2s2Set.ValueSwitch, o2s2c1Row.Switch);

            Assert.AreEqual(o1s1Set.Reference[0], o1s1c1Row.Reference);
            Assert.AreEqual(o1s2Set.Reference[0], o1s2c1Row.Reference);
            Assert.AreEqual(o2s1Set.Reference[0], o2s1c1Row.Reference);
            Assert.AreEqual(o2s2Set.Reference[0], o2s2c1Row.Reference);

            Assert.AreEqual(o1s1Set.Reference[1], ValueSetConverter.ToValueSetString(o1s1c2Row.Reference, o1s1c2Row.ParameterType));
            Assert.AreEqual(o1s2Set.Reference[1], ValueSetConverter.ToValueSetString(o1s2c2Row.Reference, o1s2c2Row.ParameterType));
            Assert.AreEqual(o2s1Set.Reference[1], ValueSetConverter.ToValueSetString(o2s1c2Row.Reference, o2s1c2Row.ParameterType));
            Assert.AreEqual(o2s2Set.Reference[1], ValueSetConverter.ToValueSetString(o2s2c2Row.Reference, o2s2c2Row.ParameterType));
        }
    }
}
