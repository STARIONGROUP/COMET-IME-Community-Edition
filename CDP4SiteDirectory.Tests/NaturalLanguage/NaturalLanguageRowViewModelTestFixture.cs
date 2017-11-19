// -------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    [TestFixture]
    public class NaturalLanguageRowViewModelTestFixture
    {
        private Mock<ISession> session;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var language = new NaturalLanguage(Guid.NewGuid(), null, new Uri("http://test.com"));
            language.Name = "Test";
            language.LanguageCode = "t";
            language.NativeName = "Testa";

            var row = new NaturalLanguageRowViewModel(language, this.session.Object, null);
            Assert.AreEqual(language.Name, row.Name);
            Assert.AreEqual(language.LanguageCode, row.LanguageCode);
            Assert.AreEqual(language.NativeName, row.NativeName);

            language.Name = "update";
            // workaround to modify a read-only field
            var type = language.GetType();
            type.GetProperty("RevisionNumber").SetValue(language, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(language, EventKind.Updated);

            Assert.AreEqual(language.Name, row.Name);
        }
    }
}