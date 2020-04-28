// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonAdapter.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Adapters
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.Linq;
    using CDP4Composition.Ribbon;
    using DevExpress.Xpf.Ribbon;
    using Microsoft.Practices.Prism.Regions;
    using NLog;

    /// <summary>
    /// A region adaptor for the <see cref="RibbonControl"/>
    /// </summary>
    [Export(typeof(RibbonAdapter)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class RibbonAdapter : RegionAdapterBase<RibbonControl>
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonAdapter" /> class.
        /// </summary>
        /// <param name="behaviorFactory">The behavior factory.</param>
        [ImportingConstructor]
        public RibbonAdapter(IRegionBehaviorFactory behaviorFactory)
            : base(behaviorFactory)
        {
        }

        /// <summary>
        /// Returns a <see cref="IRegion"/> instance to be associated with the adapted control.
        /// </summary>
        /// <returns>
        /// an instance of <see cref="IRegion"/>
        /// </returns>
        protected override IRegion CreateRegion()
        {
            return new Region();
        }

        /// <summary>
        /// Adapts the <see cref="RibbonControl"/> in the association <see cref="IRegion"/>
        /// </summary>
        /// <param name="region">
        /// The associated <see cref="IRegion"/>
        /// </param>
        /// <param name="regionTarget">
        /// the control to adapt
        /// </param>
        protected override void Adapt(IRegion region, RibbonControl regionTarget)
        {
            var manager = new Manager(region, regionTarget);
            logger.Debug("adapt region {0} in target {1} by {2}", region.Name, regionTarget.Name, manager);
        }

        /// <summary>
        /// region manager that controls the content of a <see cref="RibbonControl"/>
        /// </summary>
        private class Manager
        {
            /// <summary>
            /// The <see cref="IRegion"/> that is to be adapted
            /// </summary>
            private readonly IRegion region;

            /// <summary>
            /// The target of the region adapter
            /// </summary>
            private readonly RibbonControl regionTarget;

            /// <summary>
            /// Initializes a new instance of the <see cref="Manager" /> class.
            /// </summary>
            /// <param name="region">The region.</param>
            /// <param name="regionTarget">The region target.</param>
            public Manager(IRegion region, RibbonControl regionTarget)
            {
                this.region = region;
                this.regionTarget = regionTarget;
                this.region.Views.CollectionChanged += this.ViewsCollectionChanged;
            }

            /// <summary>
            /// Handles the CollectionChanged event of the Views control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
            private void ViewsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action != NotifyCollectionChangedAction.Add)
                {
                    return;
                }

                logger.Debug("Add control to region region {0} in target {1}", this.region.Name, this.regionTarget.Name);

                var ribbonPages = this.region.Views.OfType<ExtendedRibbonPage>().ToList();

                if (ribbonPages.Any())
                {
                    this.ProcessRibbonPages(ribbonPages);
                }

                var ribbonPageGroups = this.region.Views.OfType<ExtendedRibbonPageGroup>().ToList();

                if (ribbonPageGroups.Any())
                {
                    this.ProcessRibbonPageGroups(ribbonPageGroups);
                }

                var ribbonPageCategories = this.region.Views.OfType<ExtendedRibbonPageCategory>().ToList();

                if (ribbonPageCategories.Any())
                {
                   this.ProcessRibbonPageCategories(ribbonPageCategories);
                }
            }

            /// <summary>
            /// handles the addition of <see cref="ExtendedRibbonPageCategory"/> to the <see cref="RibbonControl"/>
            /// </summary>
            /// <param name="ribbonPageCategories">
            /// The <see cref="ExtendedRibbonPageCategory"/>s that need to be added to the <see cref="RibbonControl"/>
            /// </param>
            private void ProcessRibbonPageCategories(IEnumerable<ExtendedRibbonPageCategory> ribbonPageCategories)
            {
                foreach (var category in ribbonPageCategories)
                {
                    if (this.regionTarget.Items?.Any(cat => (cat as ExtendedRibbonPageCategory)?.Name == category.Name) == false)
                    {
                        this.regionTarget.Items.Add(category);
                        logger.Debug("Category {0} added to RibbonControl", category.Name);
                    }
                }
            }

            /// <summary>
            /// Handles the addition of <see cref="ExtendedRibbonPageGroup"/> to the <see cref="ExtendedRibbonPage"/> in a <see cref="RibbonControl"/>
            /// </summary>
            /// <param name="ribbonPageGroups">
            /// The <see cref="ExtendedRibbonPage"/>s that need to be added to the <see cref="ExtendedRibbonPageGroup"/>. 
            /// </param>
            /// <remarks>
            /// if the <see cref="ExtendedRibbonPageGroup.ContainerRegionName"/> matches the <see cref="ExtendedRibbonPage.RegionName"/> of an available
            /// <see cref="ExtendedRibbonPage"/>, the <see cref="ExtendedRibbonPageGroup"/> will be added to that <see cref="ExtendedRibbonPage"/>
            /// </remarks>
            private void ProcessRibbonPageGroups(IEnumerable<ExtendedRibbonPageGroup> ribbonPageGroups)
            {
                foreach (var ribbonPageGroup in ribbonPageGroups)
                {
                    var containerRegionName = ribbonPageGroup.ContainerRegionName;

                    if (this.regionTarget.SelfCategories.Select(category => category.Pages.OfType<ExtendedRibbonPage>()
                            .FirstOrDefault(x => x.RegionName == containerRegionName)).FirstOrDefault() is ExtendedRibbonPage ribbonPage)
                    {
                        var insertPosition = this.GetPositionForRibbonPageGroup(ribbonPage.Groups, ribbonPageGroup);
                        ribbonPage.Groups.Insert(insertPosition, ribbonPageGroup);

                        logger.Debug("RibbonPageGroup {0} added to RibbonPage {1} in position {2}", ribbonPageGroup.Name, ribbonPage.Name, insertPosition);
                    }
                    else
                    {
                        logger.Warn("The default Ribbon Category could not be found, the RibbonPageGroup {0} was not added", ribbonPageGroup.Name);
                    }
                }
            }

            /// <summary>
            /// handles the addition of <see cref="ExtendedRibbonPage"/> to the <see cref="RibbonControl"/>
            /// </summary>
            /// <param name="ribbonPages">
            /// The <see cref="ExtendedRibbonPage"/>s that need to be added to the <see cref="RibbonControl"/>
            /// </param>
            private void ProcessRibbonPages(IEnumerable<ExtendedRibbonPage> ribbonPages)
            {
                foreach (var ribbonPage in ribbonPages)
                {
                    if (ribbonPage.IsInDefaultPageCategory)
                    {
                        var defaultCategory = this.regionTarget.SelfCategories.OfType<RibbonDefaultPageCategory>().SingleOrDefault();
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

                        var customPageCategory = this.region.Views.OfType<ExtendedRibbonPageCategory>().FirstOrDefault(cat => cat.Name == categoryName);

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
            /// Gets the position for the new <see cref="RibbonPageGroup"/>
            /// </summary>
            /// <param name="existingRibbonPageGroups">
            /// The existing Ribbon Page Groups.
            /// </param>
            /// <param name="ribbonPageGroupToAdd">
            /// The ribbon Page Group To Add.
            /// </param>
            /// <returns>
            /// The position where the <see cref="RibbonPageGroup"/> will be added
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
            /// Gets the position for the new <see cref="RibbonPage"/>
            /// </summary>
            /// <param name="existingRibbonPages">
            /// The existing Ribbon Pages.
            /// </param>
            /// <param name="ribbonPageToAdd">
            /// The ribbon Page To Add.
            /// </param>
            /// <returns>
            /// The position where the <see cref="RibbonPage"/> will be added
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
}
