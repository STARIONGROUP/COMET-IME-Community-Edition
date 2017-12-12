// -------------------------------------------------------------------------------------------------
// <copyright file="CellEditBehaviorTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


namespace CDP4Composition.Tests.Mvvm.Behaviours
{
    using System;
    using System.Collections.Generic;
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
    [TestFixture, RequiresSTA]
    public class CellEditBehaviorTestFixture
    {
        private CellEditBehavior cellEditBehaviour;
        private GridControl gridControl;
        private readonly Mock<ISession> session = new Mock<ISession>();
        private GridColumn gridColumn;
        private Mock<IPermissionService> permissionService;

        [SetUp]
        public void SetUp()
        {
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
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifySaveCellValueOnCellValueChanged()
        {
            var changedEvent = EventManager.RegisterRoutedEvent("CellValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GridControl));
            var eventArgs = new CellValueChangedEventArgs(changedEvent, (GridViewBase)this.gridControl.View, 0, this.gridColumn, "updated", "test") { RoutedEvent = TableView.CellValueChangedEvent };
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
