// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterArrayAssemblerTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4ParameterSheetGenerator.Tests.ArrayAssemblers.ParameterSheet
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.ViewModels;

    using CDP4ParameterSheetGenerator.ParameterSheet;
    using CDP4ParameterSheetGenerator.RowModels;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterArrayAssembler"/> class
    /// </summary>
    [TestFixture]
    public class ParameterArrayAssemblerTestFixture
    {
        private List<IExcelRow<Thing>> excelRows;

        /// <summary>
        /// The <see cref="Iteration"/> that is being assembled
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> for which the <see cref="Iteration"/> is being assembled
        /// </summary>
        private DomainOfExpertise owner;

        [SetUp]
        public void SetUp()
        {
            this.excelRows = new List<IExcelRow<Thing>>();

            this.owner = new DomainOfExpertise(Guid.NewGuid(), null, null) { Name = "system", ShortName = "SYS" };

            // Reference SitedirectoryData
            var lengthunit = new SimpleUnit(Guid.NewGuid(), null, null);
            lengthunit.ShortName = "m";
            lengthunit.Name = "metre";

            var lengthscale = new RatioScale(Guid.NewGuid(), null, null);
            lengthscale.Unit = lengthunit;
            lengthscale.ShortName = "m-scale";
            lengthscale.Name = "metre scale";

            var xcoord = new SimpleQuantityKind(Guid.NewGuid(), null, null) { Name = "x", ShortName = "x" };
            xcoord.PossibleScale.Add(lengthscale);
            xcoord.DefaultScale = lengthscale;

            var ycoord = new SimpleQuantityKind(Guid.NewGuid(), null, null) { Name = "y", ShortName = "y" };
            ycoord.PossibleScale.Add(lengthscale);
            ycoord.DefaultScale = lengthscale;

            var zcoord = new SimpleQuantityKind(Guid.NewGuid(), null, null) { Name = "z", ShortName = "z" };
            zcoord.PossibleScale.Add(lengthscale);
            zcoord.DefaultScale = lengthscale;

            var vector = new ArrayParameterType(Guid.NewGuid(), null, null);
            vector.Name = "coordinate";
            vector.ShortName = "coord";
            var xcomp = new ParameterTypeComponent(Guid.NewGuid(), null, null) { ParameterType = xcoord };
            var ycomp = new ParameterTypeComponent(Guid.NewGuid(), null, null) { ParameterType = ycoord };
            var zcomp = new ParameterTypeComponent(Guid.NewGuid(), null, null) { ParameterType = zcoord };
            vector.Component.Add(xcomp);
            vector.Component.Add(ycomp);
            vector.Component.Add(zcomp);

            // iteration data
            this.iteration = new Iteration(Guid.NewGuid(), null, null);

            var optionA = new Option(Guid.NewGuid(), null, null) { Name = "Option A", ShortName = "OptionA" };
            this.iteration.Option.Add(optionA);
            var optionB = new Option(Guid.NewGuid(), null, null) { Name = "Option B", ShortName = "OptionB" };
            this.iteration.Option.Add(optionB);

            var possibleFiniteStateList = new PossibleFiniteStateList(Guid.NewGuid(), null, null);
            var possibleFiniteState1 = new PossibleFiniteState(Guid.NewGuid(), null, null)
            {
                ShortName = "state1",
                Name = "state 1"
            };
            possibleFiniteStateList.PossibleState.Add(possibleFiniteState1);
            var possibleFiniteState2 = new PossibleFiniteState(Guid.NewGuid(), null, null)
            {
                ShortName = "state2",
                Name = "state 2"
            };
            possibleFiniteStateList.PossibleState.Add(possibleFiniteState2);
            possibleFiniteStateList.DefaultState = possibleFiniteState1;

            var actualFiniteStateList = new ActualFiniteStateList(Guid.NewGuid(), null, null);
            actualFiniteStateList.PossibleFiniteStateList.Add(possibleFiniteStateList);
            var actualFiniteState1 = new ActualFiniteState(Guid.NewGuid(), null, null);
            actualFiniteState1.PossibleState.Add(possibleFiniteState1);
            actualFiniteStateList.ActualState.Add(actualFiniteState1);
            var actualFiniteState2 = new ActualFiniteState(Guid.NewGuid(), null, null);
            actualFiniteState2.PossibleState.Add(possibleFiniteState2);
            actualFiniteStateList.ActualState.Add(actualFiniteState2);

            var elementDefinitionA = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                Owner = this.owner,
                ShortName = "elementdefinitionA",
                Name = "element definition A"
            };

            this.iteration.Element.Add(elementDefinitionA);
            var parameterA1 = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = xcoord,
                Scale = lengthscale,
                Owner = this.owner
            };
            var parameterValueSetA1 = new ParameterValueSet(Guid.NewGuid(), null, null);
            var valueArrayA = new ValueArray<string>(new List<string>() { "x" });
            parameterValueSetA1.ValueSwitch = ParameterSwitchKind.MANUAL;
            parameterValueSetA1.Manual = valueArrayA;
            parameterValueSetA1.Computed = valueArrayA;
            parameterValueSetA1.Reference = valueArrayA;
            parameterValueSetA1.Formula = valueArrayA;
            parameterA1.ValueSet.Add(parameterValueSetA1);            
            elementDefinitionA.Parameter.Add(parameterA1);

            var parameterA2 = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = vector,
                Owner = this.owner
            };
            elementDefinitionA.Parameter.Add(parameterA2);
            var parameterValueSetA2 = new ParameterValueSet(Guid.NewGuid(), null, null);
            parameterA2.ValueSet.Add(parameterValueSetA2);
            var valueArrayA2 = new ValueArray<string>(new List<string>() { "x", "y", "z" });
            parameterValueSetA2.ValueSwitch = ParameterSwitchKind.MANUAL;
            parameterValueSetA2.Manual = valueArrayA2;
            parameterValueSetA2.Computed = valueArrayA2;
            parameterValueSetA2.Reference = valueArrayA2;
            parameterValueSetA2.Formula = valueArrayA2;
            
            var parameterA3 = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = xcoord,
                Scale = lengthscale,
                Owner = this.owner,
                IsOptionDependent = true,
            };
            elementDefinitionA.Parameter.Add(parameterA3);
            var parameterValueSetA3OptionA = new ParameterValueSet(Guid.NewGuid(), null, null) { ActualOption = optionA };
            var valueArrayA3OptionA = new ValueArray<string>(new List<string> { "x" });
            parameterValueSetA3OptionA.Manual = valueArrayA3OptionA;
            parameterValueSetA3OptionA.Reference = valueArrayA3OptionA;
            parameterValueSetA3OptionA.Computed = valueArrayA3OptionA;
            parameterValueSetA3OptionA.Formula = valueArrayA3OptionA;
            parameterA3.ValueSet.Add(parameterValueSetA3OptionA);
            var parameterValueSetA3OptionB = new ParameterValueSet(Guid.NewGuid(), null, null) { ActualOption = optionB };
            var valueArrayA3OptionB = new ValueArray<string>(new List<string>() { "x" });
            parameterValueSetA3OptionB.Manual = valueArrayA3OptionB;
            parameterValueSetA3OptionB.Reference = valueArrayA3OptionB;
            parameterValueSetA3OptionB.Computed = valueArrayA3OptionB;
            parameterValueSetA3OptionB.Formula = valueArrayA3OptionB;
            parameterA3.ValueSet.Add(parameterValueSetA3OptionB);
            
            var processedValueSets = new Dictionary<Guid, ProcessedValueSet>();

            var assembler = new ParameterSheetRowAssembler(this.iteration, this.owner);
            assembler.Assemble(processedValueSets);
            this.excelRows.AddRange(assembler.ExcelRows);
        }

        [Test]
        public void VerifyThatAssemblerPopulatesArrays()
        {
            var arrayAssembler = new ParameterArrayAssembler(this.excelRows);

            var parameterArray = arrayAssembler.ContentArray;

            // the array contains 2 more rows to make a nice header and spacing
            Assert.AreEqual(this.excelRows.Count, parameterArray.GetLength(0) - 2);
        }
    }
}
