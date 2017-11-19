// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReferenceSourceRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRDL.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ReferenceSourceRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class ReferenceSourceRowViewModelTestFixture
    {
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTheConstructorSetsTheProperties()
        {
            var shortname = "referenceSourceshortname";
            var name = "referenceSource name";
            var testSiteRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, this.uri) { ShortName = "testRDL"};
            var organization = new Organization(Guid.NewGuid(), null, this.uri) { ShortName = "Rhea" };
            var versionDate = new DateTime(2016, 8, 6);
            var referenceSource = new ReferenceSource(Guid.NewGuid(), null, this.uri)
                                      {
                                          Name = name,
                                          ShortName = shortname,
                                          Container = testSiteRdl,
                                          Author = "John Doe",
                                          Language = "EU",
                                          PublicationYear = 2016,
                                          Publisher = organization,
                                          VersionIdentifier = "v1",
                                          VersionDate = new DateTime(2016, 8, 6)
                                      };


            testSiteRdl.ReferenceSource.Add(referenceSource);
            var referenceSourceRowViewModel = new ReferenceSourceRowViewModel(referenceSource, this.session.Object, null);

            Assert.AreEqual(shortname, referenceSourceRowViewModel.ShortName);
            Assert.AreEqual(name, referenceSourceRowViewModel.Name);
            Assert.AreEqual("John Doe", referenceSourceRowViewModel.Author);
            Assert.AreEqual("EU", referenceSourceRowViewModel.Language);
            Assert.AreEqual(2016, referenceSourceRowViewModel.PublicationYear);
            Assert.AreEqual("v1", referenceSourceRowViewModel.VersionIdentifier);
            Assert.AreEqual(versionDate, referenceSourceRowViewModel.VersionDate);
            Assert.AreEqual(organization, referenceSourceRowViewModel.Publisher);
            Assert.AreEqual(testSiteRdl.ShortName, referenceSourceRowViewModel.ContainerRdl);
            Assert.AreEqual(0, referenceSourceRowViewModel.ContainedRows.Count);
        }
    }
}
