// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonContentBuilder.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Ribbon
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;

    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Ribbon;

    using NLog;

    /// <summary>
    /// Builds the ribbon content from supplied ribbon elements
    /// </summary>
    [Export(typeof(IRibbonContentBuilder))]
    public class RibbonContentBuilder : IRibbonContentBuilder
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// All the available <see cref="ExtendedRibbonPageCategory" /> from the composition container
        /// </summary>
        private readonly IEnumerable<ExtendedRibbonPageCategory> categories;

        /// <summary>
        /// All the available <see cref="ExtendedRibbonPageGroup" /> from the composition container
        /// </summary>
        private readonly IEnumerable<ExtendedRibbonPageGroup> groups;

        /// <summary>
        /// All the available <see cref="ExtendedRibbonPage" /> from the composition container
        /// </summary>
        private readonly IEnumerable<ExtendedRibbonPage> pages;

        /// <summary>
        /// Creates a <see cref="RibbonContentBuilder" />
        /// </summary>
        /// <param name="categories">The <see cref="ExtendedRibbonPageCategory" elements /></param>
        /// <param name="groups">The <see cref="ExtendedRibbonPageGroup" elements /></param>
        /// <param name="pages">The <see cref="ExtendedRibbonPage" elements /></param>
        [ImportingConstructor]
        public RibbonContentBuilder(
            [ImportMany] IEnumerable<ExtendedRibbonPageCategory> categories,
            [ImportMany] IEnumerable<ExtendedRibbonPageGroup> groups,
            [ImportMany] IEnumerable<ExtendedRibbonPage> pages)
        {
            this.categories = categories;
            this.groups = groups;
            this.pages = pages;
        }

        /// <summary>
        /// Builds and attaches the ribbon content supplied by the plugins to a <see cref="RibbonControl" />
        /// </summary>
        /// <param name="ribbon">The <see cref="RibbonControl" /> ribbon for which to add the content</param>
        public void BuildAndAppendToRibbon(RibbonControl ribbon)
        {
            if (this.pages.Any())
            {
                this.ProcessRibbonPages(ribbon);
            }

            if (this.groups.Any())
            {
                this.ProcessRibbonPageGroups(ribbon);
            }

            if (this.categories.Any())
            {
                this.ProcessRibbonPageCategories(ribbon);
            }
        }

        /// <summary>
        /// handles the addition of <see cref="ExtendedRibbonPageCategory" /> to the <see cref="RibbonControl" />
        /// </summary>
        /// <param name="ribbonPageCategories">
        /// The <see cref="ExtendedRibbonPageCategory" />s that need to be added to the <see cref="RibbonControl" />
        /// </param>
        private void ProcessRibbonPageCategories(RibbonControl ribbon)
        {
            foreach (var category in this.categories)
            {
                if (ribbon.Items?.Any(cat => (cat as ExtendedRibbonPageCategory)?.Name == category.Name) == false)
                {
                    ribbon.Items.Add(category);
                    logger.Debug("Category {0} added to RibbonControl", category.Name);
                }
            }
        }

        /// <summary>
        /// Handles the addition of <see cref="ExtendedRibbonPageGroup" /> to the <see cref="ExtendedRibbonPage" /> in a
        /// <see cref="RibbonControl" />
        /// </summary>
        /// <param name="ribbonPageGroups">
        /// The <see cref="ExtendedRibbonPage" />s that need to be added to the <see cref="ExtendedRibbonPageGroup" />.
        /// </param>
        /// <remarks>
        /// if the <see cref="ExtendedRibbonPageGroup.ContainerRegionName" /> matches the
        /// <see cref="ExtendedRibbonPage.RegionName" /> of an available
        /// <see cref="ExtendedRibbonPage" />, the <see cref="ExtendedRibbonPageGroup" /> will be added to that
        /// <see cref="ExtendedRibbonPage" />
        /// </remarks>
        private void ProcessRibbonPageGroups(RibbonControl ribbon)
        {
            foreach (var ribbonPageGroup in this.groups)
            {
                var containerRegionName = ribbonPageGroup.ContainerRegionName;

                if (ribbon.SelfCategories.Select(
                        category => category.Pages.OfType<ExtendedRibbonPage>()
                            .FirstOrDefault(x => x.RegionName == containerRegionName)).FirstOrDefault() is ExtendedRibbonPage ribbonPage)
                {
                    var existingGroup = ribbonPage.Groups.FirstOrDefault(g => g.Caption == ribbonPageGroup.Caption);

                    if (existingGroup == null)
                    {
                        var insertPosition = this.GetPositionForRibbonPageGroup(ribbonPage.Groups, ribbonPageGroup);
                        ribbonPage.Groups.Insert(insertPosition, ribbonPageGroup);
                        logger.Debug("RibbonPageGroup {0} added to RibbonPage {1} in position {2}", ribbonPageGroup.Name, ribbonPage.Name, insertPosition);
                    }
                    else
                    {
                        var extractedItems = ribbonPageGroup.Items;

                        foreach (var extractedItem in extractedItems)
                        {
                            if (!existingGroup.Items.Contains(extractedItem))
                            {
                                var insertPosition = this.GetPositionInsideRibbonGroup(existingGroup.Items, extractedItem);
                                existingGroup.Items.Insert(insertPosition, extractedItem);
                            }
                        }
                    }
                }
                else
                {
                    logger.Warn("The default Ribbon Category could not be found, the RibbonPageGroup {0} was not added", ribbonPageGroup.Name);
                }
            }
        }

        /// <summary>
        /// Get the insert position for a group item
        /// </summary>
        /// <param name="extractedItems">List of items in ribbon group</param>
        /// <param name="extractedItem">Item to be inserted in ribbon group</param>
        /// <returns></returns>
        private int GetPositionInsideRibbonGroup(CommonBarItemCollection extractedItems, IBarItem extractedItem)
        {
            var total = extractedItems.Count;

            if (total == 0)
            {
                return total;
            }

            var barItem = extractedItem as BarItem;
            var itemsList = extractedItems.ToList().ConvertAll(i => (BarItem)i);

            var requestedPosition = barItem?.MergeOrder ?? total;
            var position = itemsList.TakeWhile(x => x.MergeOrder <= requestedPosition).Count();

            return position;
        }

        /// <summary>
        /// handles the addition of <see cref="ExtendedRibbonPage" /> to the <see cref="RibbonControl" />
        /// </summary>
        /// <param name="ribbonPages">
        /// The <see cref="ExtendedRibbonPage" />s that need to be added to the <see cref="RibbonControl" />
        /// </param>
        private void ProcessRibbonPages(RibbonControl ribbon)
        {
            foreach (var ribbonPage in this.pages)
            {
                if (ribbonPage.IsInDefaultPageCategory)
                {
                    var defaultCategory = ribbon.SelfCategories.OfType<RibbonDefaultPageCategory>().SingleOrDefault();

                    if (defaultCategory != null)
                    {
                        var insertPosition = this.GetPositionForRibbonPage(defaultCategory.Pages, ribbonPage);
                        defaultCategory.Pages.Insert(insertPosition, ribbonPage);
                        logger.Debug("RibbonPage {0} added to the default Ribbon Category", ribbonPage.Name);
                    }
                    else
                    {
                        logger.Warn("The default Ribbon Category could not be found, the RibbonPage {0} was not added", ribbonPage.Name);
                    }
                }
                else
                {
                    var categoryName = ribbonPage.CustomPageCategoryName;

                    var customPageCategory = this.categories.FirstOrDefault(cat => cat.Name == categoryName);

                    if (customPageCategory != null)
                    {
                        var insertPosition = this.GetPositionForRibbonPage(customPageCategory.Pages, ribbonPage);
                        customPageCategory.Pages.Insert(insertPosition, ribbonPage);
                        logger.Debug("RibbonPage {0} added to Custom Ribbon Category {1}", ribbonPage.Name, categoryName);
                    }
                    else
                    {
                        logger.Warn("The Custom Ribbon Category {0} could not be found, the RibbonPage {1} was not added", categoryName, ribbonPage.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the position for the new <see cref="RibbonPageGroup" />
        /// </summary>
        /// <param name="existingRibbonPageGroups">
        /// The existing Ribbon Page Groups.
        /// </param>
        /// <param name="ribbonPageGroupToAdd">
        /// The ribbon Page Group To Add.
        /// </param>
        /// <returns>
        /// The position where the <see cref="RibbonPageGroup" /> will be added
        /// </returns>
        private int GetPositionForRibbonPageGroup(IList<RibbonPageGroup> existingRibbonPageGroups, RibbonPageGroup ribbonPageGroupToAdd)
        {
            var total = existingRibbonPageGroups.Count();

            if (total == 0)
            {
                return total;
            }

            var requestedPosition = ribbonPageGroupToAdd.MergeOrder ?? total;
            var position = existingRibbonPageGroups.TakeWhile(x => x.MergeOrder <= requestedPosition).Count();
            var controlWithSamePosition = existingRibbonPageGroups.FirstOrDefault(x => x.MergeOrder == requestedPosition);

            if (controlWithSamePosition == null)
            {
                return position;
            }

            // If there is already a RibbonPageGroup with the same MergeOrder use RibbonPageGroup's caption alphabetical order
            var indexFix = string.Compare(ribbonPageGroupToAdd.Caption, controlWithSamePosition.Caption);
            return indexFix == -1 ? position + indexFix : position;
        }

        /// <summary>
        /// Gets the position for the new <see cref="RibbonPage" />
        /// </summary>
        /// <param name="existingRibbonPages">
        /// The existing Ribbon Pages.
        /// </param>
        /// <param name="ribbonPageToAdd">
        /// The ribbon Page To Add.
        /// </param>
        /// <returns>
        /// The position where the <see cref="RibbonPage" /> will be added
        /// </returns>
        private int GetPositionForRibbonPage(IList<RibbonPage> existingRibbonPages, RibbonPage ribbonPageToAdd)
        {
            var total = existingRibbonPages.Count();

            if (total == 0)
            {
                return total;
            }

            var requestedPosition = ribbonPageToAdd.MergeOrder ?? total;
            var position = existingRibbonPages.TakeWhile(x => x.MergeOrder <= requestedPosition).Count();
            var controlWithSamePosition = existingRibbonPages.FirstOrDefault(x => x.MergeOrder == requestedPosition);

            if (controlWithSamePosition == null)
            {
                return position;
            }

            // If there is already a RibbonPage with the same MergeOrder use RibbonPage's caption alphabetical order
            var indexFix = string.Compare(ribbonPageToAdd.Caption.ToString(), controlWithSamePosition.Caption.ToString());
            return indexFix == -1 ? position + indexFix : position;
        }
    }
}
