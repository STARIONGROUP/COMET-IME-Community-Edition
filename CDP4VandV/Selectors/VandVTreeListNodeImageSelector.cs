// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVTreeListNodeImageSelector.cs" company="Starion Group S.A.">
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

namespace CDP4VandV.Selectors
{
    using System;
    using System.Globalization;
    using System.Windows.Media;

    using CDP4VandV.Services;
    using CDP4VandV.ViewModels.Rows;

    using CDP4Common.CommonData;
    using CDP4Common.Helpers;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CommonServiceLocator;

    using DevExpress.Xpf.Core;
    using DevExpress.Xpf.Core.Native;
    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;

    /// <summary>
    /// Selects the tree-node icons of the VCD browser. Specifications, groups and requirements get exactly the same
    /// icons the stock Requirements browser uses, so the two read alike. A V&amp;V item gets its own, visually distinct
    /// icon, not the requirement icon, so it is obvious at a glance which rows are requirements and which are
    /// verification activities.
    /// </summary>
    public class VandVTreeListNodeImageSelector : TreeListNodeImageSelector
    {
        /// <summary>
        /// The <see cref="ThingToIconUriConverter"/> used for the stock icons.
        /// </summary>
        private readonly ThingToIconUriConverter thingToIconUriConverter = new ThingToIconUriConverter();

        /// <summary>
        /// The cached <see cref="IIconCacheService"/>.
        /// </summary>
        private IIconCacheService iconCacheService;

        /// <summary>
        /// The V&amp;V item icon for an item with no review request against it, a plain checklist glyph, deliberately
        /// unlike the requirement icon.
        /// </summary>
        private static readonly Uri VandVItemIconUri = QueryImageUri("Task_16x16.png");

        /// <summary>
        /// The V&amp;V item icon for an item with at least one <b>open</b> review request, a warning glyph, so a
        /// blocked item stands out in the tree without opening any menu.
        /// </summary>
        private static readonly Uri VandVItemOpenRequestIconUri = QueryImageUri("Warning_16x16.png");

        /// <summary>
        /// The V&amp;V item icon for an item whose review requests have all been closed out, a tick glyph.
        /// </summary>
        private static readonly Uri VandVItemResolvedRequestIconUri = QueryImageUri("Apply_16x16.png");

        /// <summary>
        /// The procedure step icon, distinct again from a V&amp;V item so the Procedure view reads at a glance.
        /// </summary>
        private static readonly Uri VandVStepIconUri = QueryImageUri("ListBullets_16x16.png");

        /// <summary>
        /// Selects the icon for a row.
        /// </summary>
        /// <param name="rowData">The <see cref="TreeListRowData"/>.</param>
        /// <returns>The <see cref="ImageSource"/> for the row, or null when it cannot be determined.</returns>
        public override ImageSource Select(TreeListRowData rowData)
        {
            if (rowData.Row is VandVStepRowViewModel)
            {
                return this.QueryIcon(VandVStepIconUri);
            }

            if (rowData.Row is VandVItemRowViewModel vandVItemRow)
            {
                return this.QueryVandVItemIcon(vandVItemRow.AnnotationState);
            }

            if (!(rowData.Row is IRowViewModelBase<Thing> row))
            {
                return null;
            }

            var thingStatus = (row as IHaveThingStatus)?.ThingStatus;

            return this.thingToIconUriConverter.Convert(new object[] { row.Thing, thingStatus }, null, null, CultureInfo.InvariantCulture) as ImageSource;
        }

        /// <summary>
        /// Builds the V&amp;V item icon for a review-request state. These are dedicated images rather than the
        /// requirement icon, so a V&amp;V item never looks like a requirement, and a V&amp;V item carrying an open
        /// non-conformance never looks like a clean one.
        /// </summary>
        /// <param name="annotationState">The item's review-request state.</param>
        /// <returns>The <see cref="ImageSource"/>, or null when the image cannot be resolved.</returns>
        private ImageSource QueryVandVItemIcon(AnnotationState annotationState)
        {
            Uri uri;

            switch (annotationState)
            {
                case AnnotationState.Open:
                    uri = VandVItemOpenRequestIconUri;
                    break;
                case AnnotationState.Resolved:
                    uri = VandVItemResolvedRequestIconUri;
                    break;
                default:
                    uri = VandVItemIconUri;
                    break;
            }

            return this.QueryIcon(uri);
        }

        /// <summary>
        /// Resolves an icon through the shared icon cache.
        /// </summary>
        /// <param name="uri">The image <see cref="Uri"/>.</param>
        /// <returns>The <see cref="ImageSource"/>, or null when the image cannot be resolved.</returns>
        private ImageSource QueryIcon(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            this.iconCacheService = this.iconCacheService ?? ServiceLocator.Current.GetInstance<IIconCacheService>();

            return this.iconCacheService.QueryBitmapImage(uri);
        }

        /// <summary>
        /// Resolves a DevExpress image name to the <see cref="Uri"/> the icon cache reads.
        /// </summary>
        /// <param name="imageName">The DevExpress image name.</param>
        /// <returns>The <see cref="Uri"/>, or null when the image cannot be resolved.</returns>
        private static Uri QueryImageUri(string imageName)
        {
            return (new DXImageConverter().ConvertFrom(imageName) as DXImageInfo)?.MakeUri();
        }
    }
}
