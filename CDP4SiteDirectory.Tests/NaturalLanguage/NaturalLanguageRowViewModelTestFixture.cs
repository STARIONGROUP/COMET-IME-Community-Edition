// -------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageRowViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4SiteDirectory.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;

    using CDP4Common.SiteDirectoryData;

    using CDP4CommonView;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class NaturalLanguageRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
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
            this.messageBus.SendObjectChangeEvent(language, EventKind.Updated);

            Assert.AreEqual(language.Name, row.Name);
        }
    }
}
