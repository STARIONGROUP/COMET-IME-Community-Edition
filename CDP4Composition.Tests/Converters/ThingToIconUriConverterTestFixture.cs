// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingToIconUriConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Windows.Media.Imaging;
    using CDP4Common.CommonData;
    using CDP4Common.Helpers;
    using CDP4Common.MetaInfo;
    using CDP4Common.Poco;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// suite of tests for the <see cref="ThingToIconUriConverter"/>
    /// </summary>
    [TestFixture, RequiresSTA]
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
        [ExpectedException(typeof(NotSupportedException))]
        public void VerifyThatConvertBackThrowsException()
        {
            this.converter.ConvertBack(null, null, null, null);
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
