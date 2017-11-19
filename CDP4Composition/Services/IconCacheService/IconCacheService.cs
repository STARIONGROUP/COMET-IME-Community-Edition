// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIconCacheService.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A. All rights reserved
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
        private Dictionary<Uri, BitmapImage> bitmapImages;

        /// <summary>
        /// The cache of error overlay <see cref="BitmapSource"/>s
        /// </summary>
        private Dictionary<Uri, BitmapSource> bitmapSources;

        /// <summary>
        /// Initializes a new instance of the <see cref="IconCacheService"/> class.
        /// </summary>
        public IconCacheService()
        {
            this.bitmapImages = new Dictionary<Uri, BitmapImage>();

            this.bitmapSources = new Dictionary<Uri, BitmapSource>();
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
            BitmapImage bitmapImage;
            this.bitmapImages.TryGetValue(uri, out bitmapImage);
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
            BitmapSource bitmapSource;
            this.bitmapSources.TryGetValue(uri, out bitmapSource);
            if (bitmapSource == null)
            {
                bitmapSource = IconUtilities.WithErrorOverlay(uri);
                this.bitmapSources.Add(uri, bitmapSource);
            }

            return bitmapSource;
        }
    }
}
