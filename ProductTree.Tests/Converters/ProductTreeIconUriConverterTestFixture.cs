// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeIconUriConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.Tests.Converters
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;
    using System.Windows.Media.Imaging;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CommonView;

    using CDP4Composition.Services.NestedElementTreeService;

    using CDP4Dal;

    using CDP4ProductTree.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    using ParameterRowViewModel = CDP4ProductTree.ViewModels.ParameterRowViewModel;

    /// <summary>
    /// suite of tests for the <see cref="ProductTreeIconUriConverter"/>
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal class ProductTreeIconUriConverterTestFixture
    {
        private Mock<ISession> session;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new Uri("http://test.com");
        private Parameter parameter;
        private ParameterType parameterType;
        private ElementDefinition elementdef;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private SiteDirectory siteDirectory;
        private Participant participant;
        private Person person;
        private Option option;
        private DomainOfExpertise domain;
        private ProductTreeIconUriConverter converter;
        private Mock<INestedElementTreeService> nestedElementTreeService;
        private Mock<IServiceLocator> serviceLocator;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            var ensurePackSchemeIsKnown = System.IO.Packaging.PackUriHelper.UriSchemePack;
            this.session = new Mock<ISession>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "test", Surname = "test" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person };
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.elementdef = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);

            this.siteDirectory.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.siteDirectory.Person.Add(this.person);

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration.Element.Add(this.elementdef);

            this.parameterType = new EnumerationParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "pt1" };
            this.parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.parameterType, Owner = this.domain };
            this.converter = new ProductTreeIconUriConverter();
            this.elementdef.Parameter.Add(this.parameter);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.cache.TryAdd(new CacheKey(this.parameter.Iid, null), new Lazy<Thing>(() => this.parameter));

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.nestedElementTreeService = new Mock<INestedElementTreeService>();
            this.serviceLocator.Setup(x => x.GetInstance<INestedElementTreeService>()).Returns(this.nestedElementTreeService.Object);
        }

        [Test]
        public void VerifyThatConvertingNullReturnsNull()
        {
            var icon = this.converter.Convert(null, null, null, null);
            Assert.IsNull(icon);
        }

        [Test]
        public void VerifyThatConvertingNonParameterOrOverrideBaseReturnsNull()
        {
            var naturalLanguage = new NaturalLanguage();
            var converterResult = (BitmapImage)this.converter.Convert(new object[] { naturalLanguage }, null, null, null);
            Assert.IsNull(converterResult);

            var naturalLanguageRow = new NaturalLanguageRowViewModel(naturalLanguage, this.session.Object, null);
            var converterResult2 = (BitmapImage)this.converter.Convert(new object[] { naturalLanguageRow }, null, null, null);
            Assert.IsNull(converterResult2);
        }

        [Test]
        public void VerifyThatConvertProvidesTheExpectedIcon()
        {
            if (Application.Current == null)
            {
                new Application();
            }

            var row = new ParameterRowViewModel(this.parameter, this.option, this.session.Object, null);
            row.Usage = ParameterUsageKind.Unused;
            var unusedIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/orangeball.jpg";
            var subscribedByOthersIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/blueball.gif";
            var subscribedIcon = "pack://application:,,,/CDP4Composition;component/Resources/Images/whiteball.jpg";

            var converterResult = (BitmapImage)this.converter.Convert(new object[] { row.ThingStatus, row.Usage }, null, null, null);
            Assert.AreEqual(unusedIcon, converterResult.UriSource.ToString());

            row.Usage = ParameterUsageKind.SubscribedByOthers;
            converterResult = (BitmapImage)this.converter.Convert(new object[] { row.ThingStatus, row.Usage }, null, null, null);
            Assert.AreEqual(subscribedByOthersIcon, converterResult.UriSource.ToString());

            row.Usage = ParameterUsageKind.Subscribed;
            converterResult = (BitmapImage)this.converter.Convert(new object[] { row.ThingStatus, row.Usage }, null, null, null);
            Assert.AreEqual(subscribedIcon, converterResult.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}
