// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Tests
{
    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NLog;
    using NLog.Config;

    using NUnit.Framework;

    using ReactiveUI;

    using System;
    using System.Reactive.Concurrency;
    using System.Collections.Generic;
    
    using CDP4Composition.Navigation;
    
    using CDP4Dal;
    using CDP4Dal.Composition;
    using CDP4Dal.DAL;
    using CDP4Dal.Events;

    using CDP4IME;
    using CDP4IME.ViewModels;
    using CDP4IME.Settings;

    using CDP4ShellDialogs.ViewModels;
    
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// TestFixture for the <see cref="ShellViewModel"/>
    /// </summary>
    [TestFixture]
    public class ShellViewModelTestFixture
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// the view-model under test
        /// </summary>
        private ShellViewModel viewModel;
        
        /// <summary>
        /// mocked <see cref="IDialogNavigationService"/>
        /// </summary>
        private Mock<IDialogNavigationService> navigationService;

        /// <summary>
        /// mocked <see cref="IServiceLocator"/>
        /// </summary>
        private Mock<IServiceLocator> serviceLocator;

        /// <summary>
        /// mocked <see cref="ISession"/>
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// mocked <see cref="Iteration"/>
        /// </summary>
        private Iteration iteration;

        /// <summary>
        /// mocked <see cref="DomainOfExpertise"/>
        /// </summary>
        private DomainOfExpertise domain;

        private Uri uri = new Uri("http://www.rheagroup.com");

        private IterationSetup iterationSetup;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            
            LogManager.Configuration = new LoggingConfiguration();

            this.navigationService = new Mock<IDialogNavigationService>();

            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);

            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri) { IterationSetup = this.iterationSetup };

            this.session = new Mock<ISession>();
            
            this.session.Setup(x=>x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                {this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null)}
            });

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            var dals = new List<Lazy<IDal, IDalMetaData>>();
            var availableDals = new AvailableDals(dals);
            this.serviceLocator.Setup(x => x.GetInstance<AvailableDals>()).Returns(availableDals);

            this.viewModel = new ShellViewModel(this.navigationService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            LogManager.Configuration = null;
            this.serviceLocator = null;
            this.viewModel = null;
        }

        [Test]
        public void VerifyThatArgumentNullExceptionIsThrown()
        {
            Assert.Throws<ArgumentNullException>(() => new ShellViewModel(null));
        }

        [Test]
        public void VerifyThatTheCaptionIsCorrect()
        {
            var title = "COMET IME - Community Edition";
            Assert.AreEqual(title, this.viewModel.Title);
        }

        [Test]
        public void VerifyThatLogAreCaught()
        {
            logger.Log(LogLevel.Info, "test");

            var log = this.viewModel.LogEventInfo;
            Assert.AreEqual("test", log.Message);
        }

        [Test]
        public void VerifyThatLogDetailCommandWorks()
        {
            this.navigationService.Setup(x => x.NavigateModal(It.IsAny<LogDetailsViewModel>())).Returns(null as IDialogResult);

            this.viewModel.OpenLogDialogCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<LogDetailsViewModel>()));
        }

        [Test]
        public void VerifyThatOpenAboutCommandWorks()
        {
            Assert.DoesNotThrow(() => this.viewModel.OpenAboutCommand.Execute(null));
        }

        [Test]
        public void Verify_that_OpenProxyConfigurationCommand_can_be_executed()
        {
            Assert.DoesNotThrow(() => this.viewModel.OpenProxyConfigurationCommand.Execute(null));
        }

        [Test]
        public void VerifyThatOpenDataSourceCommandExecutesAndIfCancelledNoSessionLoaded()
        {
            this.navigationService.Setup(x => x.NavigateModal(It.IsAny<DataSourceSelectionViewModel>())).Returns(null as DataSourceSelectionResult);
            this.viewModel.OpenDataSourceCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<DataSourceSelectionViewModel>()));

            CollectionAssert.IsEmpty(this.viewModel.Sessions);
        }

        [Test]
        public void VerifyThatOpenAPluginManagerCommandNavigatesToPluginWindow()
        {
            this.viewModel.OpenPluginManagerCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<PluginManagerViewModel<ImeAppSettings>>()));
        }

        [Test]
        public void VerifyThatOpenDataSourceCommandExecutesAndSessionIsLoaded()
        {
            Assert.IsFalse(this.viewModel.HasSessions);

            var mockedSession = new Mock<ISession>();
            var selectionResult = new DataSourceSelectionResult(true, mockedSession.Object);

            this.navigationService.Setup(x => x.NavigateModal(It.IsAny<DataSourceSelectionViewModel>())).Returns(selectionResult);
            this.viewModel.OpenDataSourceCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<DataSourceSelectionViewModel>()));

            CollectionAssert.IsNotEmpty(this.viewModel.Sessions);

            Assert.IsTrue(this.viewModel.HasSessions);

            Assert.AreEqual(mockedSession.Object, this.viewModel.SelectedSession.Session);
        }

        [Test]
        public void VerifyThatExecuteOpenAboutRequestNavigatesToAboutWindow()
        {
            this.viewModel.OpenAboutCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<AboutViewModel>()));
        }

        [Test]
        public void VerifThatExecuteOpenLogDialogCommandNavigatesToLogWindow()
        {
            this.viewModel.OpenLogDialogCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<LogDetailsViewModel>()));
        }

        [Test]
        public void VerifThatSaveSessionCommandNavigatesToDataSourceExportViewModel()
        {
            this.viewModel.SaveSessionCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<DataSourceExportViewModel>()));
        }

        [Test]
        public void VerifyThatAOpenUriCommandCanBeExecuted()
        {
            Assert.IsTrue(this.viewModel.OpenUriManagerCommand.CanExecute(null));

            this.viewModel.OpenUriManagerCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<UriManagerViewModel>()));
        }

        [Test]
        public void VerifyThatOpenModelSelectionOpensDialog()
        {
            CDPMessageBus.Current.SendMessage<SessionEvent>(new SessionEvent(this.session.Object, SessionStatus.Open));

            this.navigationService.Setup(x => x.NavigateModal(It.IsAny<ModelOpeningDialogViewModel>())).Returns(null as IDialogResult);

            this.viewModel.OpenSelectIterationsCommand.Execute(null);
        }

        [Test]
        public void VerifyThatSessionArePopulated()
        {
            CDPMessageBus.Current.SendMessage<SessionEvent>(new SessionEvent(this.session.Object, SessionStatus.Open));

            Assert.AreEqual(1, this.viewModel.OpenSessions.Count);

            CDPMessageBus.Current.SendMessage<SessionEvent>(new SessionEvent(this.session.Object, SessionStatus.Closed));
            Assert.AreEqual(0, this.viewModel.OpenSessions.Count);
        }
        
        [Test]
        public void VerifyCheckForUpdateCommand()
        {
            this.navigationService.Setup(x => x.NavigateModal(It.IsAny<UpdateDownloaderInstallerViewModel>())).Returns(null as IDialogResult);

            this.viewModel.CheckForUpdateCommand.Execute(null);
            this.navigationService.Verify(x => x.NavigateModal(It.IsAny<UpdateDownloaderInstallerViewModel>()));
        }
    }
}