// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StaticPanelViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Mvvm
{
    using System;
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class EngineeringModelMenuItemGroupViewModelTestFixture
    {
        private Uri uri;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private Mock<ISession> session;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.messageBus = new CDPMessageBus();
            this.uri = new Uri("http://test.be");

            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri) { Name = "model" };
            this.model.EngineeringModelSetup = this.modelSetup;

            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri) { IterationNumber = 5 };
            this.iteration.IterationSetup = this.iterationSetup;

            this.model.Iteration.Add(this.iteration);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void Test()
        {
            var item = new EngineeringModelMenuGroupViewModel(this.iteration, this.session.Object);

            Assert.IsTrue(item.Name.Contains(this.modelSetup.Name));

            this.modelSetup.Name = "model1";

            // workaround to modify a read-only field
            var type = this.model.GetType();
            type.GetProperty("RevisionNumber").SetValue(this.model, 50);
            this.messageBus.SendObjectChangeEvent(this.model, EventKind.Updated);

            Assert.IsTrue(item.Name.Contains(this.modelSetup.Name));
        }
    }
}
