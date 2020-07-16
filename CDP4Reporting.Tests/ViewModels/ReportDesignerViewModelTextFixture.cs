using Moq;
using NUnit.Framework;
using ReactiveUI;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CDP4Reporting.ViewModels;
using CDP4Common.CommonData;
using CDP4Common.EngineeringModelData;
using CDP4Common.SiteDirectoryData;
using CDP4Common.Types;
using CDP4Composition.Navigation;
using CDP4Composition.Navigation.Interfaces;
using CDP4Composition.PluginSettingService;
using CDP4Dal;
using CDP4Dal.Permission;
using System.Reactive.Concurrency;
using System.Collections.Concurrent;
using Microsoft.Practices.ServiceLocation;

namespace CDP4Reporting.Tests.ViewModels
{
    /// <summary>
    /// Suite of tests for the <see cref="ReportDesignerViewModel"/> class
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ReportDesignerViewModelTextFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<ReportDesignerViewModel> reportDesignerViewModel;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;

        private static readonly Application application = new Application();

        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Assembler assembler;
        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private DomainOfExpertise domain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.session = new Mock<ISession>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();

            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };

            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.reportDesignerViewModel = new Mock<ReportDesignerViewModel>(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingsService.Object);
        }

        [Test]
        public void VerifyThatCommandsWork()
        {
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenScriptCommand.Execute(null));
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.SaveScriptCommand.Execute(null));
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.BuildScriptCommand.Execute(null));
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.AutomaticBuildCommand.Execute(null));
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }
    }
}
