// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessedValueSetTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.Generator
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Common.Validation;

    using CDP4ParameterSheetGenerator.Generator.ParameterSheet;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ProcessedValueSet"/> class
    /// </summary>
    [TestFixture]
    public class ProcessedValueSetTestFixture
    {
        private readonly Uri uri = new Uri("http://www.rheagroup.com");

        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> concurentDictionary;

        private ParameterType parameterType;

        private ParameterValueSet parameterValueSet;

        private ParameterOverrideValueSet parameterOverrideValueSet;

        private ParameterSubscriptionValueSet parameterSubscriptionValueSet;

        private ValueSetValues valueSetValues;

        [SetUp]
        public void SetUp()
        {
            this.concurentDictionary = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

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
