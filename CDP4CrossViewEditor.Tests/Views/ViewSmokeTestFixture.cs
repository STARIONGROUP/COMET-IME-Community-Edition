// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewSmokeTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4CrossViewEditor.Tests.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4CrossViewEditor.Views;
    using CDP4CrossViewEditor.Views.UserControls;

    using NUnit.Framework;

    /// <summary>
    /// Loads the XAML of every view of the plugin, so that a markup error is caught by the suite instead of by a user.
    /// </summary>
    /// <remarks>
    /// Nothing else in this repository exercises XAML, so a view that compiles but cannot be loaded at runtime - a bad
    /// type reference, a missing static resource, a malformed binding expression - reaches a user with the whole suite
    /// still green. Constructing the control runs the markup parse that catches those.
    /// <para>
    /// Measured limit: an unresolvable <c>{dx:DXImage Image=...}</c> name does **not** fail this fixture. It was tried
    /// with a name that exists nowhere in the repository and every case still passed, so DevExpress evidently resolves
    /// the image lazily rather than while the markup is parsed. Invented image names are caught by the
    /// <c>style-guard</c> hook at edit time instead; do not rely on this fixture for them.
    /// </para>
    /// </remarks>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ViewSmokeTestFixture
    {
        /// <summary>
        /// The views whose markup is loaded. Add a view here when one is introduced.
        /// </summary>
        private static IEnumerable<Type> ViewTypes()
        {
            yield return typeof(CrossViewDialog);
            yield return typeof(ElementThingSelector);
            yield return typeof(ParameterThingSelector);
            yield return typeof(ThingSelectorControl);
        }

        [Test]
        [TestCaseSource(nameof(ViewTypes))]
        public void VerifyThatTheMarkupOfTheViewLoads(Type viewType)
        {
            FrameworkElement view = null;

            Assert.DoesNotThrow(
                () => view = (FrameworkElement)Activator.CreateInstance(viewType),
                $"the markup of {viewType.Name} could not be loaded");

            Assert.That(view, Is.Not.Null);

            // Measure walks the visual tree, which forces the templates and the resources of the child controls to be
            // resolved as well. Loading the markup alone does not necessarily reach them.
            Assert.DoesNotThrow(
                () => view.Measure(new Size(1024, 768)),
                $"{viewType.Name} loaded but could not be measured");
        }

        [Test]
        public void VerifyThatEveryViewOfThePluginIsCovered()
        {
            // A view that is added without being listed above would otherwise never be loaded by this fixture, which
            // would quietly bring back the gap this fixture exists to close.
            var declaredViews = typeof(ElementThingSelector).Assembly
                .GetTypes()
                .Where(type => typeof(FrameworkElement).IsAssignableFrom(type))
                .Where(type => !type.IsAbstract && type.IsPublic)
                .Where(type => type.GetConstructor(Type.EmptyTypes) != null)
                .Where(type => type.Namespace != null && type.Namespace.StartsWith("CDP4CrossViewEditor.Views", StringComparison.Ordinal))
                .ToList();

            var covered = ViewTypes().ToList();
            var missing = declaredViews.Except(covered).Select(type => type.Name).ToList();

            Assert.That(missing, Is.Empty, $"these views of the plugin are not loaded by this fixture: {string.Join(", ", missing)}");
        }
    }
}
