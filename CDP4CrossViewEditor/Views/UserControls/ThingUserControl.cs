// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingUserControl.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4CrossViewEditor.Views.UserControls
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using CDP4Common.CommonData;

    /// <summary>
    /// Base class for all things selector user controls
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ThingUserControl : UserControl
    {
        /// <summary>
        /// <see cref="DependencyProperty"/> that can be set in XAML to specify different ServerTypes options
        /// </summary>
        public static readonly DependencyProperty ModeDependencyProperty = DependencyProperty.RegisterAttached(
            "Mode",
            typeof(ClassKind),
            typeof(ThingUserControl),
            new PropertyMetadata(ClassKind.Thing, OnClassKindChanged));

        /// <summary>
        /// Gets or sets the <see cref="ModeDependencyProperty"/> dependency property.
        /// </summary>
        public ClassKind Mode
        {
            get => (ClassKind)this.GetValue(ModeDependencyProperty);
            set => this.SetValue(ModeDependencyProperty, value);
        }

        /// <summary>
        /// Static callback handler which will handle any changes that occurs globally
        /// </summary>
        /// <param name="d">The dependency object user control <see cref="DependencyObject" /></param>
        /// <param name="e">The dependency object changed event args <see cref="DependencyPropertyChangedEventArgs"/></param>
        private static void OnClassKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ThingSelector)?.ThingSelectionChanged(e);
        }

        /// <summary>
        /// Selection changed handler
        /// </summary>
        /// <param name="e">The dependency object changed event args <see cref="DependencyPropertyChangedEventArgs"/></param>
        protected virtual void ThingSelectionChanged(DependencyPropertyChangedEventArgs e)
        {
        }
    }
}
