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
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Reactive.Concurrency;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CDP4Reporting.ViewModels;

    using ICSharpCode.AvalonEdit.Document;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ReportDesignerViewModel"/> class
    /// </summary>
    [TestFixture]
    public class ReportDesignerViewModelTextFixture
    {
        private readonly string dsPathOpen = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataSourceOpen.cs");
        private readonly string dsPathSave = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataSourceSave.cs");

        private readonly string zipPathOpen = Path.Combine(TestContext.CurrentContext.TestDirectory, "ReportArchiveOpen.rep4");
        private readonly string zipPathSave = Path.Combine(TestContext.CurrentContext.TestDirectory, "ReportArchiveSave.rep4");

        private const string DATASOURCE_CODE = @"namespace CDP4Reporting
        {
            using CDP4Reporting.DataCollection;
            public class TestDataSource : DataCollector
            {
                public TestDataSource()
                {
                }

                public override object CreateDataObject()
                {
                    return null;
                }
            };
        }";

        private const string REBUILD_ERROR_DATASOURCE_CODE = @"namespace CDP4Reporting
        {
            using CDP4Reporting.DataCollection;
            using System;
            
            public class TestDataSource : DataCollector
            {
                public TestDataSource()
                {
                }

                public override object CreateDataObject()
                {
                    throw new Exception(""REBUILD_FAILED"");
                }
            };
        }";

        private const string DATASOURCE_CODE_WITH_PARAMS = @"namespace CDP4Reporting
        {
            using CDP4Reporting.DataCollection;
	        using CDP4Reporting.Parameters;
	        using System.Collections.Generic;

	        public class TestReportingParameters: ReportingParameters
	        {
		        public override IEnumerable<IReportingParameter> CreateParameters(object dataObject)
		        {
                    List<IReportingParameter> paramsList = new List<IReportingParameter>();
                    paramsList.Add(new ReportingParameter(""param1"", typeof(int), 0));
                    paramsList.Add(new ReportingParameter(""param2"", typeof(string), string.Empty));
                    paramsList[1].AddLookupValue(""1"", ""one"");
                    paramsList.Add(new ReportingParameter(""param3"", typeof(string), ""DefaultValue""));
                    paramsList[2].Visible = false;
                    paramsList.Add(new ReportingParameter(""param3"", typeof(string), string.Empty));
                    paramsList[3].Visible = false;

			        return paramsList;
		        }
            }
            public class TestDataSource : DataCollector
            {
                public TestDataSource()
                {
                }

                public override object CreateDataObject()
                {
                    return null;
                }
            }
        }";

        private const string DATASOURCE_CODE_INVALID = @"namespace CDP4Reporting
        {
            using CDP4Reporting.DataCollection;
            public1 class TestDataSource : ReportingDataSource
            {
                public TestDataSource()
                {
                }

                public override object DataCollector()
                {
                    return null;
                }
            };
        }";

        private const string REPORT_CODE = @"<?xml version=""1.0"" encoding=""utf-8""?>
            <XtraReportsLayoutSerializer ControlType=""DevExpress.XtraReports.UI.XtraReport""><Bands></Bands>
        </XtraReportsLayoutSerializer>";

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
            this.CleanupExistingFiles();

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
            this.CleanupExistingFiles();

            CDPMessageBus.Current.ClearSubscriptions();
        }

        private void CleanupExistingFiles()
        {
            if (System.IO.File.Exists(this.dsPathOpen))
            {
                System.IO.File.Delete(this.dsPathOpen);
            }

            if (System.IO.File.Exists(this.dsPathSave))
            {
                System.IO.File.Delete(this.dsPathSave);
            }

            if (System.IO.File.Exists(this.zipPathOpen))
            {
                System.IO.File.Delete(this.zipPathOpen);
            }

            if (System.IO.File.Exists(this.zipPathSave))
            {
                System.IO.File.Delete(this.zipPathSave);
            }
        }

        [Test]
        public void VerifyThatExportCommandWorksWithoutSavingFile()
        {
            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(string.Empty);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.ExportScriptCommand.Execute(null));
            Assert.AreEqual(null, this.reportDesignerViewModel.Object.CodeFilePath);

            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(string.Empty);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.SaveReportCommand.Execute(null));
            Assert.AreEqual(null, this.reportDesignerViewModel.Object.CurrentReportProjectFilePath);
        }

        [Test]
        public async Task VerifySavingExistingReportWorks()
        {
            var reportStream = new MemoryStream(Encoding.ASCII.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(DATASOURCE_CODE));

            using (var zipFile = ZipFile.Open(this.zipPathSave, ZipArchiveMode.Create))
            {
                using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                {
                    reportStream.Position = 0;
                    await reportStream.CopyToAsync(reportEntry);
                }

                using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                {
                    dataSourceStream.Position = 0;
                    await dataSourceStream.CopyToAsync(reportEntry);
                }
            }

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathSave });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenReportCommand.Execute(null));

            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.SaveReportCommand.Execute(null));
            Assert.AreEqual(this.zipPathSave, this.reportDesignerViewModel.Object.CurrentReportProjectFilePath);
        }

        [Test]
        public async Task VerifyThatExportCommandWorksBySavingFile()
        {
            System.IO.File.WriteAllText(this.dsPathSave, DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.ASCII.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(DATASOURCE_CODE));

            using (var zipFile = ZipFile.Open(this.zipPathSave, ZipArchiveMode.Create))
            {
                using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                {
                    reportStream.Position = 0;
                    await reportStream.CopyToAsync(reportEntry);
                }

                using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                {
                    dataSourceStream.Position = 0;
                    await dataSourceStream.CopyToAsync(reportEntry);
                }
            }

            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(this.dsPathSave);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.ExportScriptCommand.Execute(null));

            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(this.zipPathSave);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.SaveReportCommand.Execute(null));
        }

        [Test]
        public void VerifyThatImportCommandWorksWithoutOpeningFile()
        {
            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.ImportScriptCommand.Execute(null));
            Assert.AreEqual(null, this.reportDesignerViewModel.Object.CodeFilePath);

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenReportCommand.Execute(null));
            Assert.AreEqual(null, this.reportDesignerViewModel.Object.CurrentReportProjectFilePath);
        }

        [Test]
        public async Task VerifyThatImportCommandWorksByOpeningFile()
        {
            System.IO.File.WriteAllText(this.dsPathOpen, DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.ASCII.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(DATASOURCE_CODE));

            using (var zipFile = ZipFile.Open(this.zipPathOpen, ZipArchiveMode.Create))
            {
                using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                {
                    reportStream.Position = 0;
                    await reportStream.CopyToAsync(reportEntry);
                }

                using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                {
                    dataSourceStream.Position = 0;
                    await dataSourceStream.CopyToAsync(reportEntry);
                }
            }

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.dsPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.ImportScriptCommand.Execute(null));

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenReportCommand.Execute(null));
        }

        [Test]
        public void VerifyThatCompileScriptCommandWorks()
        {
            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.Object.CompileScriptCommand.ExecuteAsyncTask(null));
        }

        [Test]
        public async Task VerifyThatCompileScriptCommandPass()
        {
            var textDocument = new TextDocument
            {
                Text = DATASOURCE_CODE
            };

            this.reportDesignerViewModel.Object.Document = textDocument;
            await this.reportDesignerViewModel.Object.CompileScriptCommand.ExecuteAsyncTask(null);

            Assert.AreEqual(0, this.reportDesignerViewModel.Object.CompileResult.Errors.Count);
            Assert.AreEqual(string.Empty, this.reportDesignerViewModel.Object.Errors);
        }

        [Test]
        public async Task VerifyThatCompileScriptCommandFailed()
        {
            var textDocument = new TextDocument
            {
                Text = DATASOURCE_CODE_INVALID
            };

            this.reportDesignerViewModel.Object.Document = textDocument;
            await this.reportDesignerViewModel.Object.CompileScriptCommand.ExecuteAsyncTask(null);

            Assert.AreNotEqual(0, this.reportDesignerViewModel.Object.CompileResult.Errors.Count);
            Assert.AreNotEqual(string.Empty, this.reportDesignerViewModel.Object.Errors);
        }

        [Test]
        public void VerifyThatNewReportCommandWorks()
        {
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.NewReportCommand.Execute(null));
            Assert.AreEqual(string.Empty, this.reportDesignerViewModel.Object.Document.Text);
        }

        [Test]
        public void VerifyThatNewReportCommandWorksWithSwitchReport()
        {
            var textDocument = new TextDocument
            {
                Text = DATASOURCE_CODE
            };

            this.reportDesignerViewModel.Object.Document = textDocument;

            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.NewReportCommand.Execute(null));
            Assert.AreEqual(DATASOURCE_CODE, this.reportDesignerViewModel.Object.Document.Text);
        }

        [Test]
        public void VerifyThatAutoCompileScriptWorks()
        {
            Assert.AreEqual(false, this.reportDesignerViewModel.Object.IsAutoCompileEnabled);

            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.DataSourceTextChangedCommand.Execute(null));
        }

        [Test]
        public void VerifyThatAutoCompileScriptWorksWithAutoCompile()
        {
            this.reportDesignerViewModel.Object.IsAutoCompileEnabled = true;
            Assert.AreEqual(true, this.reportDesignerViewModel.Object.IsAutoCompileEnabled);

            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.DataSourceTextChangedCommand.Execute(null));
        }

        [Test]
        public async Task VerifyThatRebuildDataSourceCommandWorks()
        {
            System.IO.File.WriteAllText(this.dsPathOpen, DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.ASCII.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(DATASOURCE_CODE));

            using (var zipFile = ZipFile.Open(this.zipPathOpen, ZipArchiveMode.Create))
            {
                using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                {
                    reportStream.Position = 0;
                    await reportStream.CopyToAsync(reportEntry);
                }

                using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                {
                    dataSourceStream.Position = 0;
                    await dataSourceStream.CopyToAsync(reportEntry);
                }
            }

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenReportCommand.Execute(null));

            Assert.AreEqual(true, this.reportDesignerViewModel.Object.Output.Contains("File succesfully compiled"));

            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.Object.RebuildDatasourceCommand.ExecuteAsyncTask(null));

            Assert.Zero(this.reportDesignerViewModel.Object.Errors.Length);
        }

        [Test]
        public async Task VerifyThatRebuildDataSourceWithParamsCommandWorks()
        {
            System.IO.File.WriteAllText(this.dsPathOpen, DATASOURCE_CODE_WITH_PARAMS);

            var reportStream = new MemoryStream(Encoding.ASCII.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(DATASOURCE_CODE_WITH_PARAMS));

            using (var zipFile = ZipFile.Open(this.zipPathOpen, ZipArchiveMode.Create))
            {
                using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                {
                    reportStream.Position = 0;
                    await reportStream.CopyToAsync(reportEntry);
                }

                using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                {
                    dataSourceStream.Position = 0;
                    await dataSourceStream.CopyToAsync(reportEntry);
                }
            }

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenReportCommand.Execute(null));

            Assert.AreEqual(true, this.reportDesignerViewModel.Object.Output.Contains("File succesfully compiled"));

            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.Object.RebuildDatasourceCommand.ExecuteAsyncTask(null));

            Assert.Zero(this.reportDesignerViewModel.Object.Errors.Length);
        }

        [Test]
        public async Task VerifyThatRebuildDataSourceCommandFails()
        {
            System.IO.File.WriteAllText(this.dsPathOpen, REBUILD_ERROR_DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.ASCII.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.ASCII.GetBytes(REBUILD_ERROR_DATASOURCE_CODE));

            using (var zipFile = ZipFile.Open(this.zipPathOpen, ZipArchiveMode.Create))
            {
                using (var reportEntry = zipFile.CreateEntry("Report.repx").Open())
                {
                    reportStream.Position = 0;
                    await reportStream.CopyToAsync(reportEntry);
                }

                using (var reportEntry = zipFile.CreateEntry("Datasource.cs").Open())
                {
                    dataSourceStream.Position = 0;
                    await dataSourceStream.CopyToAsync(reportEntry);
                }
            }

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.Object.OpenReportCommand.Execute(null));

            Assert.AreEqual(true, this.reportDesignerViewModel.Object.Output.Contains("File succesfully compiled"));

            Assert.NotZero(this.reportDesignerViewModel.Object.Errors.Length);

            await this.reportDesignerViewModel.Object.RebuildDatasourceCommand.ExecuteAsyncTask(null);

            Assert.NotZero(this.reportDesignerViewModel.Object.Errors.Length);
        }

        [Test]
        public void VerifyThatRebuildDataSourceCommandWorksWithNoDataSource()
        {
            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.Object.RebuildDatasourceCommand.ExecuteAsyncTask(null));

            Assert.AreEqual(true, this.reportDesignerViewModel.Object.Output.Contains("Nothing to compile"));
        }
    }
}
