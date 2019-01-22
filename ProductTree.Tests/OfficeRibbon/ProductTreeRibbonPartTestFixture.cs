// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductTreeRibbonPartTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.Tests.OfficeRibbon
{
    using System;
    using System.Collections.Generic;
    using System.IO.Packaging;
    using System.Reactive.Concurrency;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Permission;
    using Moq;
    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ProductTreeRibbonPart"/> class
    /// </summary>
    [TestFixture, RequiresSTA]
    public class ProductTreeRibbonPartTestFixture
    {

        /// <summary>
        /// The <see cref="RibbonPart"/> under test
        /// </summary>
        private ProductTreeRibbonPart ribbonPart;
        private int amountOfRibbonControls;
        private int order;
        private string ribbonxmlname;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> positiveDialogNavigationService;
        private Mock<IPermissionService> permissionService; 
        private Mock<IDialogResult> negativeDialogResult;
        private Mock<IDialogNavigationService> negativeDialogNavigationService;
        private Mock<ISession> session;
        private Assembler assembler;

        private Uri uri = new Uri("http://test.com");
        private SiteDirectory sitedir;
        private EngineeringModelSetup modelSetup;
        private EngineeringModel model;
        private IterationSetup iterationSetup;
        private Iteration iteration;
        private Person person;
        private Participant participant;
        private DomainOfExpertise domain;

        private Option option1;
        private Option option2;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.SetupRecognizePackUir();
            this.assembler = new Assembler(this.uri);
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            // inputs
            this.sitedir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);
            this.domain.ShortName = "SYS";

            this.participant = new Participant(Guid.NewGuid(), null, this.uri){Person = this.person};
            
            this.sitedir.Model.Add(this.modelSetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri){EngineeringModelSetup = this.modelSetup};
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri){IterationSetup = this.iterationSetup};
            this.option1 = new Option(Guid.NewGuid(), null, this.uri){ShortName = "o1"};
            this.option2 = new Option(Guid.NewGuid(), null, this.uri){ShortName = "o2"};

            this.model.Iteration.Add(this.iteration);
            this.iteration.Option.Add(this.option1);
            this.iteration.Option.Add(this.option2);

            var siteDirectory = new Lazy<Thing>(() => this.sitedir);
            this.assembler.Cache.GetOrAdd(new CacheKey(siteDirectory.Value.Iid, null), siteDirectory);
            this.session.Setup(x => x.DataSourceUri).Returns("test");
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);

            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.amountOfRibbonControls = 4;
            this.order = 1;

            // Setup negative dialog result - selection of iterationsetup
            this.SetupNegativeDialogResult();
            this.positiveDialogNavigationService = new Mock<IDialogNavigationService>();
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });
        }

        /// <summary>
        /// setup the negative dialog result
        /// </summary>
        private void SetupNegativeDialogResult()
        {
            bool? nullnegresult = null;
            this.negativeDialogResult = new Mock<IDialogResult>();
            this.negativeDialogResult.Setup(x => x.Result).Returns(nullnegresult);
            this.negativeDialogNavigationService = new Mock<IDialogNavigationService>();
            this.negativeDialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>()))
                .Returns(this.negativeDialogResult.Object);
        }

        [Test]
        public void VerifyThatIfFluentRibbonIsNotActiveTheSessionEventHasNoEffect()
        {
            this.ribbonPart = new ProductTreeRibbonPart(this.order, this.panelNavigationService.Object, this.negativeDialogNavigationService.Object, null);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = false;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);
            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatIfFluentRibbonIsNullTheSessionEventHasNoEffect()
        {
            this.ribbonPart = new ProductTreeRibbonPart(this.order, this.panelNavigationService.Object, this.negativeDialogNavigationService.Object, null);

            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(sessionEvent);
            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatRibbonPartHandlesSessionOpenAndCloseEvent()
        {
            this.ribbonPart = new ProductTreeRibbonPart(this.order, this.panelNavigationService.Object, this.negativeDialogNavigationService.Object, null);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            Assert.AreEqual(this.session.Object, this.ribbonPart.Session);

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

            Assert.IsNull(this.ribbonPart.Session);
        }

        [Test]
        public void VerifyThatButtonsAreEnabledAsExpected()
        {
            this.ribbonPart = new ProductTreeRibbonPart(this.order, this.panelNavigationService.Object, this.negativeDialogNavigationService.Object, null);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowProductTree_"));
            
            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowProductTree_"));

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);
            Assert.AreEqual(1, this.ribbonPart.Iterations.Count);

            Assert.IsTrue(this.ribbonPart.GetEnabled("ShowProductTree_"));

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Removed);
            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowProductTree_"));

            var closeSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            CDPMessageBus.Current.SendMessage(closeSessionEvent);

            Assert.IsFalse(this.ribbonPart.GetEnabled("ShowProductTree_"));
        }

        [Test]
        public void VerifyThatOnActionProductTreeWorks()
        {
            this.ribbonPart = new ProductTreeRibbonPart(this.order, this.panelNavigationService.Object, this.positiveDialogNavigationService.Object, null);

            var fluentRibbonManager = new FluentRibbonManager();
            fluentRibbonManager.IsActive = true;
            fluentRibbonManager.RegisterRibbonPart(this.ribbonPart);

            var openSessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            CDPMessageBus.Current.SendMessage(openSessionEvent);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Added);
            Assert.AreEqual(1, this.ribbonPart.Iterations.Count);

            var content = this.ribbonPart.GetContent("ShowProductTree_");
            this.ribbonPart.OnAction(string.Format("ShowProductTree_{0}_{1}", this.iteration.Iid, this.option1.Iid), string.Format("{0}_{1}", this.iteration.Iid, this.option1.Iid));

            this.panelNavigationService.Verify(x => x.Open(It.IsAny<IPanelViewModel>(), false));

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Removed);
            Assert.AreEqual(0, this.ribbonPart.Iterations.Count);
        }

        /// <summary>
        /// Pack Uri's are not recognized untill they have been registered in the appl domain
        /// This method makes sure pack Uri's do no throw an exception regarding incorrect port.
        /// </summary>
        private void SetupRecognizePackUir()
        {
            PackUriHelper.Create(new Uri("reliable://0"));
            new FrameworkElement();
            
            try
            {
                Application.ResourceAssembly = typeof(ProductTreeRibbonPart).Assembly;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
