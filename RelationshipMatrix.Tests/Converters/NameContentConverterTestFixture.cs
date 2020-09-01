// -------------------------------------------------------------------------------------------------
// <copyright file="NameContentConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2018-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.Converters
{
    using System;
    using System.Dynamic;
    using System.Windows;
    using CDP4RelationshipMatrix.Converters;
    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.Native;
    using Moq;
    using NUnit.Framework;
    using ViewModels;

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
        }
    }
}