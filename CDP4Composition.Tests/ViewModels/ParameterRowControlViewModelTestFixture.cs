// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterRowControlViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using NUnit.Framework;

    [TestFixture]
    internal class ParameterRowControlViewModelTestFixture
    {
        [SetUp]
        public void Setup()
        {
            this.dalUri = new Uri("http://test");

            this.assembler = new Assembler(this.dalUri, new CDPMessageBus());

            this.owner = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { ShortName = Name };

            this.option = new Option(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name
            };

            this.parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
                    {
                        ValueSwitch = ParameterSwitchKind.REFERENCE,
                        Published = new ValueArray<string>(new List<string> { "2", "3", "4" })
                    },
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
                    {
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        Reference = new ValueArray<string>(new List<string> { "5", "6", "7" })
                    }
                },
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { Name = Name, ShortName = Name },
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { ShortName = Name },
                Owner = this.owner
            };

            this.compoundParameterType = new CompoundParameterType(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name,
                ShortName = Name
            };

            this.text = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ShortName = "txt",
                Name = "Text"
            };

            this.length = new SimpleQuantityKind(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ShortName = "l",
                Name = "Length"
            };

            this.meter = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ShortName = "m",
                Name = "meter"
            };

            var component_1 = new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ParameterType = this.length,
                Scale = this.meter,
                ShortName = "x"
            };

            this.compoundParameterType.Component.Add(component_1);

            var component_2 = new ParameterTypeComponent(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ParameterType = this.text,
                ShortName = "txt"
            };

            this.compoundParameterType.Component.Add(component_2);

            this.parameter2 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
                    {
                        ValueSwitch = ParameterSwitchKind.REFERENCE, Reference = new ValueArray<string>(), Published = new ValueArray<string>()
                    }
                },
                ParameterType = this.compoundParameterType,
                Owner = this.owner
            };

            var possibleFiniteStateList = new PossibleFiniteStateList();
            possibleFiniteStateList.PossibleState.Add(new PossibleFiniteState { Name = "state1", ShortName = Name });
            possibleFiniteStateList.PossibleState.Add(new PossibleFiniteState { Name = "state2", ShortName = Name });

            var actualFiniteStateList = new ActualFiniteStateList();
            actualFiniteStateList.PossibleFiniteStateList.Add(possibleFiniteStateList);
            var actualState1 = new ActualFiniteState();
            actualState1.PossibleState.Add(possibleFiniteStateList.PossibleState[0]);
            var actualState2 = new ActualFiniteState();
            actualState2.PossibleState.Add(possibleFiniteStateList.PossibleState[1]);

            actualFiniteStateList.ActualState.Add(actualState1);
            actualFiniteStateList.ActualState.Add(actualState2);

            this.parameter3 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                StateDependence = actualFiniteStateList,
                IsOptionDependent = true,
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
                    {
                        ValueSwitch = ParameterSwitchKind.REFERENCE,
                        Published = new ValueArray<string>(new List<string> { "4" }),
                        ActualState = actualState1,
                        ActualOption = this.option
                    },
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
                    {
                        ValueSwitch = ParameterSwitchKind.MANUAL,
                        Reference = new ValueArray<string>(new List<string> { "6" }),
                        ActualState = actualState2,
                        ActualOption = this.option
                    }
                },
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { Name = Name, ShortName = Name },
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { ShortName = Name },
                Owner = this.owner
            };

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name,
                Parameter = { this.parameter, this.parameter2, this.parameter3 }
            };

            this.elementUsage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name,
                ElementDefinition = this.elementDefinition,
                Container = this.elementDefinition
            };
        }

        private ElementDefinition elementDefinition;
        private ElementUsage elementUsage;
        private Option option;
        private Parameter parameter;
        private Parameter parameter2;
        private Parameter parameter3;
        private ParameterOverride parameterOverride;
        private CompoundParameterType compoundParameterType;
        private DomainOfExpertise owner;
        private Assembler assembler;
        private const string Name = "test";
        private Uri dalUri;
        private SimpleQuantityKind length;
        private TextParameterType text;
        private RatioScale meter;

        [Test]
        public void VerifyCompoundParameterTypeRowViewModelProperties()
        {
            var parametersList = this.compoundParameterType.Component.SortedItems;
            var vm = new CompoundParameterTypeRowViewModel(parametersList, this.parameter2.ValueSet.FirstOrDefault());

            Assert.AreEqual(vm.ValueSet, this.parameter2.ValueSet.FirstOrDefault());
            Assert.AreEqual(2, vm.ParametersList.Count);

            var generatedRows = vm.GenerateCompoundParameterRowViewModels();
            Assert.AreEqual(2, generatedRows.Count);
        }

        [Test]
        public void VerifyProperties()
        {
            var vm = new ParameterRowControlViewModel(this.parameter, this.option);
            Assert.AreEqual(0, vm.ContainedRows.Count);
            Assert.AreEqual(vm.Parameter, this.parameter);
            Assert.AreEqual(vm.ActualOption, this.option);
            Assert.AreEqual(vm.Name, this.parameter.ParameterType.Name);
            Assert.AreEqual(vm.ShortName, this.parameter.ParameterType.ShortName);
            Assert.AreEqual(2, this.parameter.ValueSets.Count());
            Assert.AreEqual(vm.Switch, ParameterSwitchKind.REFERENCE.ToString());
            Assert.AreEqual($"{{2, 3, 4}} [{Name}]", vm.PublishedValue);
            Assert.AreEqual(vm.OwnerShortName, this.parameter.Owner.ShortName);

            vm = new ParameterRowControlViewModel(this.parameter2, this.option);
            Assert.AreEqual(2, vm.ContainedRows.Count);

            vm = new ParameterRowControlViewModel(this.parameter3, this.option);
            Assert.AreEqual(2, vm.ContainedRows.Count);
        }

        [Test]
        public void VerifyStateDependenceRowProperties()
        {
            var vm = new StateDependenceRowViewModel(this.parameter3.StateDependence.ActualState, this.parameter3.ValueSet);
            Assert.AreEqual(vm.ValueSets, this.parameter3.ValueSet);
            Assert.AreEqual(2, vm.ActualFiniteStates.Count);

            var generatedRows = vm.GenerateStateRows();
            Assert.AreEqual(2, generatedRows.Count);
        }
    }
}
