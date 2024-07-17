// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementParameterRowControlViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
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

namespace CDP4Composition.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Services;
    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using NUnit.Framework;

    [TestFixture]
    public class ElementParameterRowControlViewModelTestFixture
    {
        private ElementDefinition elementDefinition;
        private ElementUsage elementUsage;
        private Option option;
        private Parameter parameter;
        private Parameter parameter2;
        private ParameterOverride parameterOverride;
        private DomainOfExpertise owner;
        private Assembler assembler;
        private const string Name = "test";
        private Uri dalUri;

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
                        Published = new ValueArray<string>(new List<string>() { "2", "3", "4" })
                    },
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
                    {
                        ValueSwitch = ParameterSwitchKind.COMPUTED, 
                        Computed = new ValueArray<string>(new List<string>() { "5", "6", "7" })
                    }
                },
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { Name = Name, ShortName = Name },
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { ShortName = Name },
                Owner = this.owner
            };

            this.parameter2 = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ValueSet =
                {
                    new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
                    {
                        ValueSwitch = ParameterSwitchKind.REFERENCE, Reference = new ValueArray<string>(), Published = new ValueArray<string>()
                    }
                },
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { Name = Name, ShortName = Name },
                Owner = this.owner
            };

            this.parameterOverride = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ValueSet =
                {
                    new ParameterOverrideValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
                    {
                        ValueSwitch = ParameterSwitchKind.REFERENCE, Reference = new ValueArray<string>(), Published = new ValueArray<string>()
                    },
                    new ParameterOverrideValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
                    {
                        ValueSwitch = ParameterSwitchKind.REFERENCE, Reference = new ValueArray<string>(new List<string>() {"18"}), 
                        Published = new ValueArray<string>()
                    }
                },
                Parameter = this.parameter,
                Owner = this.owner
            };

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name,
                Parameter = { this.parameter, this.parameter2 }
            };

            this.elementUsage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name,
                ElementDefinition = this.elementDefinition,
                Container = this.elementDefinition,
                ParameterOverride = { this.parameterOverride }
            };
        }

        [Test]
        public void VerifyProperties()
        {
            var vm = new ElementParameterRowControlViewModel(this.elementDefinition, this.option);
            Assert.AreEqual(2, vm.Parameters.Count);
            Assert.AreSame(vm.Element, this.elementDefinition);
            Assert.AreSame(vm.ActualOption, this.option);
            Assert.AreSame(vm.Element, this.elementDefinition);
            Assert.AreEqual(vm.Parameters.Count, this.elementDefinition.Parameter.Count);
            Assert.AreEqual(this.elementDefinition.Tooltip(), vm.ElementTooltipInfo);

            var firstParameter = vm.Parameters.FirstOrDefault();
            Assert.IsNotNull(firstParameter);
            Assert.AreSame(firstParameter.Name, Name);
            Assert.AreSame(firstParameter.ShortName, Name);
            Assert.AreSame(firstParameter.Parameter, this.parameter);
            Assert.AreEqual($"{{2, 3, 4}} [{Name}]", firstParameter.PublishedValue);
            Assert.AreEqual("-", firstParameter.Description);
            Assert.AreSame(firstParameter.OwnerShortName, Name);
            Assert.AreEqual(firstParameter.Switch, ParameterSwitchKind.REFERENCE.ToString());
            Assert.AreEqual(firstParameter.RowType, nameof(Parameter));
            Assert.AreSame(firstParameter.ActualOption, this.option);
            Assert.IsNotNull(firstParameter.ModelCode);

            var lastParameter = vm.Parameters.LastOrDefault();
            Assert.IsNotNull(lastParameter);
            Assert.AreEqual("{}", lastParameter.ActualValue);

            vm = new ElementParameterRowControlViewModel(this.elementUsage, this.option);
            var rowParameterOverride = vm.Parameters.LastOrDefault();
            Assert.IsNotNull(rowParameterOverride);
            Assert.AreEqual("{}", rowParameterOverride.PublishedValue);

            vm = new ElementParameterRowControlViewModel(this.elementDefinition, null);
            Assert.IsEmpty(vm.Parameters);
        }

        [Test]
        public void VerifyPropertiesWhenParameterIsOptionDependant()
        {
            this.parameter.IsOptionDependent = true;
            this.parameter.ValueSet.First().ActualOption = this.option;
            var vm = new ElementParameterRowControlViewModel(this.elementDefinition, this.option);
            Assert.AreEqual($"{{2, 3, 4}} [{Name}]", vm.Parameters.First().PublishedValue);

            this.parameter.ValueSet.First().ActualOption = new Option(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name
            };

            vm = new ElementParameterRowControlViewModel(this.elementDefinition, this.option);
            Assert.AreEqual($"- [{Name}]", vm.Parameters.First().ActualValue);
        }
    }
}
