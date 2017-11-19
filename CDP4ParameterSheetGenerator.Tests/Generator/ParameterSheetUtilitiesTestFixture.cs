// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterSheetUtilitiesTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.Generator
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4ParameterSheetGenerator.Generator;
    using NUnit.Framework;

    /// <summary>
    /// suite of tests for the <see cref="ParameterSheetUtilities"/> class.
    /// </summary>
    [TestFixture]
    public class ParameterSheetUtilitiesTestFixture
    {
        [Test]
        public void VerifyThatReformatDateReturnsExpectedResult()
        {
            var value = "-";
            Assert.AreEqual(value, ParameterSheetUtilities.ReformatDate(value));

            value = string.Empty;
            Assert.AreEqual(value, ParameterSheetUtilities.ReformatDate(value));

            value = "2012-12-01";
            Assert.AreEqual(value, ParameterSheetUtilities.ReformatDate(value));

            value = "2012 february 1";
            Assert.AreEqual("2012-02-01", ParameterSheetUtilities.ReformatDate(value));

            var doubleValue = "0.11111111111111111111111111111";
            Assert.AreEqual("1899-12-30", ParameterSheetUtilities.ReformatDate(doubleValue));
        }

        [Test]
        public void VerifyThatReformatTimeOfDayReturnsExpectedResult()
        {
            var value = "-";
            Assert.AreEqual(value, ParameterSheetUtilities.ReformatTimeOfDay(value));

            value = string.Empty;
            Assert.AreEqual(value, ParameterSheetUtilities.ReformatTimeOfDay(value));

            value = "24:01:59";
            Assert.AreEqual(value, ParameterSheetUtilities.ReformatTimeOfDay(value));

            var doubleValue = "0.11111111111111111111111111111";
            Assert.AreEqual("02:40:00", ParameterSheetUtilities.ReformatTimeOfDay(doubleValue));
        }

        [Test]
        public void VerifThatConvertDoubleToDateTimeObjectConvertsAsExpected()
        {
            var timeOfDayParameterType = new TimeOfDayParameterType(Guid.NewGuid(), null, null);
            DateTime expectedDate;

            var stringValue = "-";
            var stringValueAsObject = (object)stringValue;
            ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref stringValueAsObject, timeOfDayParameterType);
            Assert.AreEqual("-", stringValueAsObject);

            var timeDoubleValue = 0.11111111111111111111111111111d;
            var timeDoubleValueAsObject = (object)timeDoubleValue;
            ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref timeDoubleValueAsObject, timeOfDayParameterType);
            expectedDate = new DateTime(1, 1, 1, 2, 40, 0);
            Assert.AreEqual(expectedDate, timeDoubleValueAsObject);

            var dateParameterType = new DateParameterType(Guid.NewGuid(), null, null);
            var dateDoubleValue = 0.11111111111111111111111111111d;
            var dateDoubleValueAsObject = (object)dateDoubleValue;
            ParameterSheetUtilities.ConvertDoubleToDateTimeObject(ref dateDoubleValueAsObject, dateParameterType);
            expectedDate = new DateTime(1899, 12, 30, 2, 40, 0);
            Assert.AreEqual(expectedDate, dateDoubleValueAsObject);
        }

        [Test]
        public void VerifyThatParameterValueSetQueryParameterTypeAndScaleReturnsExpectedResult()
        {
            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);

            var parameter = new Parameter(Guid.NewGuid(), null, null)
                                {
                                    ParameterType = simpleQuantityKind,
                                    Scale = ratioScale
                                };

            var paramterValueSet = new ParameterValueSet(Guid.NewGuid(), null, null);
            parameter.ValueSet.Add(paramterValueSet);

            ParameterType parameterType;
            MeasurementScale measurementScale;
            ParameterSheetUtilities.QueryParameterTypeAndScale(paramterValueSet, 0, out parameterType, out measurementScale);

            Assert.AreEqual(simpleQuantityKind, parameterType);
            Assert.AreEqual(ratioScale, measurementScale);

            var compoundParameterType = new CompoundParameterType(Guid.NewGuid(), null, null);
            var component = new ParameterTypeComponent(Guid.NewGuid(), null, null);
            compoundParameterType.Component.Add(component);
            component.ParameterType = simpleQuantityKind;
            component.Scale = ratioScale;

            parameter.ParameterType = compoundParameterType;
            parameter.Scale = null;

            ParameterSheetUtilities.QueryParameterTypeAndScale(paramterValueSet, 0, out parameterType, out measurementScale);

            Assert.AreEqual(simpleQuantityKind, parameterType);
            Assert.AreEqual(ratioScale, measurementScale);
        }

        [Test]
        public void VerifyThatParameterOverrideValueSetQueryParameterTypeAndScaleReturnsExpectedResult()
        {
            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);

            var parameter = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = simpleQuantityKind,
                Scale = ratioScale
            };
            var paramterValueSet = new ParameterValueSet(Guid.NewGuid(), null, null);
            parameter.ValueSet.Add(paramterValueSet);

            var parameterOverride = new ParameterOverride(Guid.NewGuid(), null, null);
            parameterOverride.Parameter = parameter;
            var parameterOverrideValueSet = new ParameterOverrideValueSet(Guid.NewGuid(), null, null);
            parameterOverride.ValueSet.Add(parameterOverrideValueSet);

            ParameterType parameterType;
            MeasurementScale measurementScale;
            ParameterSheetUtilities.QueryParameterTypeAndScale(parameterOverrideValueSet, 0, out parameterType, out measurementScale);

            Assert.AreEqual(simpleQuantityKind, parameterType);
            Assert.AreEqual(ratioScale, measurementScale);

            var compoundParameterType = new CompoundParameterType(Guid.NewGuid(), null, null);
            var component = new ParameterTypeComponent(Guid.NewGuid(), null, null);
            compoundParameterType.Component.Add(component);
            component.ParameterType = simpleQuantityKind;
            component.Scale = ratioScale;

            parameter.ParameterType = compoundParameterType;
            parameter.Scale = null;

            ParameterSheetUtilities.QueryParameterTypeAndScale(parameterOverrideValueSet, 0, out parameterType, out measurementScale);

            Assert.AreEqual(simpleQuantityKind, parameterType);
            Assert.AreEqual(ratioScale, measurementScale);
        }

        [Test]
        public void VerifyThatParameterSuscriptionValueSetQueryParameterTypeAndScaleReturnsExpectedResult()
        {
            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null);
            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);

            var parameter = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = simpleQuantityKind,
                Scale = ratioScale
            };

            var parameterSubscription = new ParameterSubscription(Guid.NewGuid(), null, null);

            parameter.ParameterSubscription.Add(parameterSubscription);
            
            var parameterSubscriptionValueSet = new ParameterSubscriptionValueSet(Guid.NewGuid(), null, null);
            parameterSubscription.ValueSet.Add(parameterSubscriptionValueSet);

            ParameterType parameterType;
            MeasurementScale measurementScale;
            ParameterSheetUtilities.QueryParameterTypeAndScale(parameterSubscriptionValueSet, 0, out parameterType, out measurementScale);

            Assert.AreEqual(simpleQuantityKind, parameterType);
            Assert.AreEqual(ratioScale, measurementScale);

            var compoundParameterType = new CompoundParameterType(Guid.NewGuid(), null, null);
            var component = new ParameterTypeComponent(Guid.NewGuid(), null, null);
            compoundParameterType.Component.Add(component);
            component.ParameterType = simpleQuantityKind;
            component.Scale = ratioScale;

            parameter.ParameterType = compoundParameterType;
            parameter.Scale = null;

            ParameterSheetUtilities.QueryParameterTypeAndScale(parameterSubscriptionValueSet, 0, out parameterType, out measurementScale);

            Assert.AreEqual(simpleQuantityKind, parameterType);
            Assert.AreEqual(ratioScale, measurementScale);
        }

        [Test]
        public void VerifyThatConvertNumericValueToStringReturnsExpectedResult()
        {
            object number = 1;
            ParameterSheetUtilities.ConvertObjectToString(ref number);
            Assert.AreEqual("1", number);
        }
    }
}
