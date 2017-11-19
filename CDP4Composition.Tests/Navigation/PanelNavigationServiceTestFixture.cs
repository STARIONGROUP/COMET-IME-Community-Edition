// -------------------------------------------------------------------------------------------------
// <copyright file="PanelNavigationServiceTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Navigation
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Tests.ViewModels;
    using CDP4Composition.Tests.Views;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4PropertyGrid.ViewModels;
    using CDP4PropertyGrid.Views;
    using Microsoft.Practices.Prism.Regions;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using CDP4Dal.Composition;

    [TestFixture]
    public class PanelNavigationServiceTestFixture
    {
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService; 
        private Mock<IRegionManager> regionManager;
        private Mock<IRegion> region;
        private Mock<IViewsCollection> viewsCollection;

        private IPanelView panelView;

        private IPanelViewModel panelViewModel;

        private IPanelViewModel panelViewModel2;

        private List<Lazy<IPanelView, IRegionMetaData>> viewList;

        private List<Lazy<IPanelViewModel, INameMetaData>> viewModelDecoratedList;

        private List<IPanelViewModel> viewModelList;

        private Mock<IRegionMetaData> metadata;

        private Mock<INameMetaData> describeMetaData;



        private PanelNavigationService NavigationService;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.regionManager = new Mock<IRegionManager>();
            this.region = new Mock<IRegion>();
            this.metadata = new Mock<IRegionMetaData>();
            this.viewsCollection = new Mock<IViewsCollection>();

            this.describeMetaData = new Mock<INameMetaData>();
            this.describeMetaData.Setup(x => x.Name).Returns("MockedPanelDecorated");

            this.regionManager.Setup(x => x.Regions[It.IsAny<string>()]).Returns(this.region.Object);
            this.region.Setup(x => x.Views).Returns(this.viewsCollection.Object);

            this.panelView = new Test(true);
            this.panelViewModel = new TestViewModel();
            this.panelViewModel2 = new TestViewModel("data source");

            this.viewList = new List<Lazy<IPanelView, IRegionMetaData>>();
            this.viewList.Add(new Lazy<IPanelView, IRegionMetaData>(() => this.panelView, this.metadata.Object));
            this.viewList.Add(new Lazy<IPanelView, IRegionMetaData>(() => new PropertyGrid(), this.metadata.Object));

            this.viewModelDecoratedList = new List<Lazy<IPanelViewModel, INameMetaData>>();
            this.viewModelDecoratedList.Add(new Lazy<IPanelViewModel, INameMetaData>(() => this.panelViewModel2, this.describeMetaData.Object));

            this.viewModelList = new List<IPanelViewModel>();
            this.viewModelList.Add(this.panelViewModel);
            this.viewModelList.Add(new PropertyGridViewModel());

            this.NavigationService = new PanelNavigationService(this.viewList, this.viewModelList, this.regionManager.Object, this.viewModelDecoratedList);

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatOpenViewModelWorks()
        {
            this.NavigationService.Open(this.panelViewModel, true);
            this.region.Verify(x => x.Add(It.IsAny<object>(), It.IsAny<string>()));
        }

        [Test]
        public void VerifyThatOpenThingPropertyWorks()
        {
            var person = new Person(Guid.NewGuid(), null, null);
            this.NavigationService.Open(new PropertyGridViewModel(), true);
            this.NavigationService.Open(person, this.session.Object);
            Assert.AreEqual(1, this.NavigationService.ViewModelViewPairs.Count);
            var vm = this.NavigationService.ViewModelViewPairs.Keys.Single() as PropertyGridViewModel;

            Assert.IsNotNull(vm);
            Assert.AreEqual(person, vm.Thing);
        }

        [Test]
        public void VerifyThatOpenViewModelByNameWorks()
        {
            this.NavigationService.Open(this.describeMetaData.Object.Name, this.session.Object, true, 
                this.thingDialogNavigationService.Object, this.dialogNavigationService.Object);

            this.region.Verify(x => x.Add(It.IsAny<object>(), It.IsAny<string>()));
        }

        [Test]
        public void VerifyThatCloseViewModelWorks()
        {
            this.NavigationService.Open(this.panelViewModel, true);
            this.NavigationService.Close(this.panelViewModel, true);

            this.region.Verify(x => x.Remove(It.IsAny<object>()));
        }

        [Test]
        public void VerifyThatCloseAllPanelTypeWorks()
        {
            this.NavigationService.Open(new PropertyGridViewModel(), true);

            Assert.AreEqual(1, this.NavigationService.ViewModelViewPairs.Count);

            this.NavigationService.Close(typeof(PropertyGridViewModel));
            this.region.Verify(x => x.Remove(It.IsAny<object>()));
        }

        [Test]
        public void VerifyThatEventHandlerIsTriggered()
        {
            var closed = false;

            CDPMessageBus.Current.Listen<NavigationPanelEvent>()
                .Where(x => x.ViewModel == this.panelViewModel && x.PanelStatus == PanelStatus.Closed)
                .Subscribe(x => { closed = true; });

            this.NavigationService.Open(this.panelViewModel, true);
            var olditems = new List<IPanelView>() { this.NavigationService.ViewModelViewPairs.Values.Single() };

            this.viewsCollection.Raise(x => x.CollectionChanged += null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, olditems));
            Assert.IsTrue(closed);
            Assert.AreEqual(0, this.NavigationService.ViewModelViewPairs.Count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyThatNavigationServiceThrowsException()
        {
            this.NavigationService.Open(new ExceptionViewModel(), true);
        }

        [Test]
        public void VerifyThatNavigationServiceDoesNotThrowWhenPropertyGridNotFound()
        {
            this.NavigationService = new PanelNavigationService(new List<Lazy<IPanelView, IRegionMetaData>>(), new List<IPanelViewModel>(),
                regionManager.Object, new List<Lazy<IPanelViewModel, INameMetaData>>());

            Assert.DoesNotThrow(() => this.NavigationService.Open(new Person(Guid.NewGuid(), null, null), this.session.Object));
        }
         
        [Test]
        public void VerifyThatCloseDataSourceWorks()
        {
            this.NavigationService.Open(new TestViewModel("uri"), true);
            this.NavigationService.Open(new TestViewModel("uri"), true);
            this.NavigationService.Open(new TestViewModel("alalala"), true);

            Assert.AreEqual(3, this.NavigationService.ViewModelViewPairs.Count);

            this.NavigationService.Close("uri");
            this.region.Verify(x => x.Remove(It.IsAny<IPanelView>()), Times.Exactly(2));
        }
    }
}

namespace CDP4Composition.Tests.Views
{
    public class Test : IPanelView
    {
        public Test()
        {
        }

        public Test(bool initializeComponent)
        {
        }

        public object DataContext { get; set; }
    }
}

namespace CDP4Composition.Tests.ViewModels
{
    using System;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;

    public class TestViewModel : IPanelViewModel
    {
        public TestViewModel(string uri = default(string))
        {
            this.Identifier = Guid.NewGuid();
            this.DataSource = uri;
        }

        public TestViewModel(ISession session, SiteDirectory siteDir, IThingDialogNavigationService thingDialogNavigationService,
            IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
        {
            this.Session = session;
            this.SiteDirectory = siteDir;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.PanelNavigationService = panelNavigationService;
            this.DialogNavigationService = dialogNavigationService;
        }

        public string Caption { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        public Guid Identifier { get; private set; }

        public string ToolTip { get; private set; }
        public string DataSource { get; private set; }
        public ISession Session { get; private set;}
        public SiteDirectory SiteDirectory { get; private set; }
        public IThingDialogNavigationService ThingDialogNavigationService { get; private set; }
        public IPanelNavigationService PanelNavigationService { get; private set; }
        public IDialogNavigationService DialogNavigationService { get; private set; }
        public void Dispose()
        {
        }
    }

    public class ExceptionViewModel : IPanelViewModel
    {
        public ExceptionViewModel(string uri = default(string))
        {
            this.Identifier = Guid.NewGuid();
            this.DataSource = uri;
        }

        public string Caption { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        public Guid Identifier { get; private set; }

        public string ToolTip { get; private set; }
        public string DataSource { get; private set; }
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}

namespace CDP4PropertyGrid.ViewModels
{
    using System;

    using CDP4Common.CommonData;
    using CDP4Composition;

    using CDP4Dal;

    public class PropertyGridViewModel : IPanelViewModel
    {
        public PropertyGridViewModel()
        {
            this.Identifier = Guid.NewGuid();
        }

        public PropertyGridViewModel(Thing thing, ISession session)
        {
            this.Thing = thing;
        }

        /// <summary>
        /// Gets a value indicating whether this is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return false; }
        }

        public Thing Thing { get; private set; }

        public string Caption { get; private set; }

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        public Guid Identifier { get; private set; }

        public string ToolTip { get; private set; }
        public string DataSource { get; private set; }
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}

namespace CDP4PropertyGrid.Views
{
    using CDP4Composition;

    public class PropertyGrid : IPanelView
    {
        public PropertyGrid()
        {
        }

        public PropertyGrid(bool initializeComponent)
        {
        }

        public object DataContext { get; set; }
    }
}