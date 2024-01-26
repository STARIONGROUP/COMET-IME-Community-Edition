// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser.Tests
{
    using System;
    using System.Reactive.Concurrency;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// TestFixture for the <see cref="NaturalLanguageRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class NaturalLanguageRowViewModelTestFixture
    {
        private NaturalLanguage naturalLanguage;

        /// <summary>
        /// The view-model under test
        /// </summary>
        private NaturalLanguageRowViewModel viewModel;
        private string name = "british english";
        private string nativeName = "engb";
        private string languageCode = "en-GB";
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        
        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.naturalLanguage = new NaturalLanguage(Guid.NewGuid(), null, null) { LanguageCode = this.languageCode, NativeName = this.nativeName, Name = this.name };
            this.viewModel = new NaturalLanguageRowViewModel(this.naturalLanguage, this.session.Object, null);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatThePropertiesAreSet()
        {
            Assert.AreEqual(this.name, this.viewModel.Name);
        }

        [Test]
        public void VerifyThatObjectChangeMessageIsProcessed()
        {
            this.name = this.name + "_update";
            this.nativeName = this.nativeName + "_update";
            this.languageCode = this.languageCode + "_update";

            this.naturalLanguage.Name = this.name;
            this.naturalLanguage.NativeName = this.nativeName;
            this.naturalLanguage.LanguageCode = this.languageCode;

            // workaround to modify a read-only field
            var type = this.naturalLanguage.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.naturalLanguage, 50);
            var objectChangedEvent = new ObjectChangedEvent(this.naturalLanguage, EventKind.Updated);
            this.messageBus.SendMessage(objectChangedEvent, this.naturalLanguage, null);

            Assert.AreEqual(this.name, this.viewModel.Name);
        }
    }
}