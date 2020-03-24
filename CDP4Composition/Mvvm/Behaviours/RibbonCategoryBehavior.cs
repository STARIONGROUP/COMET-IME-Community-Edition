// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonCategoryBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski.
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
namespace CDP4Composition.Mvvm.Behaviours
{
    using System.Linq;
    using System.Windows;

    using CDP4Composition.Ribbon;

    using DevExpress.Mvvm.UI.Interactivity;

    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Allows proper callbacks on the diagramming category ribbon page
    /// </summary>
    public class RibbonCategoryBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// Initializes members of the <see cref="RibbonCategoryBehavior"/> class.
        /// </summary>
        public RibbonCategoryBehavior()
        {
            this.RegionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            this.RibbonRegion = this.RegionManager.Regions.FirstOrDefault(region => region.Name == RegionNames.RibbonRegion);
        }

        public IRegionManager RegionManager { get; set; }

        public IRegion RibbonRegion { get; set; }

        public string CategoryName { get; set; }

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="SetCategoryName"/> method.
        /// </summary>
        public static readonly DependencyProperty CategoryNameProperty =
            DependencyProperty.RegisterAttached(
                "CategoryName",
                typeof(string),
                typeof(RibbonCategoryBehavior),
                new PropertyMetadata(CategoryNameChanged));

        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.IsVisibleChanged += this.AssociatedObject_IsVisibleChanged;
        }

        /// <summary>
        /// Unsubscribes eventhandlers when detaching.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.IsVisibleChanged -= this.AssociatedObject_IsVisibleChanged;
            base.OnDetaching();
        }

        private void AssociatedObject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var category = this.RibbonRegion.Views.OfType<ExtendedRibbonPageCategory>().FirstOrDefault(view => view.Name == this.CategoryName);

            if (category != null)
            {
                category.IsVisible = (bool)e.NewValue;
            }
        }

        /// <summary>
        /// Sets the dialog result of the target <see cref="RibbonCategoryBehavior"/>.
        /// </summary>
        /// <param name="target">
        /// the <see cref="RibbonCategoryBehavior"/> that has this behavior attached.
        /// </param>
        /// <param name="value">
        /// The actual category name.
        /// </param>
        public static void SetCategoryName(RibbonCategoryBehavior target, string value)
        {
            target.SetValue(CategoryNameProperty, value);
        }

        /// <summary>
        /// Event handler for a change on the <see cref="CategoryNameProperty"/>.
        /// </summary>
        /// <param name="d">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private static void CategoryNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RibbonCategoryBehavior ribbonCategoryBehavior)
            {
                ribbonCategoryBehavior.CategoryName = (string)e.NewValue;
            }
        }
    }
}
