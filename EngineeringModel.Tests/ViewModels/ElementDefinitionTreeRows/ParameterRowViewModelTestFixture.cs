// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.ElementDefinitionTreeRows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

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
        private PossibleFiniteState state1;
        private PossibleFiniteState state2;
        private PossibleFiniteStateList posStateList;

        private Assembler assembler;

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

            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option1, this.stateList.ActualState.First()));
            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option1, this.stateList.ActualState.Last()));
            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option2, this.stateList.ActualState.First()));
            this.cptParameter.ValueSet.Add(this.GetNewParameterValueSet(this.option2, this.stateList.ActualState.Last()));

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

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatUpdateValueSetWorks()
        {
            var row = new ParameterRowViewModel(this.cptParameter, this.session.Object, null, false);
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
            Assert.IsFalse(option1Row.IsEditable());

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

            var o1s1Set =this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option1 && x.ActualState == this.actualState1);
            var o1s2Set = this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option1 && x.ActualState == this.actualState2);
            var o2s1Set = this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option2 && x.ActualState == this.actualState1);
            var o2s2Set = this.cptParameter.ValueSet.Single(x => x.ActualOption == this.option2 && x.ActualState == this.actualState2);

            row.UpdateValueSets(o1s1Set);
            row.UpdateValueSets(o1s2Set);
            row.UpdateValueSets(o2s1Set);
            row.UpdateValueSets(o2s2Set);

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

        [Test]
        public void VerifyThatUpdateValueSetUpdatesRow()
        {
            var valueset = new ParameterValueSet(Guid.NewGuid(), null, this.uri);
            this.parameter.ValueSet.Add(valueset);

            var row = new ParameterRowViewModel(this.parameter, this.session.Object, null, false);

            var revInfo = typeof (Thing).GetProperty("RevisionNumber");
            revInfo.SetValue(valueset, 10);

            Assert.AreEqual("-", row.Manual);
            valueset.Manual = new ValueArray<string>(new List<string>{"test"});
            CDPMessageBus.Current.SendObjectChangeEvent(valueset, EventKind.Updated);

            Assert.AreEqual("test", row.Manual);
            row.Dispose();
        }

        [Test]
        public void VerifyThatValueSetInlineEditWorksCompoundOptionState()
        {
            var row = new ParameterRowViewModel(this.cptParameter, this.session.Object, null, false);
            var option1Row =
                row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option1);

            var o1s1Row = option1Row.ContainedRows.OfType<ParameterStateRowViewModel>()
                    .Single(x => x.ActualState == this.actualState1);

            var o1s1c1Row = o1s1Row.ContainedRows.OfType<ParameterComponentValueRowViewModel>().First();

            o1s1c1Row.Switch = ParameterSwitchKind.REFERENCE;
            o1s1c1Row.CreateCloneAndWrite(ParameterSwitchKind.REFERENCE, "Switch");

            this.session.Verify(x => x.Write(It.Is<OperationContainer>(op => ((CDP4Common.DTO.ParameterValueSet)op.Operations.Single().ModifiedThing).ValueSwitch == ParameterSwitchKind.REFERENCE)));
        }

        [Test]
        public void VerifyThatValueSetInlineEditWorksSimpleOptionNoState()
        {
            this.parameter.IsOptionDependent = true;
            var set1 = new ParameterValueSet(Guid.NewGuid(), null, this.uri) { ActualOption = this.option1 };
            var set2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri) { ActualOption = this.option2 };

            this.parameter.ValueSet.Add(set1);
            this.parameter.ValueSet.Add(set2);

            var row = new ParameterRowViewModel(this.parameter, this.session.Object, null, false);
            var option1Row =
                row.ContainedRows.OfType<ParameterOptionRowViewModel>().Single(x => x.ActualOption == this.option1);

            option1Row.Switch = ParameterSwitchKind.REFERENCE;
            option1Row.CreateCloneAndWrite(ParameterSwitchKind.REFERENCE, "Switch");

            this.session.Verify(x => x.Write(It.Is<OperationContainer>(op => ((CDP4Common.DTO.ParameterValueSet)op.Operations.Single().ModifiedThing).ValueSwitch == ParameterSwitchKind.REFERENCE)));
        }

        [Test]
        public void VerifyThatValueSetInlineEditWorksSimpleNoOptionState()
        {
            this.parameter.IsOptionDependent = false;
            this.parameter.StateDependence = this.stateList;
            var set1 = new ParameterValueSet(Guid.NewGuid(), null, this.uri) { ActualState = this.actualState1 };
            var set2 = new ParameterValueSet(Guid.NewGuid(), null, this.uri) { ActualState = this.actualState2 };

            this.parameter.ValueSet.Add(set1);
            this.parameter.ValueSet.Add(set2);

            var row = new ParameterRowViewModel(this.parameter, this.session.Object, null, false);
            var option1Row =
                row.ContainedRows.OfType<ParameterStateRowViewModel>().Single(x => x.ActualState == this.actualState1);

            option1Row.Switch = ParameterSwitchKind.REFERENCE;
            option1Row.CreateCloneAndWrite(ParameterSwitchKind.REFERENCE, "Switch");

            this.session.Verify(x => x.Write(It.Is<OperationContainer>(op => ((CDP4Common.DTO.ParameterValueSet)op.Operations.Single().ModifiedThing).ValueSwitch == ParameterSwitchKind.REFERENCE)));
        }

        [Test]
        public void VerifyThatValueSetInlineEditWorksSimpleNoOptionNoState()
        {
            this.parameter.IsOptionDependent = false;
            this.parameter.StateDependence = null;
            var set1 = new ParameterValueSet(Guid.NewGuid(), null, this.uri);

            this.parameter.ValueSet.Add(set1);

            var row = new ParameterRowViewModel(this.parameter, this.session.Object, null, false);

            row.Switch = ParameterSwitchKind.REFERENCE;
            row.CreateCloneAndWrite(ParameterSwitchKind.REFERENCE, "Switch");

            this.session.Verify(x => x.Write(It.Is<OperationContainer>(op => ((CDP4Common.DTO.ParameterValueSet)op.Operations.Single().ModifiedThing).ValueSwitch == ParameterSwitchKind.REFERENCE)));
        }

        [Test]
        public void VerifyThatStartDragWithPermissionWorks()
        {
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var row = new ParameterRowViewModel(this.cptParameter, this.session.Object, null, false);

            var draginfo = new Mock<IDragInfo>();
            draginfo.SetupProperty(x => x.Payload);
            draginfo.SetupProperty(x => x.Effects);
            row.StartDrag(draginfo.Object);

            Assert.AreEqual(DragDropEffects.All, draginfo.Object.Effects);
            Assert.AreSame(this.cptParameter, draginfo.Object.Payload);
        }

        [Test]
        public void VerifyThatGroupCanBeDropped1()
        {
            var row = new ParameterRowViewModel(this.cptParameter, this.session.Object, null, false);

            var group1 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var group2 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.elementDefinitionForUsage1.ParameterGroup.Add(group1);
            this.elementDefinitionForUsage1.ParameterGroup.Add(group2);

            this.assembler.Cache.TryAdd(new Tuple<Guid, Guid?>(group1.Iid, this.iteration.Iid), new Lazy<Thing>(() => group1));
            this.assembler.Cache.TryAdd(new Tuple<Guid, Guid?>(group2.Iid, this.iteration.Iid), new Lazy<Thing>(() => group2));

            group2.ContainingGroup = group1;

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(group2);
            dropinfo.SetupProperty(x => x.Effects);

            row.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Move, dropinfo.Object.Effects);

            row.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatGroupCanBeDropped2()
        {
            var row = new ParameterRowViewModel(this.cptParameter, this.session.Object, null, false);

            var group1 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var group2 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.elementDefinitionForUsage1.ParameterGroup.Add(group1);
            this.elementDefinitionForUsage1.ParameterGroup.Add(group2);

            this.assembler.Cache.TryAdd(new Tuple<Guid, Guid?>(group1.Iid, this.iteration.Iid), new Lazy<Thing>(() => group1));
            this.assembler.Cache.TryAdd(new Tuple<Guid, Guid?>(group2.Iid, this.iteration.Iid), new Lazy<Thing>(() => group2));

            this.cptParameter.Group = group1;
            group2.ContainingGroup = group1;

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(group2);
            dropinfo.SetupProperty(x => x.Effects);

            row.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Move, dropinfo.Object.Effects);

            row.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        [Test]
        public void VerifyThatGroupCannotBeDropped2()
        {
            var row = new ParameterRowViewModel(this.cptParameter, this.session.Object, null, false);

            var group1 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var group2 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.assembler.Cache.TryAdd(new Tuple<Guid, Guid?>(group1.Iid, this.iteration.Iid), new Lazy<Thing>(() => group1));
            this.assembler.Cache.TryAdd(new Tuple<Guid, Guid?>(group2.Iid, this.iteration.Iid), new Lazy<Thing>(() => group2));

            this.elementDefinitionForUsage1.ParameterGroup.Add(group1);
            this.elementDefinitionForUsage1.ParameterGroup.Add(group2);

            this.cptParameter.Group = group1;
            group2.ContainingGroup = group1;

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(group1);
            dropinfo.SetupProperty(x => x.Effects);

            row.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.None, dropinfo.Object.Effects);

            row.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Never);
        }

        [Test]
        public void VerifyThatParameterCanBeDropped()
        {
            var row = new ParameterRowViewModel(this.cptParameter, this.session.Object, null, false);

            var group1 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            var group2 = new ParameterGroup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.assembler.Cache.TryAdd(new Tuple<Guid, Guid?>(group1.Iid, this.iteration.Iid), new Lazy<Thing>(() => group1));
            this.assembler.Cache.TryAdd(new Tuple<Guid, Guid?>(group2.Iid, this.iteration.Iid), new Lazy<Thing>(() => group2));

            this.elementDefinitionForUsage1.ParameterGroup.Add(group1);
            this.elementDefinitionForUsage1.ParameterGroup.Add(group2);

            this.cptParameter.Group = group1;
            group2.ContainingGroup = group1;

            this.parameter.Group = null;

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.parameter);
            dropinfo.SetupProperty(x => x.Effects);

            row.DragOver(dropinfo.Object);

            Assert.AreEqual(DragDropEffects.Move, dropinfo.Object.Effects);

            row.Drop(dropinfo.Object);
            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()), Times.Once);
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
    }
}