// -------------------------------------------------------------------------------------------------
// <copyright file="ColumnWidthConverterTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2018-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.Converters
{
    using System;
    using CDP4RelationshipMatrix.Converters;
    using DevExpress.Xpf.Grid;
    using NUnit.Framework;
    using ViewModels;

    /// <summary>
    /// Suite of tests for the <see cref="ColumnWidthConverter"/>
    /// </summary>
    [TestFixture]
    public class ColumnWidthConverterTestFixture
    {
        private ColumnWidthConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new ColumnWidthConverter();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void AssertThatConverterWorks()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));

            Assert.AreEqual(35, this.converter.Convert(null, null, null, null));
            Assert.AreEqual(35, this.converter.Convert("rowname", null, null, null));
            Assert.AreEqual(100, this.converter.Convert(MatrixViewModel.ROW_NAME_COLUMN, null, null, null));
        }
    }
}