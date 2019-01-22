// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeIconUriConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.Tests.Converters
{
    using CDP4Dal;
    using Moq;
    using System;
    using System.Threading;
    using System.Windows.Media.Imaging;
    using NUnit.Framework;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Windows;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4CommonView;
    using CDP4ProductTree.ViewModels;

    using ParameterRowViewModel = CDP4ProductTree.ViewModels.ParameterRowViewModel;
    using Thing = CDP4Common.CommonData.Thing;

    /// <summary>
    /// suite of tests for the <see cref="ProductTreeIconUriConverter"/>
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
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

        [SetUp]
        public void SetUp()
        {
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

            this.cache.TryAdd(new CacheKey(this.parameter.Iid, null), new Lazy<Thing>(() => this.parameter));
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

            var converterResult = (BitmapImage)this.converter.Convert(new object[] { row }, null, null, null);
            Assert.AreEqual(unusedIcon, converterResult.UriSource.ToString());

            row.Usage = ParameterUsageKind.SubscribedByOthers;
            converterResult = (BitmapImage)this.converter.Convert(new object[] { row }, null, null, null);
            Assert.AreEqual(subscribedByOthersIcon, converterResult.UriSource.ToString());

            row.Usage = ParameterUsageKind.Subscribed;
            converterResult = (BitmapImage)this.converter.Convert(new object[] { row }, null, null, null);
            Assert.AreEqual(subscribedIcon, converterResult.UriSource.ToString());
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}