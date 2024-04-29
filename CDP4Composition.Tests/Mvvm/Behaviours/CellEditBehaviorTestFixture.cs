// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CellEditBehaviorTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Mvvm.Behaviours
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using DevExpress.Xpf.Grid;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CellEditBehavior"/> class.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CellEditBehaviorTestFixture
    {
        private CellEditBehavior cellEditBehaviour;
        private GridControl gridControl;
        private Mock<ISession> session;
        private GridColumn gridColumn;
        private Mock<IPermissionService> permissionService;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.messageBus = new CDPMessageBus();
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var uri = new Uri("http://test.com");
            var domain = new DomainOfExpertise(Guid.NewGuid(), null, uri);
            var siteDir = new SiteDirectory(Guid.NewGuid(), null, uri) { Name = "SiteDir" };
            siteDir.Domain.Add(domain);
            var domainRow = new TestRowViewModel(domain, this.session.Object, null);
            var domains = new List<TestRowViewModel> { domainRow };

            this.gridControl = new GridControl();
            this.gridColumn = new GridColumn { FieldName = "Name" };
            this.gridControl.Columns.Add(this.gridColumn);
            this.gridControl.ItemsSource = domains;

            this.cellEditBehaviour = new CellEditBehavior();
            this.cellEditBehaviour.Attach(this.gridControl.View);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifySaveCellValueOnCellValueChanged()
        {
            var changedEvent = EventManager.RegisterRoutedEvent("CellValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GridControl));
            var eventArgs = new CellValueChangedEventArgs(changedEvent, (GridViewBase)this.gridControl.View, 0, this.gridColumn, "updated", "test") { RoutedEvent = GridViewBase.CellValueChangedEvent };
            this.gridControl.View.RaiseEvent(eventArgs);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }

        private class TestRowViewModel : RowViewModelBase<DomainOfExpertise>
        {
            public TestRowViewModel(DomainOfExpertise domain, ISession session, IViewModelBase<Thing> vmContainer) : base(domain, session, vmContainer)
            {
            }
        }
    }
}
