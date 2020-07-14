//  --------------------------------------------------------------------------------------------------------------------
//  <copyright file="CDP4RibbonMerge.cs" company="RHEA System S.A.">
//     Copyright (c) 2015-2020 RHEA System S.A.
// 
//     Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//             Nathanael Smiechowski, Kamil Wojnowski
// 
//     This file is part of CDP4-IME Community Edition.
//     The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//     compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//     The CDP4-IME Community Edition is free software; you can redistribute it and/or
//     modify it under the terms of the GNU Affero General Public
//     License as published by the Free Software Foundation; either
//     version 3 of the License, or any later version.
// 
//     The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//     GNU Affero General Public License for more details.
// 
//     You should have received a copy of the GNU Affero General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//  </copyright>
//  --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Ribbon
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;

    /// <summary>
    /// Attached dependency properties to handle ribbon button merging
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CDP4RibbonMerge : DependencyObject
    {
        /// <summary>
        /// The property describing the name of the ribbon page to merge this item
        /// </summary>
        public static readonly DependencyProperty MergePageProperty = DependencyProperty.RegisterAttached(
            "MergePage",
            typeof(string),
            typeof(CDP4RibbonMerge),
            new PropertyMetadata("Home"));

        /// <summary>
        /// Gets the MergePage property from a dependency object
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <returns>The value of the property</returns>
        public static string GetMergePage(DependencyObject d)
        {
            return (string)d.GetValue(MergePageProperty);
        }

        /// <summary>
        /// Sets the MergePage property to a dependency object
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="value">The value.</param>
        public static void SetMergePage(DependencyObject d, string value)
        {
            d.SetValue(MergePageProperty, value);
        }

        /// <summary>
        /// The property describing the name of the ribbon page group to merge this item
        /// </summary>
        public static readonly DependencyProperty MergePageGroupProperty = DependencyProperty.RegisterAttached(
            "MergePageGroup",
            typeof(string),
            typeof(CDP4RibbonMerge),
            new PropertyMetadata("Others"));

        /// <summary>
        /// Gets the MergePageGroup property from a dependency object
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <returns>The value of the property</returns>
        public static string GetMergePageGroup(DependencyObject d)
        {
            return (string)d.GetValue(MergePageGroupProperty);
        }

        /// <summary>
        /// Sets the MergePageGroup property to a dependency object
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="value">The value.</param>
        public static void SetMergePageGroup(DependencyObject d, string value)
        {
            d.SetValue(MergePageGroupProperty, value);
        }

        /// <summary>
        /// The property describing the merge order of the group in the page
        /// </summary>
        public static readonly DependencyProperty GroupMergeOrderInPageProperty = DependencyProperty.RegisterAttached(
            "GroupMergeOrderInPage",
            typeof(int?),
            typeof(CDP4RibbonMerge),
            new PropertyMetadata(1000));

        /// <summary>
        /// Gets the GroupMergeOrderInPage property from a dependency object
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <returns>The value of the property</returns>
        public static int? GetGroupMergeOrderInPage(DependencyObject d)
        {
            return (int?)d.GetValue(GroupMergeOrderInPageProperty);
        }

        /// <summary>
        /// Sets the MergeOrderInGroup property to a dependency object
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="value">The value.</param>
        public static void SetGroupMergeOrderInPage(DependencyObject d, int? value)
        {
            d.SetValue(GroupMergeOrderInPageProperty, value);
        }

        /// <summary>
        /// The property describing the merge order of the group in the page
        /// </summary>
        public static readonly DependencyProperty GroupPageMergeOrderProperty = DependencyProperty.RegisterAttached(
            "GroupPageMergeOrder",
            typeof(int?),
            typeof(CDP4RibbonMerge),
            new PropertyMetadata(1000));

        /// <summary>
        /// Gets the GroupPageMergeOrder property from a dependency object
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <returns>The value of the property</returns>
        public static int? GetGroupPageMergeOrder(DependencyObject d)
        {
            return (int?)d.GetValue(GroupPageMergeOrderProperty);
        }

        /// <summary>
        /// Sets the GroupPageMergeOrder property to a dependency object
        /// </summary>
        /// <param name="d">The dependency object.</param>
        /// <param name="value">The value.</param>
        public static void SetGroupPageMergeOrder(DependencyObject d, int? value)
        {
            d.SetValue(GroupPageMergeOrderProperty, value);
        }
    }
}
