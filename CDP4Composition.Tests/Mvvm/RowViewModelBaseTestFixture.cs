// -------------------------------------------------------------------------------------------------
// <copyright file="RowViewModelBaseTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Events;
    using CDP4Composition.MessageBus;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class RowViewModelBaseTestFixture
    {
        private SiteDirectory siteDir;
        private Person person;
        private Mock<IPanelNavigationService> navigation;
        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private ElementDefinition elementDefinition;
        private Category category;
        private BinaryRelationship binaryRelationShip;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.messageBus = new CDPMessageBus();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.session = new Mock<ISession>();
            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, null);
            this.person = new Person(Guid.NewGuid(), this.cache, null);
            this.category = new Category(Guid.NewGuid(), this.cache, null);
            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, null);
            this.elementDefinition.Category.Add(this.category);
            this.binaryRelationShip = new BinaryRelationship(Guid.NewGuid(), this.cache, null);
            this.binaryRelationShip.Source = this.elementDefinition;
            this.binaryRelationShip.Target = this.person;

            this.siteDir.Person.Add(this.person);

            this.navigation = new Mock<IPanelNavigationService>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.cache.TryAdd(new CacheKey(this.siteDir.Iid, null), new Lazy<Thing>(() => this.siteDir));
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatIdataErrorValidationWorks()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.ShortName = "a";
            Assert.That(row["ShortName"], Is.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatValidatePropertyWorks()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.ShortName = "a";
            Assert.That(row.ValidateProperty("ShortName", "a"), Is.Null.Or.Empty);

            Assert.That(row.ValidateProperty("ShortName", "---"), Is.Not.Null.Or.Not.Empty);
        }

        [Test]
        public void VerifyThatUpdatePersonMsgIsCaughtForDirectMessageBusSubscription()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            Assert.That(row.isUpdatePropertyCalled, Is.True);

            row.isUpdatePropertyCalled = false;

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(this.person, 50);

            this.messageBus.SendObjectChangeEvent(this.person, EventKind.Updated);
            Assert.That(row.isUpdatePropertyCalled, Is.True);
        }

        [Test]
        public void VerifyThatUpdatePersonMsgIsCaughtForMessageBusHandler()
        {
            var containerViewModel = new TestMessageBusHandlerContainerViewModel();
            var row = new RowTestClass(this.person, this.session.Object, containerViewModel);
            Assert.That(row.isUpdatePropertyCalled, Is.True);

            row.isUpdatePropertyCalled = false;

            var rev = typeof(Thing).GetProperty("RevisionNumber");
            rev.SetValue(this.person, 50);

            this.messageBus.SendObjectChangeEvent(this.person, EventKind.Updated);
            Assert.That(row.isUpdatePropertyCalled, Is.True);
        }

        [Test]
        public void VerifyThatUpdateHightlightMsgIsCaughtForDirectMessageBusSubscription()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            Assert.That(row.IsHighlighted, Is.False);

            this.messageBus.SendMessage(new HighlightEvent(this.person), this.person);
            Assert.That(row.IsHighlighted, Is.True);
        }

        [Test]
        public void VerifyThatUpdateHighlightMsgIsCaughtForMessageBusHandler()
        {
            var containerViewModel = new TestMessageBusHandlerContainerViewModel();
            var row = new RowTestClass(this.person, this.session.Object, containerViewModel);
            Assert.That(row.IsHighlighted, Is.False);

            this.messageBus.SendMessage(new HighlightEvent(this.person), this.person);
            Assert.That(row.IsHighlighted, Is.False);

            this.messageBus.SendMessage(new HighlightEvent(this.person), null);
            Assert.That(row.IsHighlighted, Is.True);
        }

        [Test]
        public void VerifyThatUpdateHightlightByCategoryMsgIsCaughtForDirectMessageBusSubscription()
        {
            var row = new CategorizableRowTestClass(this.elementDefinition, this.session.Object);
            Assert.That(row.IsHighlighted, Is.False);

            this.messageBus.SendMessage(new HighlightByCategoryEvent(this.category), this.category);
            Assert.That(row.IsHighlighted, Is.False);

            this.messageBus.SendMessage(new HighlightByCategoryEvent(this.category), null);
            Assert.That(row.IsHighlighted, Is.True);
        }

        [Test]
        public void VerifyThatUpdateHighlightByCategoryMsgIsCaughtForMessageBusHandler()
        {
            var containerViewModel = new TestMessageBusHandlerContainerViewModel();
            var row = new CategorizableRowTestClass(this.elementDefinition, this.session.Object, containerViewModel);
            Assert.That(row.IsHighlighted, Is.False);

            this.messageBus.SendMessage(new HighlightByCategoryEvent(this.category), this.category);
            Assert.That(row.IsHighlighted, Is.False);

            this.messageBus.SendMessage(new HighlightByCategoryEvent(this.category), null);
            Assert.That(row.IsHighlighted, Is.True);
        }

        [Test]
        public void VerifyThatUpdateRelationshipMsgIsCaughtForDirectMessageBusSubscription()
        {
            var row = new CategorizableRowTestClass(this.elementDefinition, this.session.Object);
            Assert.That(row.ThingStatusHasChanged, Is.False);

            this.messageBus.SendObjectChangeEvent(this.binaryRelationShip, EventKind.Updated);
            Assert.That(row.ThingStatusHasChanged, Is.True);
        }

        [Test]
        public void VerifyThatUpdateRelationshipMsgIsCaughtForMessageBusHandler()
        {
            var containerViewModel = new TestMessageBusHandlerContainerViewModel();
            var row = new CategorizableRowTestClass(this.elementDefinition, this.session.Object, containerViewModel);
            Assert.That(row.ThingStatusHasChanged, Is.False);

            this.messageBus.SendObjectChangeEvent(this.binaryRelationShip, EventKind.Updated);
            Assert.That(row.ThingStatusHasChanged, Is.True);
        }

        [Test]
        public void VerifyCreateCloneAndWriteWorks()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.CreateCloneAndWrite("abc", "ShortName");

            this.session.Verify(x => x.Write(It.Is<OperationContainer>(op => ((CDP4Common.DTO.Person)op.Operations.Single().ModifiedThing).ShortName == "abc")));
        }

        [Test]
        public void VerifyThatCreateCloneThrows()
        {
            var row = new RowTestClass(this.person, this.session.Object);

            Assert.That(() => row.CreateCloneAndWrite("abc", "Exception"), Throws.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void VerifyThatUpdatePropertyIsCalledOnDalError()
        {
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Throws(new Exception("test"));
            var row = new RowTestClass(this.person, this.session.Object);

            row.isUpdatePropertyCalled = false;
            row.CreateCloneAndWrite("abc", "ShortName");

            Assert.That(row.HasError, Is.True);
            Assert.That(row.ErrorMsg, Is.Not.Null.Or.Not.Empty);

            Assert.That(row.isUpdatePropertyCalled, Is.True);

            row.Dispose();
        }

        [Test]
        public void VerifyThatRowTypeIsCorrect()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            Assert.That("Person", Is.EqualTo(row.RowType));
        }

        [Test]
        public void VerifyThatComputeGenericRowWork()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.ComputeRow(new List<Person> { this.person });
            Assert.That(1, Is.EqualTo(row.ContainedRows.Count));

            row.ComputeRow(new List<Person>());
            Assert.That(0, Is.EqualTo(row.ContainedRows.Count));
        }

        [Test]
        public void VerifyThatCheckAllRowsExpanedWorks()
        {
            var row = new RowTestClass(this.person, this.session.Object);
            row.ComputeRow(new List<Person> { this.person });

            Assert.That(row.AllChildRowsExpanded(), Is.False);

            row.IsExpanded = true;
            row.ContainedRows[0].IsExpanded = true;

            Assert.That(row.AllChildRowsExpanded(), Is.True);
        }

        [Test]
        public void VerifyThatNotThingIsNotEditable()
        {
            var row = new NotThingRowTestClass(new NotThing("NotThing"), this.session.Object, null);

            Assert.That(row.IsEditable("AnyPropertyName"), Is.False);

            this.permissionService.Verify(x => x.CanWrite(It.IsAny<NotThing>()), Times.Never);
        }

        internal class CategorizableRowTestClass : RowViewModelBase<ElementDefinition>
        {
            public bool ThingStatusHasChanged = false;

            internal CategorizableRowTestClass(ElementDefinition elementDefinition, ISession session)
                : base(elementDefinition, session, null)
            {
            }

            internal CategorizableRowTestClass(ElementDefinition elementDefinition, ISession session, IViewModelBase<Thing> containerViewModel)
                : base(elementDefinition, session, containerViewModel)
            {
            }

            protected override void UpdateThingStatus()
            {
                this.ThingStatusHasChanged = true;
            }
        }

        internal class RowTestClass : RowViewModelBase<Person>
        {
            internal RowTestClass(Person person, ISession session)
                : base(person, session, null)
            {
                this.UpdateProperties();
            }

            internal RowTestClass(Person person, ISession session, IViewModelBase<Thing> containerViewModel)
                : base(person, session, containerViewModel)
            {
                this.UpdateProperties();
            }

            public bool isUpdatePropertyCalled { get; set; }

            public string ShortName { get; set; }

            public void ComputeRow(IEnumerable<Person> currentList)
            {
                this.ComputeRows(currentList, this.ComputeTestRow);
            }

            /// <summary>
            /// The object changed event handler
            /// </summary>
            /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
            protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
            {
                base.ObjectChangeEventHandler(objectChange);
                this.UpdateProperties();
            }

            private void UpdateProperties()
            {
                this.isUpdatePropertyCalled = true;
            }

            private void ComputeTestRow(Person person)
            {
                var row = new RowTestClass(person, this.Session);
                this.ContainedRows.Add(row);
            }
        }

        internal class NotThingRowTestClass : RowViewModelBase<NotThing>
        {
            public NotThingRowTestClass(NotThing thing, ISession session, IViewModelBase<Thing> containerViewModel) : base(thing, session, containerViewModel)
            {
            }
        }

        /// <summary>
        /// Implementation of <see cref="IViewModelBase{Thing}"/> and <see cref="IHaveMessageBusHandler"/>
        /// </summary>
        internal class TestMessageBusHandlerContainerViewModel : IViewModelBase<Thing>, IHaveMessageBusHandler
        {
            /// <summary>
            /// The <see cref="MessageBusHandler"/>
            /// </summary>
            public MessageBusHandler MessageBusHandler { get; } = new MessageBusHandler();

            /// <summary>
            /// The <see cref="Thing"/>
            /// </summary>
            public Thing Thing { get; }

            /// <summary name="messageBus">
            /// The <see cref="ICDPMessageBus"/>
            /// </summary>
            public ICDPMessageBus CDPMessageBus { get; }

            /// <summary>
            /// Disposes the instance
            /// </summary>
            public void Dispose()
            {
            }
        }
    }
}
