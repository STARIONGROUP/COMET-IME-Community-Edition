﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementParameterRowControlViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace CDP4Composition.Tests.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.ViewModels;

    using CDP4Dal;

    using DevExpress.Mvvm.Native;

    using NUnit.Framework;

    [TestFixture]
    public class ElementParameterRowControlViewModelTestFixture
    {
        private ElementDefinition elementDefinition;
        private ElementUsage elementUsage;
        private Option option;
        private Parameter parameter;
        private ParameterOverride parameterOverride;
        private DomainOfExpertise owner;
        private Assembler assembler;
        private const string Name = "test";
        private Uri dalUri;

        [SetUp]
        public void Setup()
        {
            this.dalUri = new Uri("http://test");

            this.assembler = new Assembler(this.dalUri);

            this.owner = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { ShortName = Name };

            this.parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ValueSet = { new ParameterValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { ValueSwitch = ParameterSwitchKind.REFERENCE, Reference = new ValueArray<string>(), Published = new ValueArray<string>() }},
                ParameterType = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { Name = Name, ShortName = Name},
                Scale = new RatioScale(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { ShortName = Name },
                Owner = this.owner
            };

            this.parameterOverride = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                ValueSet = { new ParameterOverrideValueSet(Guid.NewGuid(), this.assembler.Cache, this.dalUri) { ValueSwitch = ParameterSwitchKind.REFERENCE, Reference = new ValueArray<string>(), Published = new ValueArray<string>() } },
                Parameter = this.parameter,
                Owner = this.owner
            };

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name,
                Parameter = { this.parameter }
            };

            this.elementUsage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name,
                ElementDefinition = this.elementDefinition,
                ParameterOverride = { this.parameterOverride }
            };

            this.option = new Option(Guid.NewGuid(), this.assembler.Cache, this.dalUri)
            {
                Name = Name
            };
        }

        [Test]
        public void VerifyProperties()
        {
            var vm = new ElementParameterRowControlViewModel(this.elementDefinition, this.option);
            Assert.AreEqual(1, vm.Parameters.Count);
            Assert.AreSame(vm.Element, this.elementDefinition);
            Assert.AreSame(vm.ActualOption, this.option);
            Assert.AreSame(vm.Element, this.elementDefinition);
            Assert.IsNotEmpty(vm.ModelCode);
            Assert.AreEqual(vm.Category, "-");
            Assert.AreEqual(vm.Parameters.Count, this.elementDefinition.Parameter.Count);
            Assert.AreEqual(vm.Definition, ": -");
            Assert.AreSame(vm.Owner, "NA");

            var firstParameter = vm.Parameters.FirstOrDefault();
            Assert.IsNotNull(firstParameter);
            Assert.AreSame(firstParameter.Name, Name);
            Assert.AreSame(firstParameter.ShortName, Name);
            Assert.AreSame(firstParameter.Parameter, this.parameter);
            Assert.AreEqual(firstParameter.Value, " [test]");
            Assert.AreEqual(firstParameter.Description, "-");
            Assert.AreSame(firstParameter.OwnerShortName, Name);
            Assert.AreEqual(firstParameter.Switch, ParameterSwitchKind.REFERENCE.ToString());
            Assert.AreEqual(firstParameter.RowType, nameof(Parameter));
            Assert.AreSame(firstParameter.ActualOption, this.option);
            Assert.IsNotNull(firstParameter.ModelCode);
            
            vm = new ElementParameterRowControlViewModel(this.elementDefinition, null);
            Assert.IsEmpty(vm.Parameters);
        }
    }
}