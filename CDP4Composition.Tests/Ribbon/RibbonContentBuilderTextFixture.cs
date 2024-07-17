// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonContentBuilderTextFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Tests.Ribbon
{
    using System.Threading;

    using CDP4Composition.Ribbon;
    using DevExpress.Xpf.Ribbon;
    using NUnit.Framework;

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RibbonContentBuilderTextFixture
    {
        [Test]
        public void VerifyThatContentIsCorrectlyBuilt()
        {
            var categories = new[]
            {
                new ExtendedRibbonPageCategory(),
                new ExtendedRibbonPageCategory() 
                { 
                    Name = "CustomCategory" 
                },
            };

            var pages = new[]
            {
                new ExtendedRibbonPage(),
                new ExtendedRibbonPage() 
                { 
                    IsInDefaultPageCategory = false, 
                    CustomPageCategoryName = "CustomCategory" 
                },
                new ExtendedRibbonPage(),
            };

            var groups = new[]
            {
                new ExtendedRibbonPageGroup(),
                new ExtendedRibbonPageGroup(),
                new ExtendedRibbonPageGroup(),
            };

            var builder = new RibbonContentBuilder(categories, groups, pages);

            var ribbon = new RibbonControl();
            ribbon.Categories.Add(new RibbonDefaultPageCategory());

            builder.BuildAndAppendToRibbon(ribbon);

            Assert.That(ribbon.ActualCategories.Count, Is.EqualTo(3));
            Assert.That(ribbon.ActualCategories[1].Pages.Count, Is.EqualTo(1));
            Assert.That(ribbon.ActualCategories[2].Pages.Count, Is.EqualTo(2));
            Assert.That(ribbon.ActualCategories[2].Pages[0].ActualGroups.Count, Is.EqualTo(1));
        }
    }
}
