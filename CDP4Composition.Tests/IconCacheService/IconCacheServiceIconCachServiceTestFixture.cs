// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IconCacheServiceIconCachServiceTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016-2020 RHEA System S.A. All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.IconCacheService
{
    using System;
    using System.Threading;
    using System.Windows.Media.Imaging;    
    using CDP4Composition.Services;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="IconCacheService"/> class
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class IconCacheServiceIconCachServiceTestFixture
    {
        private IconCacheService iconCacheService;
        
        [SetUp]
        public void SetUp()
        {
            var ensurePackSchemeIsKnown = System.IO.Packaging.PackUriHelper.UriSchemePack;

            this.iconCacheService = new IconCacheService();
        }

        [Test]
        public void VerifyThatSameIconIsReturned()
        {
            const string naturalLanguageIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/naturallanguage.png";
            var imageUri = new Uri(naturalLanguageIcon);

            var firstBitmapImage = this.iconCacheService.QueryBitmapImage(imageUri);

            Assert.IsInstanceOf<BitmapImage>(firstBitmapImage);

            var secondBitmapImage = this.iconCacheService.QueryBitmapImage(imageUri);

            Assert.IsInstanceOf<BitmapImage>(secondBitmapImage);

            Assert.AreSame(firstBitmapImage , secondBitmapImage);
        }

        [Test]
        public void VerityThatSameBitmapSourceIsReturned()
        {
            const string naturalLanguageIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/naturallanguage.png";
            var imageUri = new Uri(naturalLanguageIcon);

            var firstBitmapSource = this.iconCacheService.QueryErrorOverlayBitmapSource(imageUri);

            Assert.IsInstanceOf<BitmapSource>(firstBitmapSource);

            var secondBitmapSource = this.iconCacheService.QueryErrorOverlayBitmapSource(imageUri);

            Assert.IsInstanceOf<BitmapSource>(secondBitmapSource);

            Assert.AreSame(firstBitmapSource, secondBitmapSource);
        }


        [Test]
        public void VerityThatOverlayIconGetCached()
        {
            var stopWatchFirstFetch = new System.Diagnostics.Stopwatch();
            var stopWatchSecondFetch = new System.Diagnostics.Stopwatch();
            const string naturalLanguageIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/naturallanguage.png";
            const string relationshipOverlayUri = "pack://application:,,,/CDP4Composition;component/Resources/Images/Log/linkgreen_16x16.png";
            
            var imageUri = new Uri(naturalLanguageIcon);
            var overlayUri = new Uri(relationshipOverlayUri);

            stopWatchFirstFetch.Start();
            var firstBitmapSource = this.iconCacheService.QueryOverlayBitmapSource(imageUri, overlayUri);
            stopWatchFirstFetch.Stop();

            Assert.IsInstanceOf<BitmapSource>(firstBitmapSource);

            stopWatchSecondFetch.Start();
            var secondBitmapSource = this.iconCacheService.QueryOverlayBitmapSource(imageUri, overlayUri);
            Assert.IsInstanceOf<BitmapSource>(secondBitmapSource);
            stopWatchSecondFetch.Stop();

            Assert.AreSame(firstBitmapSource, secondBitmapSource);

            Assert.Greater(stopWatchFirstFetch.ElapsedMilliseconds, stopWatchSecondFetch.ElapsedMilliseconds);
        }

    }
}
