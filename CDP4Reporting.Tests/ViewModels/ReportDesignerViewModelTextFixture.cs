// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesignerViewModelTextFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4Reporting.Tests.ViewModels
{
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;
    using System;
    using System.Windows;
    using System.Collections.Generic;
    using System.Threading;
    using CDP4Reporting.ViewModels;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using System.Reactive.Concurrency;
    using System.Collections.Concurrent;
    using Microsoft.Practices.ServiceLocation;
    using ICSharpCode.AvalonEdit.Document;
    using System.IO;
    using System.Text;
    using System.IO.Compression;

    /// <summary>
    /// Suite of tests for the <see cref="ReportDesignerViewModel"/> class
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ReportDesignerViewModelTextFixture
    {
        private string filePathOpen = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataSourceOpen.cs");
        private string filePathSave = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataSourceSave.cs");

        private string zipPathOpen = Path.Combine(TestContext.CurrentContext.TestDirectory, "ReportArchiveOpen.rep4");
        private string zipPathSave = Path.Combine(TestContext.CurrentContext.TestDirectory, "ReportArchiveSave.rep4");

        private const string DATASOURCE_CODE = "namespace CDP4Reporting { public class TestDataSource { public TestDataSource(){} }; }";
        private const string DATASOURCE_CODE_NOT_COMPILE = "namespace CDP4Reporting { public class1 TestDataSource { public TestDataSource(){} }; }";
        private const string REPORT_CODE = "<?xml version=\"1.0\" encoding=\"utf-8\"?><XtraReportsLayoutSerializer></XtraReportsLayoutSerializer>";

        private Mock<IServiceLocator> serviceLocator;
        private Mock<ReportDesignerViewModel> reportDesignerViewModel;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Mock<IOpenSaveFileDialogService> openSaveFileDialogService;

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

            this.session = new Mock<ISession>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();
            this.openSaveFileDialogService = new Mock<IOpenSaveFileDialogService>();

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.openSaveFileDialogService.Object);

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

        [TearDown]
        public void TearDown()
        {
            if (System.IO.File.Exists(filePathOpen))
            {
                System.IO.File.Delete(filePathOpen);
            }
            if (System.IO.File.Exists(filePathSave))
            {
                System.IO.File.Delete(filePathSave);
            }
            if (System.IO.File.Exists(zipPathOpen))
            {
                System.IO.File.Delete(zipPathOpen);
            }
            if (System.IO.File.Exists(zipPathSave))
            {
                System.IO.File.Delete(zipPathSave);
            }

            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSaveCommandWorksWithoutSavingFile()
        {
            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(string.Empty);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.SaveScriptCommand.Execute(null));

            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(string.Empty);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.SaveReportCommand.Execute(null));
        }

        [Test]
        public void VerifyThatSaveCommandWorksBySavingFile()
        {
            System.IO.File.WriteAllText(this.filePathSave, DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.ASCII.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(DATASOURCE_CODE));

            using (var zipFile = ZipFile.Open(this.zipPathSave, ZipArchiveMode.Create))
            {
                using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                {
                    reportStream.Position = 0;
                    reportStream.CopyTo(reportEntry);
                }

                using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                {
                    dataSourceStream.Position = 0;
                    dataSourceStream.CopyTo(reportEntry);
                }
            }

            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(this.filePathSave);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.SaveScriptCommand.Execute(null));

            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(this.zipPathSave);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.SaveScriptCommand.Execute(null));
        }

        [Test]
        public void VerifyThatOpenCommandWorksWithoutOpeningFile()
        {
            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.filePathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenScriptCommand.Execute(null));

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenScriptCommand.Execute(null));
        }

        [Test]
        public void VerifyThatOpenCommandWorksByOpeningFile()
        {
            System.IO.File.WriteAllText(this.filePathOpen, DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.ASCII.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(DATASOURCE_CODE));

            using (var zipFile = ZipFile.Open(this.zipPathOpen, ZipArchiveMode.Create))
            {
                using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                {
                    reportStream.Position = 0;
                    reportStream.CopyTo(reportEntry);
                }

                using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                {
                    dataSourceStream.Position = 0;
                    dataSourceStream.CopyTo(reportEntry);
                }
            }
            System.IO.File.Copy(this.zipPathOpen, this.zipPathSave);

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.filePathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenScriptCommand.Execute(null));

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenScriptCommand.Execute(null));
        }

        [Test]
        public void VerifyBuildCommands()
        {
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.BuildScriptCommand.Execute(null));
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.AutomaticBuildCommand.Execute(null));
        }

        [Test]
        public void VerifyThatBuildCommandPass()
        {
            var textDocument = new TextDocument();
            textDocument.Text = DATASOURCE_CODE;

            this.reportDesignerViewModel.Object.Document = textDocument;
            this.reportDesignerViewModel.Object.BuildScriptCommand.Execute(null);

            Assert.AreEqual(0, this.reportDesignerViewModel.Object.BuildResult.Errors.Count);
            Assert.AreEqual(string.Empty, this.reportDesignerViewModel.Object.Errors);
        }

        [Test]
        public void VerifyThatBuildCommandFailed()
        {
            var textDocument = new TextDocument();
            textDocument.Text = DATASOURCE_CODE_NOT_COMPILE;

            this.reportDesignerViewModel.Object.Document = textDocument;
            this.reportDesignerViewModel.Object.BuildScriptCommand.Execute(null);

            Assert.AreNotEqual(0, this.reportDesignerViewModel.Object.BuildResult.Errors.Count);
            Assert.AreNotEqual(string.Empty, this.reportDesignerViewModel.Object.Errors);
        }
    }
}
