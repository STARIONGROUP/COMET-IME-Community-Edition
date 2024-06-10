// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIconCacheService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate
//
//    This file is part of COMET-IME Community Edition.
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

namespace CDP4Composition.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Windows.Media.Imaging;

    using CDP4Common.Helpers;

    /// <summary>
    /// The purpose of the <see cref="IconCacheService"/> is to provide a caching mechanism
    /// for icons that ar frequently queried
    /// </summary>
    [Export(typeof(IIconCacheService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class IconCacheService : IIconCacheService
    {
        /// <summary>
        /// The cache of <see cref="BitmapImage"/>s
        /// </summary>
        private readonly Dictionary<Uri, BitmapImage> bitmapImages;

        /// <summary>
        /// The cache of error overlay <see cref="BitmapSource"/>s
        /// </summary>
        private readonly Dictionary<Uri, BitmapSource> bitmapSources;

        /// <summary>
        /// The cache of any other overlay icon than error overlay <see cref="BitmapSource"/>s
        /// </summary>
        private readonly Dictionary<(Uri, Uri, OverlayPositionKind), BitmapSource> bitmapWithOverlay;

        /// <summary>
        /// Initializes a new instance of the <see cref="IconCacheService"/> class.
        /// </summary>
        public IconCacheService()
        {
            this.bitmapImages = new Dictionary<Uri, BitmapImage>();
            this.bitmapSources = new Dictionary<Uri, BitmapSource>();
            this.bitmapWithOverlay = new Dictionary<(Uri, Uri, OverlayPositionKind), BitmapSource>();
        }

        /// <summary>
        /// Queries the <see cref="BitmapImage"/> from the cache if the cache contains it else the <see cref="BitmapImage"/>
        /// is created, added to the cache and then returned.
        /// </summary>
        /// <param name="uri">
        /// The uri of the <see cref="BitmapImage"/>
        /// </param>
        /// <returns>
        /// An instance of <see cref="BitmapImage"/>
        /// </returns>
        public BitmapImage QueryBitmapImage(Uri uri)
        {
            this.bitmapImages.TryGetValue(uri, out var bitmapImage);

            if (bitmapImage == null)
            {
                bitmapImage = new BitmapImage(uri);
                this.bitmapImages.Add(uri, bitmapImage);
            }

            return bitmapImage;
        }

        /// <summary>
        /// Queries the <see cref="BitmapSource"/> with an error icon overlayed from the cache if the cache contains it else the <see cref="BitmapSource"/>
        /// is created, added to the cache and then returned.
        /// </summary>
        /// <param name="uri">
        /// The uri of the <see cref="BitmapSource"/>
        /// </param>
        /// <returns>
        /// An instance of <see cref="BitmapSource"/>
        /// </returns>
        public BitmapSource QueryErrorOverlayBitmapSource(Uri uri)
        {
            this.bitmapSources.TryGetValue(uri, out var bitmapSource);

            if (bitmapSource == null)
            {
                bitmapSource = IconUtilities.WithErrorOverlay(uri);
                this.bitmapSources.Add(uri, bitmapSource);
            }

            return bitmapSource;
        }

        /// <summary>
        /// Queries the <see cref="BitmapSource"/> with an icon overlayed from the cache if the cache contains it else the <see cref="BitmapSource"/>
        /// is created, added to the cache and then returned.
        /// </summary>
        /// <param name="uri">
        /// The uri of the <see cref="BitmapSource"/>
        /// </param>
        /// <param name="overlayUri">
        /// The uri of the overlay <see cref="BitmapSource"/>
        /// </param>
        /// <param name="overlayPosition">The <see cref="OverlayPositionKind"/></param>
        /// <returns>
        /// An instance of <see cref="BitmapSource"/>
        /// </returns>
        public BitmapSource QueryOverlayBitmapSource(Uri uri, Uri overlayUri, OverlayPositionKind overlayPosition = OverlayPositionKind.TopRight)
        {
            try
            {
                var key = (uri, uri, overlayPosition);
                this.bitmapWithOverlay.TryGetValue(key, out var bitmapSource);

                if (bitmapSource == null)
                {
                    bitmapSource = IconUtilities.WithOverlay(uri, overlayUri, overlayPosition);
                    this.bitmapWithOverlay.Add(key, bitmapSource);
                }

                return bitmapSource;
            }
            catch (Exception e)
            {
                // Do nothing, just return null for this edge case. Otherwise the app will crash.
            }

            return null;
        }
    }
}
