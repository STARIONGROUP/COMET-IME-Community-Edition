// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesignerBehaviour.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Reporting.Behaviours
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using DevExpress.Xpf.Ribbon;
    using DevExpress.Mvvm.UI;
    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Reports.UserDesigner;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The purpose of the <see cref="ReportDesignerBehaviour"/> is to handle events from
    /// the attached view of type <see cref="ReportDesigner"/>
    /// </summary>
    /// <remarks>
    /// This behavior is meant to be attached to the ReportDesigner view from CDP4Reporting plugin
    /// </remarks>
    public class ReportDesignerBehaviour : Behavior<ReportDesigner>
    {
        /// <summary>
        /// The ribbon merge category stored for cleanup.
        /// </summary>
        private RibbonPageCategoryBase mergeCategory;

        /// <summary>
        /// The ribbon merged categories stored for cleanup.
        /// </summary>
        private List<RibbonPageCategoryBase> mergedCategories;

        /// <summary>
        /// The main ribbon of the shell.
        /// </summary>
        private RibbonControl parentRibbon;

        /// <summary>
        /// The dependency property that allows setting the <see cref="IEventPublisher"/>
        /// </summary>
        public static readonly DependencyProperty RibbonMergeCategoryNameProperty = DependencyProperty.Register("RibbonMergeCategoryName", typeof(string), typeof(ReportDesignerBehaviour));

        /// <summary>
        /// Initializes static members of the <see cref="ReportDesignerBehaviour"/> class.
        /// </summary>
        static ReportDesignerBehaviour()
        {
        }

        /// <summary>
        /// Gets or sets the name of the custom Ribbon Category to merge the reporting ribbon to.
        /// </summary>
        public string RibbonMergeCategoryName
        {
            get { return (string)this.GetValue(RibbonMergeCategoryNameProperty); }
            set { this.SetValue(RibbonMergeCategoryNameProperty, value); }
        }

        /// <summary>
        /// The on attached event handler
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += this.Loaded;
            this.AssociatedObject.Unloaded += this.Unloaded;
        }

        /// <summary>
        /// On Unloaded event handler.
        /// </summary>
        /// <param name="sender">The sender report designer control.</param>
        /// <param name="e">Event arguments.</param>
        [ExcludeFromCodeCoverage]
        private void Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is ReportDesigner && this.mergeCategory != null)
            {
                // clean up merged category
                this.ClearCategory();
            }
        }

        /// <summary>
        /// On Loaded event handler.
        /// </summary>
        /// <param name="sender">The sender report designer control.</param>
        /// <param name="e">Event arguments.</param>
        [ExcludeFromCodeCoverage]
        private void Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is ReportDesigner designControl && !string.IsNullOrWhiteSpace(this.RibbonMergeCategoryName))
            {
                // merge ribbon into category
                this.MergeRibbonToCategory(designControl);
            }
        }

        /// <summary>
        /// Clears the reporting ribbon from a specified RibbonCategory
        /// </summary>
        [ExcludeFromCodeCoverage]
        private void ClearCategory()
        {
            foreach (var ribbonPageCategoryBase in this.mergedCategories)
            {
                (this.mergeCategory as IRibbonMergingSupport)?.Unmerge(ribbonPageCategoryBase);
            }

            // select a valid selected page
            this.parentRibbon.SelectedPage = this.parentRibbon.ActualCategories.FirstOrDefault(x => x is RibbonDefaultPageCategory)?.ActualPages.FirstOrDefault();
        }

        /// <summary>
        /// Merges the reporting ribbon into a spcified RibbonCategory
        /// </summary>
        /// <param name="reportDesignerControl">The diagram design control</param>
        [ExcludeFromCodeCoverage]
        private void MergeRibbonToCategory(ReportDesigner reportDesignerControl)
        {
            // extract the report ribbon
            var reportRibbon = LayoutTreeHelper.GetVisualChildren(reportDesignerControl).OfType<RibbonControl>().FirstOrDefault();

            if (reportRibbon == null)
            {
                return;
            }

            // extract the main ribbon
            var mainShell = LayoutTreeHelper.GetVisualParents(reportDesignerControl).OfType<DXRibbonWindow>().FirstOrDefault();

            if (mainShell == null && this.parentRibbon == null)
            {
                return;
            }

            if (mainShell != null)
            {
                this.parentRibbon = mainShell.ActualRibbon;
            }

            // get the category to merge controls into
            var category = this.parentRibbon.ActualCategories.FirstOrDefault(x => x.Name == this.RibbonMergeCategoryName);

            if (category == null)
            {
                return;
            }

            // only merge if the category is visible, its visibility is controlled by RibbonCategoryBehavior
            if (category.IsVisible)
            {
                this.mergedCategories = new List<RibbonPageCategoryBase>();

                foreach (var reportingRibbonActualCategory in reportRibbon.ActualCategories)
                {
                    this.mergedCategories.Add(reportingRibbonActualCategory);
                    ((IRibbonMergingSupport)category).Merge(reportingRibbonActualCategory);
                }
            }

            // store category for cleanup
            this.mergeCategory = category;
        }

        /// <summary>
        /// Unsubscribes eventhandlers when detaching.
        /// </summary>
        [ExcludeFromCodeCoverage]
        protected override void OnDetaching()
        {
            this.AssociatedObject.Loaded -= this.Loaded;
            this.AssociatedObject.Unloaded -= this.Unloaded;

            base.OnDetaching();
        }
    }
}
