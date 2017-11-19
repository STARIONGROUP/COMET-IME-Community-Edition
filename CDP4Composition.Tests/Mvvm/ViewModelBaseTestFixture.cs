// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewModelBaseTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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

        [SetUp]
        public void SetUp()
        {
            this.cache = new List<Thing>();
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatDisposeRemoveSubscriptionFromMessageBus()
        {
            var session = new Mock<ISession>();
            var uri = new Uri("http://test.com");
            var assembler = new Assembler(uri);

            var pocoPerson = new Person(Guid.NewGuid(),assembler.Cache,uri);

            session.Setup(x => x.Assembler).Returns(assembler);

            var vm = new TestViewModel(pocoPerson, session.Object);
            // workaround to modify a read-only field
            var type = pocoPerson.GetType();
            type.GetProperty("RevisionNumber").SetValue(pocoPerson, 50);
            CDPMessageBus.Current.SendObjectChangeEvent(pocoPerson, EventKind.Updated);
            Assert.IsTrue(vm.EventCaught);

            vm.EventCaught = false;
            vm.Dispose();
            CDPMessageBus.Current.SendObjectChangeEvent(pocoPerson, EventKind.Updated);

            vm = null;
            CDPMessageBus.Current.SendObjectChangeEvent(pocoPerson, EventKind.Added);
            //Assert.IsFalse(vm.EventCaught);
        }

        [Test]
        public void VerifyThatSubscribeToHighlightWorks()
        {
            var eu1 = new CDP4Common.EngineeringModelData.ElementUsage();
            var eu2 = new CDP4Common.EngineeringModelData.ElementUsage();

            // The ViewModel subscribes to events
            CDPMessageBus.Current.Listen<HighlightEvent>().Subscribe(x => this.OnHighlightEvent(x.HighlightedThing));

            CDPMessageBus.Current.SendMessage(new HighlightEvent(eu1));
            Assert.AreEqual(1, this.cache.Count);

            CDPMessageBus.Current.SendMessage(new HighlightEvent(eu2));
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
                var thingSubscription = CDPMessageBus.Current.Listen<ObjectChangedEvent>(this.Thing)
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