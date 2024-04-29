// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IconCacheServiceIconCachServiceTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2016-2020 Starion Group S.A. All rights reserved
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
            const string naturalLanguageIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/naturallanguage.png";
            const string relationshipOverlayUri = "pack://application:,,,/CDP4Composition;component/Resources/Images/Log/linkgreen_16x16.png";
            
            var imageUri = new Uri(naturalLanguageIcon);
            var overlayUri = new Uri(relationshipOverlayUri);

            var firstBitmapSource = this.iconCacheService.QueryOverlayBitmapSource(imageUri, overlayUri);

            Assert.IsInstanceOf<BitmapSource>(firstBitmapSource);

            var secondBitmapSource = this.iconCacheService.QueryOverlayBitmapSource(imageUri, overlayUri);
            Assert.IsInstanceOf<BitmapSource>(secondBitmapSource);

            Assert.AreSame(firstBitmapSource, secondBitmapSource);
        }
    }
}
