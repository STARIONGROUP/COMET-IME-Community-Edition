// -------------------------------------------------------------------------------------------------
// <copyright file=DashboardBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.ViewModels.Rows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4Dashboard.ViewModels;
    using CDP4Dashboard.ViewModels.Widget;
    using CDP4Dashboard.Views.Widget;

    using DevExpress.Xpf.LayoutControl;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    using Parameter = CDP4Common.EngineeringModelData.Parameter;

    /// <summary>
    /// Tests if <see cref="DashboardBrowserViewModel"/> works set correctly
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    internal class DashboardBrowserViewModelTestFixture
    {
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;

        private SiteDirectory sitedir;
        private EngineeringModel model;
        private Person person;
        private List<Thing> cache;

        private EngineeringModelSetup engineeringModelSetup;
        private ModelReferenceDataLibrary mrdl;
        private SiteReferenceDataLibrary srdl;
        private Iteration iteration;
        private ElementDefinition elementDef;
        private Participant participant;
        private Uri uri;
        private DomainOfExpertise domain;
        private Mock<ISession> session;
        private Assembler assembler;
        private Mock<IPermissionService> permissionService;
        private TextParameterType pt;
        private PropertyInfo rev = typeof(Thing).GetProperty("RevisionNumber");

        [SetUp]
        public void Setup()
        {
            this.cache = new List<Thing>();
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
            this.assembler = new Assembler(this.uri);
            this.panelNavigationService = new Mock<IPanelNavigationService>();

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.pt = new TextParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.Person.Add(person);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "TestDoE" };
            this.sitedir.Domain.Add(this.domain);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.elementDef = new ElementDefinition(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "1" };
            this.elementDef.Owner = this.domain;
            this.elementDef.Container = this.iteration;
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.participant.Domain.Add(this.domain);

            var parameter = new Parameter(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Container = this.elementDef,
                ParameterType = this.pt,
                Owner = this.elementDef.Owner
            };

            var parameterOverride = new ParameterOverride(Guid.NewGuid(), this.assembler.Cache, this.uri) { Owner = this.domain, Parameter = parameter, Container = this.elementDef };
            var elementUsage = new ElementUsage(Guid.NewGuid(), this.assembler.Cache, this.uri) { ElementDefinition = this.elementDef };
            elementUsage.ParameterOverride.Add(parameterOverride);
            elementUsage.Container = this.iteration;
            this.elementDef.ContainedElement.Add(elementUsage);

            this.model.Iteration.Add(this.iteration);

            var iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iteration.IterationSetup = iterationSetup;

            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "ModelSetup" };
            this.engineeringModelSetup.IterationSetup.Add(iterationSetup);

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.srdl.ParameterType.Add(this.pt);

            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl.RequiredRdl = this.srdl;
            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl);

            this.model.EngineeringModelSetup = this.engineeringModelSetup;
            this.model.EngineeringModelSetup.Participant.Add(this.participant);
            this.permissionService = new Mock<IPermissionService>();

            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.assembler.Cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));

            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<IterationTrackParameterDetailViewModel>())).Returns(new BaseDialogResult(true));
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatDragOverWorksWithParameterOverride()
        {
            var vm = new DashboardBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDef.ContainedElement.First().ParameterOverride.First());

            vm.DragOver(dropinfo.Object);

            dropinfo.VerifySet(x => x.Effects = DragDropEffects.Copy);
        }

        [Test]
        public void VerifyThatDragOverWorksWithParameter()
        {
            var vm = new DashboardBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDef.ContainedElement.First().ParameterOverride.First().Parameter);

            vm.DragOver(dropinfo.Object);

            dropinfo.VerifySet(x => x.Effects = DragDropEffects.Copy);
        }

        [Test]
        public async Task VerifyThatDropWorksWithParameter()
        {
            var vm = new DashboardBrowserViewModel(this.iteration, this.session.Object, null, null, this.dialogNavigationService.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDef.ContainedElement.First().ParameterOverride.First().Parameter);

            Assert.AreEqual(1, vm.Widgets.Count);

            await vm.Drop(dropinfo.Object);

            Assert.AreEqual(2, vm.Widgets.Count);
        }

        [Test]
        public async Task VerifyThatDropWorksWithParameterOverride()
        {
            var vm = new DashboardBrowserViewModel(this.iteration, this.session.Object, null, null, this.dialogNavigationService.Object, null);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDef.ContainedElement.First().ParameterOverride.First());

            Assert.AreEqual(1, vm.Widgets.Count);

            await vm.Drop(dropinfo.Object);

            Assert.AreEqual(2, vm.Widgets.Count);
        }

        [Test]
        public void VerifyThatDummyWidgetIsAddedAutomatically()
        {
            var vm = new DashboardBrowserViewModel(this.iteration, this.session.Object, null, null, this.dialogNavigationService.Object, null);

            Assert.AreEqual(1, vm.Widgets.Count);
            Assert.IsInstanceOf<DummyParameterView>(vm.Widgets.First());
        }

        [Test]
        public async Task VerifyThatChartIsSizedCorrectly()
        {
            var vm = new DashboardBrowserViewModel(this.iteration, this.session.Object, null, null, this.dialogNavigationService.Object, null);

            vm.ActualWidth = 1024;
            vm.ActualHeight = 768;

            //Initial, no Widgets
            Assert.AreEqual(vm.MainOrientation, Orientation.Vertical);
            Assert.AreEqual(vm.MaximizedElementPosition, MaximizedElementPosition.Top);

            var dropinfo = new Mock<IDropInfo>();
            dropinfo.Setup(x => x.Payload).Returns(this.elementDef.ContainedElement.First().ParameterOverride.First().Parameter);

            //Add widget
            await vm.Drop(dropinfo.Object);

            Assert.AreEqual(2, vm.Widgets.Count);

            //Set Widget maximized in LandScape
            var widget = vm.Widgets.OfType<IterationTrackParameterView>().First();
            var iterationTrackParameterViewModel = widget.DataContext as IterationTrackParameterViewModel<Parameter, ParameterValueSet>;
            iterationTrackParameterViewModel.ChartVisible = Visibility.Visible;

            Assert.AreEqual(vm.MainOrientation, Orientation.Horizontal);
            Assert.AreEqual(vm.MaximizedElementPosition, MaximizedElementPosition.Left);

            Assert.AreEqual(718, vm.MaximizedElement.Height);
            Assert.AreEqual(724, vm.MaximizedElement.Width);

            //Set Portrait mode
            vm.ActualWidth = 768;
            vm.ActualHeight = 1024;

            Assert.AreEqual(vm.MainOrientation, Orientation.Vertical);
            Assert.AreEqual(vm.MaximizedElementPosition, MaximizedElementPosition.Top);

            Assert.AreEqual(774, vm.MaximizedElement.Height);
            Assert.AreEqual(718, vm.MaximizedElement.Width);

            //Add another Widget
            var dropinfo2 = new Mock<IDropInfo>();
            dropinfo2.Setup(x => x.Payload).Returns(this.elementDef.ContainedElement.First().ParameterOverride.First());

            await vm.Drop(dropinfo2.Object);

            Assert.AreEqual(3, vm.Widgets.Count);

            //Maximize the new widget's chart
            var widget2 = vm.Widgets.OfType<IterationTrackParameterView>().First(x => x.DataContext is IterationTrackParameterViewModel<ParameterOverride, ParameterOverrideValueSet>);
            var iterationTrackParameterViewModel2 = widget2.DataContext as IterationTrackParameterViewModel<ParameterOverride, ParameterOverrideValueSet>;
            iterationTrackParameterViewModel2.ChartVisible = Visibility.Visible;

            Assert.AreEqual(widget2, vm.MaximizedElement);

            Assert.AreEqual(200, widget.Height);
            Assert.AreEqual(250, widget.Width);

            Assert.AreEqual(774, widget2.Height);
            Assert.AreEqual(718, widget2.Width);


        }
    }
}
