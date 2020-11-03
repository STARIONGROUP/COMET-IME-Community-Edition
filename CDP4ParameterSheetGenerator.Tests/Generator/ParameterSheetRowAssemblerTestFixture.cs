// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetRowAssemblerTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4ParameterSheetGenerator.Tests.Generator
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.ViewModels;

    using CDP4ParameterSheetGenerator.ParameterSheet;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterSheetRowAssembler"/>
    /// </summary>
    [TestFixture]
    public class ParameterSheetRowAssemblerTestFixture
    {
        /// <summary>
        /// The <see cref="Iteration"/> that is being assembled
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// The <see cref="DomainOfExpertise"/> for which the <see cref="Iteration"/> is being assembled
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Another <see cref="DomainOfExpertise"/> that may own items in the <see cref="Iteration"/> that should be skipped in by the assembler
        /// </summary>
        private DomainOfExpertise otherOwner;

        [SetUp]
        public void SetUp()
        {
            this.owner = new DomainOfExpertise(Guid.NewGuid(), null, null);
            this.owner.ShortName = "SYS";
            this.owner.Name = "System";
            this.otherOwner = new DomainOfExpertise(Guid.NewGuid(), null, null);
            this.otherOwner.ShortName = "THR";
            this.otherOwner.Name = "Thermal";

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
            var valueArrayA2 = new ValueArray<string>(new List<string>() { "x", "y", "z"});
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
            var valueArrayA3OptionA = new ValueArray<string>(new List<string>() { "x" });
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

            var elementDefinitionB = new ElementDefinition(Guid.NewGuid(), null, null)
                                         {
                                             Owner = this.otherOwner,
                                             ShortName = "elementdefinitionB",
                                             Name = "element definition B"
                                         };

            this.iteration.Element.Add(elementDefinitionB);
            var parameterB1 = new Parameter(Guid.NewGuid(), null, null) { ParameterType = xcoord, Owner = this.owner };
            elementDefinitionB.Parameter.Add(parameterB1);

            // element usage
            var elementUsageOfA1 = new ElementUsage(Guid.NewGuid(), null, null) { ShortName = "usageofA1", Name = "usage of A 2", Owner = this.owner};
            elementDefinitionB.ContainedElement.Add(elementUsageOfA1);
            elementUsageOfA1.ElementDefinition = elementDefinitionA;

            var elementUsageOfA2 = new ElementUsage(Guid.NewGuid(), null, null) { ShortName = "usageofA2", Name = "usage of A 2", Owner = this.owner };
            elementDefinitionB.ContainedElement.Add(elementUsageOfA2);
            elementUsageOfA2.ElementDefinition = elementDefinitionA;

            var parameterOverrideA1 = new ParameterOverride()
                                          {
                                              Parameter = parameterA1,
                                              Container = elementUsageOfA2,
                                              Owner = this.owner
                                          };
            parameterOverrideA1.Iid = Guid.NewGuid();

            var parameterOverrideValueSetA1 = new ParameterOverrideValueSet();
            parameterOverrideValueSetA1.Iid = Guid.NewGuid();
            var overrideValueArrayA = new ValueArray<string>(new List<string>() { "x" });
            parameterOverrideValueSetA1.Manual = overrideValueArrayA;
            parameterOverrideValueSetA1.Computed = overrideValueArrayA;
            parameterOverrideValueSetA1.Reference = overrideValueArrayA;
            parameterOverrideValueSetA1.Formula = overrideValueArrayA;
            parameterOverrideA1.ValueSet.Add(parameterOverrideValueSetA1);
            elementUsageOfA2.ParameterOverride.Add(parameterOverrideA1);

            var parameterOverrideA2 = new ParameterOverride()
            {
                Parameter = parameterA2,
                Container = elementUsageOfA2,
                Owner = this.owner
            };
            parameterOverrideA2.Iid = Guid.NewGuid();

            var parameterOverrideValueSetA2 = new ParameterOverrideValueSet();
            parameterOverrideValueSetA2.Iid = Guid.NewGuid();
            var overrideValueArrayA2 = new ValueArray<string>(new List<string>() { "x", "y", "z" });
            parameterOverrideValueSetA2.Manual = overrideValueArrayA2;
            parameterOverrideValueSetA2.Computed = overrideValueArrayA2;
            parameterOverrideValueSetA2.Reference = overrideValueArrayA2;
            parameterOverrideValueSetA2.Formula = overrideValueArrayA2;
            parameterOverrideA2.ValueSet.Add(parameterOverrideValueSetA2);
            elementUsageOfA2.ParameterOverride.Add(parameterOverrideA2);

            var parameterOverrideA3 = new ParameterOverride()
            {
                Parameter = parameterA3,
                Container = elementUsageOfA2,
                Owner = this.owner
            };
            parameterOverrideA3.Iid = Guid.NewGuid();

            var parameterOverrideValueSetA3 = new ParameterOverrideValueSet();
            parameterOverrideValueSetA3.Iid = Guid.NewGuid();

            var parameterOverrideValueSetA3OptionA = new ParameterOverrideValueSet();
            parameterOverrideValueSetA3OptionA.Iid = Guid.NewGuid();
            parameterOverrideValueSetA3OptionA.ParameterValueSet = parameterValueSetA3OptionA;
            var overrideValueArrayA3OptionA = new ValueArray<string>(new List<string>() { "x" });
            parameterOverrideValueSetA3OptionA.Manual = overrideValueArrayA3OptionA;
            parameterOverrideValueSetA3OptionA.Reference = overrideValueArrayA3OptionA;
            parameterOverrideValueSetA3OptionA.Computed = overrideValueArrayA3OptionA;
            parameterOverrideValueSetA3OptionA.Formula = overrideValueArrayA3OptionA;
            parameterOverrideA3.ValueSet.Add(parameterOverrideValueSetA3OptionA);
            var parameterOverrideValueSetA3OptionB = new ParameterOverrideValueSet();
            parameterOverrideValueSetA3OptionB.Iid = Guid.NewGuid();
            parameterOverrideValueSetA3OptionB.ParameterValueSet = parameterValueSetA3OptionB;
            var overrideValueArrayA3OptionB = new ValueArray<string>(new List<string>() { "x" });
            parameterOverrideValueSetA3OptionB.Manual = overrideValueArrayA3OptionB;
            parameterOverrideValueSetA3OptionB.Reference = overrideValueArrayA3OptionB;
            parameterOverrideValueSetA3OptionB.Computed = overrideValueArrayA3OptionB;
            parameterOverrideValueSetA3OptionB.Formula = overrideValueArrayA3OptionB;
            parameterOverrideA3.ValueSet.Add(parameterOverrideValueSetA3OptionB);
            elementUsageOfA2.ParameterOverride.Add(parameterOverrideA3);
        }

        [Test]        
        public void VerifyThatNullIterationThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new ParameterSheetRowAssembler(null, null));

            Assert.Throws<ArgumentNullException>(() => new ParameterSheetRowAssembler(this.iteration, null));
        }

        [Test]
        public void VerifyThatExpectedParameterSheetRowsAreAssembled()
        {
            var assembler = new ParameterSheetRowAssembler(this.iteration, this.owner);
            CollectionAssert.IsEmpty(assembler.ExcelRows);

            var processedValueSets = new Dictionary<Guid, ProcessedValueSet>();
            
            assembler.Assemble(processedValueSets);

            CollectionAssert.IsNotEmpty(assembler.ExcelRows);

            foreach (var excelRow in assembler.ExcelRows)
            {
                System.Console.WriteLine(string.Format("{0} - {1} - {2}", excelRow.Type, excelRow.Name, excelRow.Thing.Iid));
            }
        }
    }
}
