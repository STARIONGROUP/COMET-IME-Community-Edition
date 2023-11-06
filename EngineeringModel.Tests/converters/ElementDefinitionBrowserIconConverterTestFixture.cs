// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionBrowserIconConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Converters
{
    using System.IO.Packaging;
    using System.Threading;
    using System.Windows.Media.Imaging;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;

    using CommonServiceLocator;

    using CDP4EngineeringModel.Selectors;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// suite of tests for the <see cref="ThingToIconUriConverter" />
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ElementDefinitionTreeListNodeImageSelectorTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            this.iconCacheService = new IconCacheService();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.serviceLocator.Setup(x => x.GetInstance<IIconCacheService>()).Returns(this.iconCacheService);
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            var ensurePackSchemeIsKnown = PackUriHelper.UriSchemePack;
            this.elementDefinitionTreeListNodeImageSelector = new ElementDefinitionTreeListNodeImageSelector();
            this.genericConverter = new ThingToIconUriConverter();
        }

        /// <summary>
        /// the <see cref="ThingToIconUriConverter" /> under test
        /// </summary>
        private ElementDefinitionTreeListNodeImageSelector elementDefinitionTreeListNodeImageSelector;

        private ThingToIconUriConverter genericConverter;

        private IconCacheService iconCacheService;

        private Mock<IServiceLocator> serviceLocator;

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForElementDefinition()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new ElementDefinition() }, null, null, null);
            var icon = (BitmapImage)this.elementDefinitionTreeListNodeImageSelector.Convert(new object[] { new ElementDefinition() }, null, null, null);

            Assert.AreEqual(generic.UriSource.ToString(), icon.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForParameterBase()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new Parameter() }, null, null, null);
            var icon = (BitmapImage)this.elementDefinitionTreeListNodeImageSelector.Convert(new object[] { new Parameter() }, null, null, null);

            Assert.AreEqual(generic.UriSource.ToString(), icon.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForParameterBaseWithOptionNoState()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new Option() }, null, null, null);

            var parameter = new Parameter();
            parameter.ParameterType = new BooleanParameterType();
            var icon = this.elementDefinitionTreeListNodeImageSelector.Convert(new object[] { parameter }, null, ClassKind.Option, null);

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
            var icon = (BitmapImage)this.elementDefinitionTreeListNodeImageSelector.Convert(new object[] { new ThingStatus(parameter) }, null, ClassKind.Option, null);

            // overlay
            Assert.AreEqual(generic.UriSource.ToString(), icon.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForParameterBaseWithState()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new ActualFiniteState() }, null, null, null);

            var parameter = new Parameter();
            parameter.ParameterType = new BooleanParameterType();
            parameter.StateDependence = new ActualFiniteStateList();
            var icon = this.elementDefinitionTreeListNodeImageSelector.Convert(new object[] { parameter }, null, ClassKind.ActualFiniteState, null);

            // overlay
            Assert.IsTrue(icon is BitmapSource);
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIconForParameterBaseWithStateCompound()
        {
            var generic = (BitmapImage)this.genericConverter.Convert(new object[] { new ActualFiniteState() }, null, null, null);

            var parameter = new Parameter();
            parameter.ParameterType = new ArrayParameterType();
            parameter.StateDependence = new ActualFiniteStateList();
            var icon = (BitmapImage)this.elementDefinitionTreeListNodeImageSelector.Convert(new object[] { new ThingStatus(parameter) }, null, ClassKind.ActualFiniteState, null);

            Assert.AreEqual(generic.UriSource.ToString(), icon.UriSource.ToString());
        }
    }
}
