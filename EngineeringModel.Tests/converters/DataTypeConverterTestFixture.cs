// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataTypeConverterTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.converters
{
    using System;

    using CDP4Common.EngineeringModelData;

    using CDP4EngineeringModel.Converters;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="DataTypeConverter"/>
    /// </summary>
    [TestFixture]
    public class DataTypeConverterTestFixture
    {
        private DataTypeConverter dataTypeConverter;

        [SetUp]
        public void SetUp()
        {
            this.dataTypeConverter = new DataTypeConverter();
        }

        [Test]
        public void VerifyThatConverReturnsExpectedResuly()
        {
            object result;

            result = this.dataTypeConverter.Convert(null, null, null, null);
            Assert.IsNull(result);

            var parameterSubscription = new ParameterSubscription(Guid.NewGuid(), null, null);

            result = this.dataTypeConverter.Convert(parameterSubscription, null, null, null);

            Assert.AreEqual(typeof(ParameterSubscription), result);
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.dataTypeConverter.ConvertBack(null, null, null, null));
        }
    }
}
