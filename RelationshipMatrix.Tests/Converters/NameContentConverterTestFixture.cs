// -------------------------------------------------------------------------------------------------
// <copyright file="NameContentConverterTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2018-2020 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.Converters
{
    using System;

    using CDP4RelationshipMatrix.Converters;

    using DevExpress.Xpf.Grid;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="NameContentConverter"/>
    /// </summary>
    [TestFixture]
    public class NameContentConverterTestFixture
    {
        private NameContentConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new NameContentConverter();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void AssertThatConverterWorks()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
            Assert.IsNull(this.converter.Convert(It.IsAny<object[]>(), null, null, null));
            Assert.IsNull(this.converter.Convert(new[] { It.IsAny<RowData>() }, null, null, null));
        }
    }
}