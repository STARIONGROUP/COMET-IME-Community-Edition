// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingToIconUriConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Windows.Media.Imaging;
    using CDP4Common.CommonData;
    using CDP4Common;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;
    using CommonServiceLocator;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// suite of tests for the <see cref="ThingToIconUriConverter"/>
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ThingToIconUriConverterTestFixture
    {
        /// <summary>
        /// the <see cref="ThingToIconUriConverter"/> under test
        /// </summary>
        private ThingToIconUriConverter converter;

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
            this.converter = new ThingToIconUriConverter();
        }

        [Test]
        public void VerifyThatConvertingNullReturnsNull()
        {
            var icon = this.converter.Convert(null, null, null, null);
            Assert.IsNull(icon);
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIcon()
        {
            const string NaturalLanguageIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/naturallanguage.png";
            var naturalLanguage = new NaturalLanguage();
            var firstConverterResult = (BitmapImage)this.converter.Convert(new object[]{naturalLanguage}, null, null, null);

            Assert.AreEqual(NaturalLanguageIcon, firstConverterResult.UriSource.ToString());

            var secondConverterResult = (BitmapImage)this.converter.Convert(new object[] { naturalLanguage }, null, null, null);

            Assert.AreSame(firstConverterResult, secondConverterResult);
        }

        [Test]
        public void VerifyThatConvertPersonProvidesTheExpectedIcon()
        {
            const string personGrayIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/grayscalePerson_16x16.png";
            var person = new Person();
            var converterResult = (BitmapImage)this.converter.Convert(new object[] { person }, null, null, null);

            Assert.AreNotEqual(personGrayIcon, converterResult.UriSource.ToString());

            converterResult = (BitmapImage)this.converter.Convert(new object[] { person, RowStatusKind.Inactive }, null, true, null);
            Assert.AreEqual(personGrayIcon, converterResult.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertParticipantProvidesTheExpectedIcon()
        {
            const string participantGrayIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/grayscaleParticipant_16x16.png";
            var participant = new Participant();
            var converterResult = (BitmapImage)this.converter.Convert(new object[] { participant }, null, null, null);

            Assert.AreNotEqual(participantGrayIcon, converterResult.UriSource.ToString());

            converterResult = (BitmapImage)this.converter.Convert(new object[] { participant, RowStatusKind.Inactive }, null, null, null);
            Assert.AreEqual(participantGrayIcon, converterResult.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertIterationSetupProvidesTheExpectedIcon()
        {
            const string iterationSetupGrayIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/Thing/grayscaleIterationSetup_16x16.png";
            var iterationSetup = new IterationSetup();
            var converterResult = (BitmapImage)this.converter.Convert(new object[] { iterationSetup }, null, null, null);

            Assert.AreNotEqual(iterationSetupGrayIcon, converterResult.UriSource.ToString());

            converterResult = (BitmapImage)this.converter.Convert(new object[] { iterationSetup, RowStatusKind.Inactive }, null, null, null);
            Assert.AreEqual(iterationSetupGrayIcon, converterResult.UriSource.ToString());
        }

        /// <summary>
        /// coverage test
        /// </summary>
        [Test]
        public void VerifyThatIconIsReturnedForAllClassKind()
        {
            var assembly = Assembly.GetAssembly(typeof(Thing));
            var values = Enum.GetValues(typeof(ClassKind)).Cast<ClassKind>().Select(x => x.ToString()).ToList();
            foreach (var type in assembly.GetTypes())
            {
                if (!values.Contains(type.Name) || type.FullName.Contains("DTO"))
                {
                    continue;
                }

                Thing thing;
                if (type.Name == "NotThing")
                {
                    thing = new NotThing("a");
                }
                else
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    thing = (Thing)Activator.CreateInstance(type, Guid.NewGuid(), null, null);
                }

                var converterResult = this.converter.Convert(new object[] { thing }, null, null, null);
                Assert.IsNotNull(converterResult);
            }
        }

        /// <summary>
        /// coverage test
        /// </summary>
        [Test]
        public void VerifyThatIconIsReturnedForAllClassKindGrayScale()
        {
            var assembly = Assembly.GetAssembly(typeof(Thing));
            var values = Enum.GetValues(typeof(ClassKind)).Cast<ClassKind>().Select(x => x.ToString()).ToList();
            foreach (var type in assembly.GetTypes())
            {
                if (!values.Contains(type.Name) || type.FullName.Contains("DTO"))
                {
                    continue;
                }

                Thing thing;
                if (type.Name == "NotThing")
                {
                    thing = new NotThing("a");
                }
                else
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    thing = (Thing)Activator.CreateInstance(type, Guid.NewGuid(), null, null);
                }

                var converterResult = this.converter.Convert(new object[] { thing, RowStatusKind.Inactive }, null, null, null);
                Assert.IsNotNull(converterResult);
            }
        }

        [Test]
        public void VerifyThatLargeIconIsReturnedForAllClassKind()
        {
            var values = Enum.GetValues(typeof(ClassKind)).Cast<ClassKind>();
            foreach (var classKind in values)
            {
                var converterResult = this.converter.GetImage(classKind, false);
                Assert.IsNotNull(converterResult);
            }
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }

        [Test]
        public void VerifyThatThingWithErrorReturnsIconWithErrorOverlay()
        {
            var constant = new Constant();
            constant.ValidatePoco();
            var converterResult = (System.Windows.Interop.InteropBitmap)this.converter.Convert(new object[] { constant }, null, null, null);

            Assert.NotNull(converterResult);
        }
    }
}
