// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionBrowserIconConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Converters
{
    using System;
    using System.Threading;
    using System.Windows.Media.Imaging;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;    
    using CDP4Composition.Services;
    using CDP4EngineeringModel.Converters;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// suite of tests for the <see cref="ThingToIconUriConverter"/>
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ElementDefinitionBrowserIconConverterTestFixture
    {
        /// <summary>
        /// the <see cref="ThingToIconUriConverter"/> under test
        /// </summary>
        private ElementDefinitionBrowserIconConverter converter;
        
        private ThingToIconUriConverter genericConverter;

        private IconCacheService iconCacheService;

        private Mock<IServiceLocator> serviceLocator;

        [SetUp]
        public void SetUp()
        {
            this.iconCacheService = new IconCacheService();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(x => x.GetInstance<IIconCacheService>()).Returns(this.iconCacheService);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            var ensurePackSchemeIsKnown = System.IO.Packaging.PackUriHelper.UriSchemePack;
            this.converter = new ElementDefinitionBrowserIconConverter();
            this.genericConverter = new ThingToIconUriConverter();
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForElementDefinition()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new ElementDefinition() }, null, null, null);
            var icon = (BitmapImage)this.converter.Convert(new object[] {new ElementDefinition() }, null, null, null);

            Assert.AreEqual(generic.UriSource.ToString(), icon.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForParameterBase()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new Parameter(),  }, null, null, null);
            var icon = (BitmapImage)this.converter.Convert(new object[] { new Parameter() }, null, null, null);

            Assert.AreEqual(generic.UriSource.ToString(), icon.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForParameterBaseWithOptionNoState()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new Option(), }, null, null, null);

            var parameter = new Parameter();
            parameter.ParameterType = new BooleanParameterType();
            var icon = this.converter.Convert(new object[] { parameter }, null, ClassKind.Option, null);
            // overlay
            Assert.IsTrue(icon is BitmapSource);
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForParameterBaseWithOptionWithState()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new Option() }, null, null, null);

            var parameter = new Parameter();
            parameter.ParameterType = new BooleanParameterType();
            parameter.StateDependence = new ActualFiniteStateList();
            var icon = (BitmapImage)this.converter.Convert(new object[] { parameter }, null, ClassKind.Option, null);

            // overlay
            Assert.AreEqual(generic.UriSource.ToString(), icon.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForParameterBaseWithStateCompound()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new ActualFiniteState() }, null, null, null);

            var parameter = new Parameter();
            parameter.ParameterType = new ArrayParameterType();
            parameter.StateDependence = new ActualFiniteStateList();
            var icon = (BitmapImage)this.converter.Convert(new object[] { parameter }, null, ClassKind.ActualFiniteState, null);

            Assert.AreEqual(generic.UriSource.ToString(), icon.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForParameterBaseWithState()
        {
            var generic = (BitmapImage) this.genericConverter.Convert(new object[] {new ActualFiniteState() }, null, null, null);

            var parameter = new Parameter();
            parameter.ParameterType = new BooleanParameterType();
            parameter.StateDependence = new ActualFiniteStateList();
            var icon = this.converter.Convert(new object[] { parameter }, null, ClassKind.ActualFiniteState, null);

            // overlay
            Assert.IsTrue(icon is BitmapSource);
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}