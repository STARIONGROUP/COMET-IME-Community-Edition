// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IIconCacheService.cs" company="Starion Group S.A.">
//   Copyright (c) 2016-2020 Starion Group S.A. All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// The purpose of the <see cref="IIconCacheService"/> is to provide a caching mechanism
    /// for icons that ar frequently queried
    /// </summary>
    public interface IIconCacheService
    {
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
        BitmapImage QueryBitmapImage(Uri uri);

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
        BitmapSource QueryErrorOverlayBitmapSource(Uri uri);

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
        /// <param name="overlayPosition">The overlay position</param>
        /// <returns>
        /// An instance of <see cref="BitmapSource"/>
        /// </returns>
        BitmapSource QueryOverlayBitmapSource(Uri uri, Uri overlayUri, OverlayPositionKind overlayPosition);
    }
}
