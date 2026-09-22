// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVBrowserViewTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Views
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media;

    using CDP4Requirements.Views;

    using DevExpress.Xpf.Bars;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="VandVBrowser"/> view. Loading the view headless catches XAML errors that only
    /// surface at runtime, and asserts that the standard browser icons and the VCD-specific controls live in a single
    /// toolbar (gh1498).
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class VandVBrowserViewTestFixture
    {
        [Test]
        public void VerifyThatTheViewLoadsAndMeasures()
        {
            VandVBrowser view = null;

            Assert.DoesNotThrow(() => view = new VandVBrowser(true));

            Assert.DoesNotThrow(() =>
            {
                view.Measure(new Size(800, 600));
                view.Arrange(new Rect(0, 0, 800, 600));
            });
        }

        [Test]
        public void VerifyThatTheStandardAndVcdControlsShareOneToolbar()
        {
            var view = new VandVBrowser(true);

            view.Measure(new Size(800, 600));
            view.Arrange(new Rect(0, 0, 800, 600));

            var toolbars = new List<ToolBarControl>();
            FindDescendants(view, toolbars);

            Assert.That(toolbars, Has.Count.EqualTo(1), "the standard icons and the VCD controls must share a single toolbar");

            var hasViewPicker = toolbars[0].Items.OfType<BarEditItem>().Any(item => (item.Content as string) == "View: ");

            Assert.That(hasViewPicker, Is.True, "the VCD view picker must be appended into the shared toolbar");
        }

        /// <summary>
        /// Collects every descendant of the requested type from the visual tree.
        /// </summary>
        /// <typeparam name="T">The type to collect.</typeparam>
        /// <param name="root">The root <see cref="DependencyObject"/>.</param>
        /// <param name="found">The accumulator the matches are added to.</param>
        private static void FindDescendants<T>(DependencyObject root, ICollection<T> found) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(root);

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);

                if (child is T match)
                {
                    found.Add(match);
                }

                FindDescendants(child, found);
            }
        }
    }
}
