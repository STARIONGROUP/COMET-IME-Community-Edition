// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelBaseTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Tests.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Events;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    internal class ViewModelBaseTestFixture
    {
        private List<Thing> cache;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.cache = new List<Thing>();
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatDisposeRemoveSubscriptionFromMessageBus()
        {
            var session = new Mock<ISession>();
            var uri = new Uri("http://test.com");
            this.messageBus = new CDPMessageBus();
            var assembler = new Assembler(uri, this.messageBus);

            var pocoPerson = new Person(Guid.NewGuid(), assembler.Cache, uri);

            session.Setup(x => x.Assembler).Returns(assembler);
            session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            var vm = new TestViewModel(pocoPerson, session.Object);

            // workaround to modify a read-only field
            var type = pocoPerson.GetType();
            type.GetProperty("RevisionNumber").SetValue(pocoPerson, 50);
            this.messageBus.SendObjectChangeEvent(pocoPerson, EventKind.Updated);
            Assert.IsTrue(vm.EventCaught);

            vm.EventCaught = false;
            vm.Dispose();
            this.messageBus.SendObjectChangeEvent(pocoPerson, EventKind.Updated);

            vm = null;
            this.messageBus.SendObjectChangeEvent(pocoPerson, EventKind.Added);

            //Assert.IsFalse(vm.EventCaught);
        }

        [Test]
        public void VerifyThatSubscribeToHighlightWorks()
        {
            var eu1 = new CDP4Common.EngineeringModelData.ElementUsage();
            var eu2 = new CDP4Common.EngineeringModelData.ElementUsage();

            // The ViewModel subscribes to events
            this.messageBus.Listen<HighlightEvent>().Subscribe(x => this.OnHighlightEvent(x.HighlightedThing));

            this.messageBus.SendMessage(new HighlightEvent(eu1));
            Assert.AreEqual(1, this.cache.Count);

            this.messageBus.SendMessage(new HighlightEvent(eu2));
            Assert.AreEqual(2, this.cache.Count);
        }

        private void OnHighlightEvent(Thing highlightedThings)
        {
            this.cache.Add(highlightedThings);
        }

        private class TestViewModel : ViewModelBase<Person>
        {
            public bool EventCaught;

            public TestViewModel(Person person, ISession session)
                : base(person, session)
            {
                this.EventCaught = false;

                var thingSubscription = this.Session.CDPMessageBus.Listen<ObjectChangedEvent>(this.Thing)
                    .Where(objectChange => objectChange.EventKind == EventKind.Added && objectChange.ChangedThing.Cache == this.Session.Assembler.Cache)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(this.ObjectChangeEventHandler);
            }

            protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
            {
                base.ObjectChangeEventHandler(objectChange);
                this.EventCaught = true;
            }
        }
    }
}
