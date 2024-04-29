﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIconCacheService.cs" company="Starion Group S.A.">
//   Copyright (c) 2016-2020 Starion Group S.A. All rights reserved
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
            var key = (uri, uri, overlayPosition);
            this.bitmapWithOverlay.TryGetValue(key, out var bitmapSource);

            if (bitmapSource == null)
            {
                bitmapSource = IconUtilities.WithOverlay(uri, overlayUri, overlayPosition);
                this.bitmapWithOverlay.Add(key, bitmapSource);
            }

            return bitmapSource;
        }
    }
}
