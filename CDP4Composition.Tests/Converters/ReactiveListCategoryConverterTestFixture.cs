// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReactiveListCategoryConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System.Collections;
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ReactiveCategoryToObjectListConverter"/>
    /// </summary>
    [TestFixture]
    internal class ReactiveListCategoryConverterTestFixture
    {
        [Test]
        public void VerifyThatConvertWorksWithValue()
        {
            var converter = new ReactiveCategoryToObjectListConverter();
            var value = converter.Convert(new ReactiveList<Category> { new Category{ShortName = "1"} }, null, null, null) as IList;

            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Count);
        }

        [Test]
        public void VerifyThatConvertWorksWithNullValue()
        {
            var converter = new ReactiveCategoryToObjectListConverter();
            var value = converter.Convert(null, null, null, null) as IList;

            Assert.IsNotNull(value);
            Assert.AreEqual(0, value.Count);
        }

        [Test]
        public void VerifyThatConvertBackWorksWithValue()
        {
            var converter = new ReactiveCategoryToObjectListConverter();
            var value = converter.ConvertBack(new List<Category> { new Category { ShortName = "1" } }, null, null, null) as ReactiveList<Category>;

            Assert.AreEqual(1, value.Count);
        }

        [Test]
        public void VerifyThatConvertBackWorksWithNullValue()
        {
            var converter = new ReactiveCategoryToObjectListConverter();
            var value = converter.ConvertBack(null, null, null, null) as ReactiveList<Category>;

            Assert.AreEqual(0, value.Count);
        }
    }
}