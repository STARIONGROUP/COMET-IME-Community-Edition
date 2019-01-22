// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IconCachServiceIconCachServiceTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A. All rights reserved
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
    public class IconCachServiceIconCachServiceTestFixture
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
            const string NaturalLanguageIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/naturallanguage.png";
            var imageUri = new Uri(NaturalLanguageIcon);

            var firstBitmapImage = this.iconCacheService.QueryBitmapImage(imageUri);

            Assert.IsInstanceOf<BitmapImage>(firstBitmapImage);

            var secondBitmapImage = this.iconCacheService.QueryBitmapImage(imageUri);

            Assert.IsInstanceOf<BitmapImage>(secondBitmapImage);

            Assert.AreSame(firstBitmapImage , secondBitmapImage);
        }

        [Test]
        public void VerityThatSameBitmapSourceIsReturned()
        {
            const string NaturalLanguageIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/naturallanguage.png";
            var imageUri = new Uri(NaturalLanguageIcon);

            var firstBitmapSource = this.iconCacheService.QueryErrorOverlayBitmapSource(imageUri);

            Assert.IsInstanceOf<BitmapSource>(firstBitmapSource);

            var secondBitmapSource = this.iconCacheService.QueryErrorOverlayBitmapSource(imageUri);

            Assert.IsInstanceOf<BitmapSource>(secondBitmapSource);

            Assert.AreSame(firstBitmapSource, secondBitmapSource);
        }
    }
}
