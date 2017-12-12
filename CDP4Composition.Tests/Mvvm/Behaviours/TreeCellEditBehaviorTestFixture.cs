// -------------------------------------------------------------------------------------------------
// <copyright file="TreeCellEditBehaviorTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


namespace CDP4Composition.Tests.Mvvm.Behaviours
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;
    using DevExpress.Xpf.Grid;
    using DevExpress.Xpf.Grid.TreeList;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="TreeCellEditBehavior"/> class.
    /// </summary>
    [TestFixture, RequiresSTA]
    public class TreeCellEditBehaviorTestFixture
    {
        private TreeCellEditBehavior cellEditBehaviour;
        private TreeListControl treelistControl;
        private readonly Mock<ISession> session = new Mock<ISession>();
        private TreeListColumn gridColumn;
        private Mock<IPermissionService> permissionService;
        private TestRowViewModel domainRow;

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
            this.domainRow = new TestRowViewModel(domain, this.session.Object, null);
            var domains = new List<TestRowViewModel> { this.domainRow };

            this.treelistControl = new TreeListControl();
            this.gridColumn = new TreeListColumn { FieldName = "Name" };
            this.treelistControl.Columns.Add(this.gridColumn);
            this.treelistControl.ItemsSource = domains;

            this.cellEditBehaviour = new TreeCellEditBehavior();
            this.cellEditBehaviour.Attach(this.treelistControl.View);
        }


        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifySaveCellValueOnCellValueChanged()
        {
            var eventArgs = new TreeListCellValueChangedEventArgs(new TreeListNode(this.domainRow), this.gridColumn, "updated", "test") { RoutedEvent = TreeListView.CellValueChangedEvent };
            this.treelistControl.View.RaiseEvent(eventArgs);

            this.session.Verify(x => x.Write(It.IsAny<OperationContainer>()));
        }
        
        [Test]
        public void VerifyThatDetachingBehaviourDoesNotThrowException()
        {
            Assert.DoesNotThrow(() => this.cellEditBehaviour.Detach());
        }

        private class TestRowViewModel : RowViewModelBase<DomainOfExpertise>
        {
            public TestRowViewModel(DomainOfExpertise domain, ISession session, IViewModelBase<Thing> vmContainer) : base(domain, session, vmContainer)
            {
            }
        }
    }
}
