// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonFileStoreIconConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.converters
{
    using System;
    using System.Threading;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition;
    using CDP4Composition.Services;
    using CDP4EngineeringModel.Converters;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CommonFileStoreIconConverter"/>
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class CommonFileStoreIconConverterTestFixture
    {
        private CommonFileStoreIconConverter commonFileStoreIconConverter;

        private ThingToIconUriConverter genericConverter;

        private IconCacheService iconCacheService;

        private Mock<IServiceLocator> serviceLocator;

        [SetUp]
        public void SetUp()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(x => x.GetInstance<IIconCacheService>()).Returns(this.iconCacheService);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            var ensurePackSchemeIsKnown = System.IO.Packaging.PackUriHelper.UriSchemePack;
            var uri = new Uri("pack://blal");

            this.commonFileStoreIconConverter = new CommonFileStoreIconConverter();
            this.genericConverter = new ThingToIconUriConverter();
        }

        [Test]
        public void VerifyThatWhenTheValueIsNotAThingThenNullIsReturned()
        {
            var result = this.commonFileStoreIconConverter.Convert(new object[] { new object() }, null, null, null);
            Assert.IsNull(result);
        }

        [Test]
        public void VerifyThatIfAnyOtherThingThanFileIsProvidedImageIsReturned()
        {
            var result = (System.Windows.Media.Imaging.BitmapImage)this.commonFileStoreIconConverter.Convert(new object[] { new ElementDefinition() }, null, null, null);

            Assert.IsTrue(result.UriSource.ToString().Contains("Product"));
        }

        [Test]
        public void VerifyThatIfThingIsAFileABitmapIsReturned()
        {
            var result = (System.Windows.Media.Imaging.BitmapImage)this.commonFileStoreIconConverter.Convert(new object[] { new CDP4Common.EngineeringModelData.File() }, null, null, null);

            var expectedUri = "pack://application:,,,/DevExpress.Images.v20.1;component/Images/Business Objects/BOFileAttachment_16x16.png";

            Assert.AreEqual(expectedUri, result.UriSource.ToString());
        }
        
        [Test]
        public void VerifyThatIfNullReturnsNull()
        {
            var result = this.commonFileStoreIconConverter.Convert(null, null, null, null);
            Assert.IsNull(null);
        }

        [Test]
        public void VerifyThatIfEmptyObjectReturnsNull()
        {
            var result = this.commonFileStoreIconConverter.Convert(new object[] { }, null, null, null);
            Assert.IsNull(null);
        }

        [Test]
        public void VerifyThatConverBackThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => this.commonFileStoreIconConverter.ConvertBack(null, null, null, null));
        }
    }
}