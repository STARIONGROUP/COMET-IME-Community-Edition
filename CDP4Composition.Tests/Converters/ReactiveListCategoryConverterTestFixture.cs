// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReactiveListCategoryConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System.Collections;
    using System.Collections.Generic;

    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Converters;
    using CDP4Composition.Mvvm;

    using NUnit.Framework;

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