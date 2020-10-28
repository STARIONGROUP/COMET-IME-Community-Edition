// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessedValueSetTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Common.Validation;

    using CDP4Composition.ViewModels;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ProcessedValueSet"/> class
    /// </summary>
    [TestFixture]
    public class ProcessedValueSetTestFixture
    {
        private readonly Uri uri = new Uri("http://www.rheagroup.com");

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> concurentDictionary;

        private ParameterType parameterType;

        private ParameterValueSet parameterValueSet;

        private ParameterOverrideValueSet parameterOverrideValueSet;

        private ParameterSubscriptionValueSet parameterSubscriptionValueSet;

        private ValueSetValues valueSetValues;

        [SetUp]
        public void SetUp()
        {
            this.concurentDictionary = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.parameterType = new TextParameterType(Guid.NewGuid(), this.concurentDictionary, this.uri);

            var manualValue = new ValueArray<string>(new List<string> { "-" });
            var computedValue = new ValueArray<string>(new List<string> { "-" });
            var referenceValue = new ValueArray<string>(new List<string> { "-" });
            var formula = new ValueArray<string>(new List<string> { "-" });

            this.parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.concurentDictionary, this.uri)
                                         {
                                             Manual = manualValue,
                                             Computed = computedValue,
                                             Reference = referenceValue,
                                             Formula = formula,
                                             ValueSwitch = ParameterSwitchKind.MANUAL
                                         };


            this.parameterOverrideValueSet = new ParameterOverrideValueSet(Guid.NewGuid(), this.concurentDictionary, this.uri)
                                        {
                                             Manual = manualValue,
                                             Computed = computedValue,
                                             Reference = referenceValue,
                                             Formula = formula,
                                             ValueSwitch = ParameterSwitchKind.MANUAL
                                         };

            this.parameterSubscriptionValueSet = new ParameterSubscriptionValueSet(Guid.NewGuid(), this.concurentDictionary, this.uri)
                                        {
                                            Manual = manualValue,
                                             ValueSwitch = ParameterSwitchKind.MANUAL
                                        };

            this.valueSetValues = new ValueSetValues(0, this.parameterType, ParameterSwitchKind.COMPUTED, "manual", "computed", "reference", "formula");
        }

        [Test]
        public void VerifyThatProcessedValueSetConstructorThrowsExceptionWhenNotValueSet()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), this.concurentDictionary, this.uri);

            Assert.Throws<ArgumentException>(() => new ProcessedValueSet(elementDefinition, ValidationResultKind.Valid));
        }

        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            var processedValueSet = new ProcessedValueSet(this.parameterValueSet, ValidationResultKind.Valid);

            Assert.AreEqual(this.parameterValueSet, processedValueSet.OriginalThing);
            Assert.AreEqual(ValidationResultKind.Valid, processedValueSet.ValidationResult);
            Assert.IsNull(processedValueSet.ClonedThing);
        }

        [Test]
        public void VerifyThatIsDirtyReturnsExpectedResultForParameterValueSet()
        {
            ValueSetValues valueSetValues;
            var processedValueSet = new ProcessedValueSet(this.parameterValueSet, ValidationResultKind.Valid);

            var isDirty = processedValueSet.IsDirty(0, this.parameterType, ParameterSwitchKind.COMPUTED, "1","1","1","1", out valueSetValues);
            Assert.IsTrue(isDirty);

            isDirty = processedValueSet.IsDirty(0, this.parameterType, ParameterSwitchKind.MANUAL, "-", "-", "-", "-", out valueSetValues);
            Assert.IsFalse(isDirty);
        }

        [Test]
        public void VerifyThatIsDirtyReturnsExpectedResultForParameterOverrideValueSet()
        {
            ValueSetValues valueSetValues;
            var processedValueSet = new ProcessedValueSet(this.parameterOverrideValueSet, ValidationResultKind.Valid);

            var isDirty = processedValueSet.IsDirty(0, this.parameterType, ParameterSwitchKind.COMPUTED, "1", "1", "1", "1", out valueSetValues);
            Assert.IsTrue(isDirty);

            isDirty = processedValueSet.IsDirty(0, this.parameterType, ParameterSwitchKind.MANUAL, "-", "-", "-", "-", out valueSetValues);
            Assert.IsFalse(isDirty);
        }

        [Test]
        public void VerifyThatIsDirtyReturnsExpectedResultForParameterSubscriptionValueSet()
        {
            ValueSetValues valueSetValues;
            var processedValueSet = new ProcessedValueSet(this.parameterSubscriptionValueSet, ValidationResultKind.Valid);

            var isDirty = processedValueSet.IsDirty(0, this.parameterType, ParameterSwitchKind.COMPUTED, "1", "1", "1", "1", out valueSetValues);
            Assert.IsTrue(isDirty);

            isDirty = processedValueSet.IsDirty(0, this.parameterType, ParameterSwitchKind.MANUAL, "-", "-", "-", "-", out valueSetValues);
            Assert.IsFalse(isDirty);
        }

        [Test]
        public void VerifyUpdateParameterValueset()
        {
            var processedValueSet = new ProcessedValueSet(this.parameterValueSet, ValidationResultKind.Valid);

            processedValueSet.UpdateClone(this.valueSetValues);

            Assert.IsNotNull(processedValueSet.ClonedThing);

            Assert.IsTrue(processedValueSet.IsManualValueDirty(0));
            Assert.IsTrue(processedValueSet.IsComputedValueDirty(0));
            Assert.IsTrue(processedValueSet.IsReferenceValueDirty(0));
            Assert.IsTrue(processedValueSet.IsFormulaValueDirty(0));
            Assert.IsTrue(processedValueSet.IsValueSwitchDirty());


            this.valueSetValues = new ValueSetValues(0, this.parameterType, ParameterSwitchKind.MANUAL, "-", "-", "-", "-");
            processedValueSet.UpdateClone(this.valueSetValues);
            Assert.IsFalse(processedValueSet.IsManualValueDirty(0));
            Assert.IsFalse(processedValueSet.IsComputedValueDirty(0));
            Assert.IsFalse(processedValueSet.IsReferenceValueDirty(0));
            Assert.IsFalse(processedValueSet.IsFormulaValueDirty(0));
            Assert.IsFalse(processedValueSet.IsValueSwitchDirty());
        }

        [Test]
        public void VerifyUpdateParameterOverrideValueset()
        {
            var processedValueSet = new ProcessedValueSet(this.parameterOverrideValueSet, ValidationResultKind.Valid);

            processedValueSet.UpdateClone(this.valueSetValues);

            Assert.IsNotNull(processedValueSet.ClonedThing);

            Assert.IsTrue(processedValueSet.IsManualValueDirty(0));
            Assert.IsTrue(processedValueSet.IsComputedValueDirty(0));
            Assert.IsTrue(processedValueSet.IsReferenceValueDirty(0));
            Assert.IsTrue(processedValueSet.IsFormulaValueDirty(0));
            Assert.IsTrue(processedValueSet.IsValueSwitchDirty());

            this.valueSetValues = new ValueSetValues(0, this.parameterType, ParameterSwitchKind.MANUAL, "-", "-", "-", "-");
            processedValueSet.UpdateClone(this.valueSetValues);
            Assert.IsFalse(processedValueSet.IsManualValueDirty(0));
            Assert.IsFalse(processedValueSet.IsComputedValueDirty(0));
            Assert.IsFalse(processedValueSet.IsReferenceValueDirty(0));
            Assert.IsFalse(processedValueSet.IsFormulaValueDirty(0));
            Assert.IsFalse(processedValueSet.IsValueSwitchDirty());
        }

        [Test]
        public void VerifyUpdateParameterSubscriptionValueset()
        {
            var processedValueSet = new ProcessedValueSet(this.parameterSubscriptionValueSet, ValidationResultKind.Valid);

            processedValueSet.UpdateClone(this.valueSetValues);

            Assert.IsNotNull(processedValueSet.ClonedThing);

            Assert.IsTrue(processedValueSet.IsManualValueDirty(0));
            Assert.IsFalse(processedValueSet.IsComputedValueDirty(0));
            Assert.IsFalse(processedValueSet.IsReferenceValueDirty(0));
            Assert.IsFalse(processedValueSet.IsFormulaValueDirty(0));
            Assert.IsTrue(processedValueSet.IsValueSwitchDirty());

            this.valueSetValues = new ValueSetValues(0, this.parameterType, ParameterSwitchKind.MANUAL, "-", "-", "-", "-");
            processedValueSet.UpdateClone(this.valueSetValues);
            Assert.IsFalse(processedValueSet.IsManualValueDirty(0));
            Assert.IsFalse(processedValueSet.IsComputedValueDirty(0));
            Assert.IsFalse(processedValueSet.IsReferenceValueDirty(0));
            Assert.IsFalse(processedValueSet.IsFormulaValueDirty(0));
            Assert.IsFalse(processedValueSet.IsValueSwitchDirty());
        }

        [Test]
        public void VerifyThatIsDirtyMethodsThrowExceptionWhenUpdateCloneHasNotBeenCalled()
        {
            ProcessedValueSet processedValueSet;

            processedValueSet = new ProcessedValueSet(this.parameterValueSet, ValidationResultKind.Valid);
            Assert.Throws<InvalidOperationException>(() => processedValueSet.IsManualValueDirty(0));
            Assert.Throws<InvalidOperationException>(() => processedValueSet.IsComputedValueDirty(0));
            Assert.Throws<InvalidOperationException>(() => processedValueSet.IsReferenceValueDirty(0));
            Assert.Throws<InvalidOperationException>(() => processedValueSet.IsFormulaValueDirty(0));
            Assert.Throws<InvalidOperationException>(() => processedValueSet.IsValueSwitchDirty());
        }
    }
}
