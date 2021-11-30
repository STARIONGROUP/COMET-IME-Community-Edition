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
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4CommonView.ViewModels;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4Reporting.DynamicTableChecker;
    using CDP4Reporting.SubmittableParameterValues;
    using CDP4Reporting.ViewModels;

    using DevExpress.XtraReports.UI;

    using ICSharpCode.AvalonEdit.Document;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ReportDesignerViewModel"/> class
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ReportDesignerViewModelTextFixture
    {
        private readonly string dsPathOpen = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataSourceOpen.cs");
        private readonly string dsPathSave = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDataSourceSave.cs");

        private readonly string zipPathOpen = Path.Combine(TestContext.CurrentContext.TestDirectory, "ReportArchiveOpen.rep4");
        private readonly string zipPathSave = Path.Combine(TestContext.CurrentContext.TestDirectory, "ReportArchiveSave.rep4");

        private const string DATASOURCE_CODE = @"namespace CDP4Reporting
        {
            using CDP4Reporting.DataCollection;
            public class TestDataSource : OptionDependentDataCollector
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
                public override IEnumerable<IReportingParameter> CreateParameters(object dataObject, IDataCollector dataCollector)
                {
                    List<IReportingParameter> paramsList = new List<IReportingParameter>();
                    paramsList.Add(new ReportingParameter(""param1"", typeof(int), 0));
                    paramsList.Add(new ReportingParameter(""param2"", typeof(string), string.Empty));
                    paramsList[1].AddLookupValue(""1"", ""one"");
                    paramsList.Add(new ReportingParameter(""param3"", typeof(string), ""DefaultValue""));
                    paramsList[2].Visible = false;
                    paramsList.Add(new ReportingParameter(""param3"", typeof(string), null));
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
        private ReportDesignerViewModel reportDesignerViewModel;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;
        private Mock<IOpenSaveFileDialogService> openSaveFileDialogService;
        private Mock<ISubmittableParameterValuesCollector> submittableParameterValuesCollector;
        private Mock<IDynamicTableChecker> dynamicTableChecker;
        private Mock<IPermissionService> permissionService;
        private Mock<IMessageBoxService> messageBoxService;

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
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private static DomainOfExpertise domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null) { ShortName = "DOMAIN", Name = "domain" };
        private static DomainOfExpertise domainOfExpertise_2 = new DomainOfExpertise(Guid.NewGuid(), null, null) { ShortName = "TEST", Name = "Test" };
        private static DomainOfExpertise domainOfExpertise_3 = new DomainOfExpertise(Guid.NewGuid(), null, null) { ShortName = "SUB", Name = "Subscription" };
        private ElementDefinition elementDefinition_1;
        private ElementDefinition elementDefinition_2;
        private ElementUsage elementUsage_1;
        private ElementUsage elementUsage_2;
        private static Option option_A = new Option(Guid.NewGuid(), null, null) { ShortName = "OPT_A", Name = "Option A" };
        private static Option option_B = new Option(Guid.NewGuid(), null, null) { ShortName = "OPT_B", Name = "Option B" };
        private Parameter parameter;
        private ParameterOverride parameterOverride;
        private Parameter parameter2;
        private ActualFiniteState actualState_3;
        private ActualFiniteState actualState_4;

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
            this.submittableParameterValuesCollector = new Mock<ISubmittableParameterValuesCollector>();
            this.permissionService = new Mock<IPermissionService>();
            this.dynamicTableChecker = new Mock<IDynamicTableChecker>();
            this.messageBoxService = new Mock<IMessageBoxService>();

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.openSaveFileDialogService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<ISubmittableParameterValuesCollector>()).Returns(this.submittableParameterValuesCollector.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDynamicTableChecker>()).Returns(this.dynamicTableChecker.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = domainOfExpertise };

            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(domainOfExpertise);
            this.sitedir.Domain.Add(domainOfExpertise_2);
            this.sitedir.Domain.Add(domainOfExpertise_3);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.model.Iteration.Add(this.iteration);

            this.elementDefinition_1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "Sat",
                Name = "Satellite"
            };

            this.elementDefinition_2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "Bat",
                Name = "Battery"
            };

            this.elementUsage_1 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ElementDefinition = this.elementDefinition_2,
                ShortName = "bat_a",
                Name = "battery a"
            };

            this.elementUsage_2 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri)
            {
                ElementDefinition = this.elementDefinition_2,
                ShortName = "bat_b",
                Name = "battery b"
            };

            var simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "m"
            };

            var simpleQuantityKind2 = new SimpleQuantityKind(Guid.NewGuid(), null, null)
            {
                ShortName = "v"
            };

            var ratioScale = new RatioScale(Guid.NewGuid(), null, null);

            simpleQuantityKind.PossibleScale.Add(ratioScale);
            simpleQuantityKind.DefaultScale = ratioScale;

            simpleQuantityKind2.PossibleScale.Add(ratioScale);
            simpleQuantityKind2.DefaultScale = ratioScale;

            var actualList = new ActualFiniteStateList(Guid.NewGuid(), null, null);
            actualList.Owner = domainOfExpertise;

            var possibleList1 = new PossibleFiniteStateList(Guid.NewGuid(), null, null);

            var possibleState1 = new PossibleFiniteState(Guid.NewGuid(), null, null) { Name = "possiblestate1", ShortName = "1" };
            var possibleState2 = new PossibleFiniteState(Guid.NewGuid(), null, null) { Name = "possiblestate2", ShortName = "2" };

            possibleList1.PossibleState.Add(possibleState1);
            possibleList1.PossibleState.Add(possibleState2);

            actualList.PossibleFiniteStateList.Add(possibleList1);

            this.actualState_3 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);

            this.actualState_3.PossibleState.Add(possibleState1);

            this.actualState_4 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);

            this.actualState_4.PossibleState.Add(possibleState2);

            this.parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = domainOfExpertise,
                ParameterType = simpleQuantityKind,
                IsOptionDependent = true,
                Scale = ratioScale
            };

            var parameterSubscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = domainOfExpertise_3
            };

            this.parameter.ParameterSubscription.Add(parameterSubscription);

            this.parameter2 = new Parameter(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = domainOfExpertise_2,
                ParameterType = simpleQuantityKind2,
                StateDependence = actualList,
                Scale = ratioScale
            };

            this.parameterOverride = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri)
            {
                Owner = domainOfExpertise,
                Parameter = this.parameter
            };

            var parameterValueset_1 = new ParameterValueSet
            {
                ActualOption = option_B,
                Iid = Guid.NewGuid()
            };

            var parameterValueset_2 = new ParameterValueSet
            {
                ActualOption = option_A,
                Iid = Guid.NewGuid()
            };

            var parameterValueset_3 = new ParameterValueSet
            {
                ActualState = this.actualState_3,
                Iid = Guid.NewGuid()
            };

            var parameterValueset_4 = new ParameterValueSet
            {
                ActualState = this.actualState_4,
                Iid = Guid.NewGuid()
            };

            var parameterSubscriptionValueSetA = new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid()
            };

            var parameterSubscriptionValueSetB = new ParameterSubscriptionValueSet
            {
                Iid = Guid.NewGuid()
            };

            var values_1 = new List<string> { "2" };
            var values_2 = new List<string> { "3" };
            var values_3 = new List<string> { "220" };
            var emptyValues = new List<string> { "-" };
            var publishedValues = new List<string> { "123" };
            var subscriptionValues = new List<string> { "456" };

            var overrideValueset = new ParameterOverrideValueSet()
            {
                ParameterValueSet = parameterValueset_1,
                Iid = Guid.NewGuid()
            };

            this.iteration.Option.Add(option_A);
            this.iteration.Option.Add(option_B);
            this.iteration.DefaultOption = option_A;

            parameterValueset_1.Manual = new ValueArray<string>(values_1);
            parameterValueset_1.Reference = new ValueArray<string>(values_1);
            parameterValueset_1.Computed = new ValueArray<string>(values_1);
            parameterValueset_1.Formula = new ValueArray<string>(values_1);
            parameterValueset_1.Published = new ValueArray<string>(publishedValues);
            parameterValueset_1.ValueSwitch = ParameterSwitchKind.MANUAL;

            parameterValueset_2.Manual = new ValueArray<string>(values_2);
            parameterValueset_2.Reference = new ValueArray<string>(values_2);
            parameterValueset_2.Computed = new ValueArray<string>(values_2);
            parameterValueset_2.Formula = new ValueArray<string>(values_2);
            parameterValueset_2.Published = new ValueArray<string>(publishedValues);
            parameterValueset_2.ValueSwitch = ParameterSwitchKind.MANUAL;

            parameterValueset_3.Manual = new ValueArray<string>(values_3);
            parameterValueset_3.Reference = new ValueArray<string>(values_3);
            parameterValueset_3.Computed = new ValueArray<string>(values_3);
            parameterValueset_3.Formula = new ValueArray<string>(values_3);
            parameterValueset_3.Published = new ValueArray<string>(emptyValues);
            parameterValueset_3.ValueSwitch = ParameterSwitchKind.MANUAL;

            parameterValueset_4.Manual = new ValueArray<string>(emptyValues);
            parameterValueset_4.Reference = new ValueArray<string>(emptyValues);
            parameterValueset_4.Computed = new ValueArray<string>(emptyValues);
            parameterValueset_4.Formula = new ValueArray<string>(emptyValues);
            parameterValueset_4.Published = new ValueArray<string>(publishedValues);
            parameterValueset_4.ValueSwitch = ParameterSwitchKind.MANUAL;

            overrideValueset.Manual = new ValueArray<string>(values_1);
            overrideValueset.Reference = new ValueArray<string>(values_1);
            overrideValueset.Computed = new ValueArray<string>(values_1);
            overrideValueset.Formula = new ValueArray<string>(values_1);
            overrideValueset.Published = new ValueArray<string>(publishedValues);
            overrideValueset.ValueSwitch = ParameterSwitchKind.MANUAL;

            parameterSubscriptionValueSetA.Manual = new ValueArray<string>(subscriptionValues);
            parameterSubscriptionValueSetA.ValueSwitch = ParameterSwitchKind.MANUAL;
            parameterSubscriptionValueSetA.SubscribedValueSet = parameterValueset_1;

            parameterSubscriptionValueSetB.Manual = new ValueArray<string>(subscriptionValues);
            parameterSubscriptionValueSetB.ValueSwitch = ParameterSwitchKind.COMPUTED;
            parameterSubscriptionValueSetB.SubscribedValueSet = parameterValueset_2;

            this.parameter.ValueSet.Add(parameterValueset_1);
            this.parameter.ValueSet.Add(parameterValueset_2);

            this.parameterOverride.ValueSet.Add(overrideValueset);

            this.parameter2.ValueSet.Add(parameterValueset_3);
            this.parameter2.ValueSet.Add(parameterValueset_4);

            parameterSubscription.ValueSet.Add(parameterSubscriptionValueSetA);
            parameterSubscription.ValueSet.Add(parameterSubscriptionValueSetB);

            this.elementUsage_1.ExcludeOption.Add(option_A);
            this.elementUsage_1.ParameterOverride.Add(this.parameterOverride);

            this.elementDefinition_1.Parameter.Add(this.parameter);
            this.elementDefinition_1.ContainedElement.Add(this.elementUsage_1);
            this.elementDefinition_1.ContainedElement.Add(this.elementUsage_2);

            this.elementDefinition_2.Parameter.Add(this.parameter);
            this.elementDefinition_2.Parameter.Add(this.parameter2);

            this.iteration.Element.Add(this.elementDefinition_1);
            this.iteration.Element.Add(this.elementDefinition_2);
            this.iteration.TopElement = this.elementDefinition_1;

            this.iteration.ActualFiniteStateList.Add(actualList);
            this.iteration.PossibleFiniteStateList.Add(possibleList1);
            actualList.ActualState.Add(this.actualState_3);
            actualList.ActualState.Add(this.actualState_4);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.IsVersionSupported(It.IsAny<Version>())).Returns(true);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.reportDesignerViewModel = new ReportDesignerViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingsService.Object);
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
        [TestCaseSource(nameof(SubmitParameterValueTestCases))]
        public async Task VerifyThatSubmitParameterValuesCommandWorks((DomainOfExpertise domain, Option option, string path, bool found, string newValue, bool isValid, bool shouldChange) tuple)
        {
            this.iteration.DefaultOption = tuple.option;

            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>())).Returns(tuple.domain);

            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<SubmitConfirmationViewModel>()))
                .Returns(new SubmitConfirmationDialogResult(false, "", new List<Thing>()));

            var submittableParameterValue = new SubmittableParameterValue(tuple.path)
            {
                ControlName = "Label",
                Text = tuple.newValue
            };

            var submittableParameterValues = new List<SubmittableParameterValue>
            {
                submittableParameterValue
            };

            this.submittableParameterValuesCollector.Setup(x => x.Collect(It.IsAny<XtraReport>())).Returns(submittableParameterValues);

            var reportStream = new MemoryStream(Encoding.UTF8.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.UTF8.GetBytes(DATASOURCE_CODE));

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

            this.openSaveFileDialogService.Setup(x =>
                x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathSave });

            Assert.DoesNotThrow(() => this.reportDesignerViewModel.OpenReportCommand.Execute(null));

            await this.reportDesignerViewModel.CurrentReport.CreateDocumentAsync();

            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.SubmitParameterValuesCommand.ExecuteAsyncTask(null));

            if (!tuple.found)
            {
                this.dialogNavigationService.Verify(
                    x => x.NavigateModal(
                        It.Is<OkDialogViewModel>(
                            okDialog =>
                                okDialog.Title == "Warning" &&
                                okDialog.Message.Contains("The following errors were found during ValueSet lookup") &&
                                okDialog.Message.Contains(tuple.path)
                        )
                    )
                    , Times.Once);

                this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<SubmitConfirmationViewModel>()), Times.Never);
            }
            else if (!tuple.isValid)
            {
                this.dialogNavigationService.Verify(
                    x => x.NavigateModal(
                        It.Is<OkDialogViewModel>(
                            okDialog =>
                                okDialog.Title == "Warning" &&
                                okDialog.Message.Contains("The following errors were found during ValueSet lookup") &&
                                okDialog.Message.Contains(tuple.newValue)
                        )
                    )
                    , Times.Once);

                this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<SubmitConfirmationViewModel>()), Times.Once);
            }
            else
            {
                var times = tuple.shouldChange ? Times.Once() : Times.Never();

                this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<SubmitConfirmationViewModel>()), times);
            }
        }

        private static IEnumerable<(DomainOfExpertise domain, Option option, string path, bool found, string newValue, bool isValid, bool shouldChange)> SubmitParameterValueTestCases()
        {
            yield return (domainOfExpertise, option_A, @"Sat\m\\OPT_A", true, "1", true, true);
            yield return (domainOfExpertise, option_A, @"Sat\m\\OPT_A", true, "3", true, false);
            yield return (domainOfExpertise, option_A, @"NotFound\m\\OPT_A", false, "1", true, false);
            yield return (domainOfExpertise, option_A, @"NotFound", false, "1", true, false);
            yield return (domainOfExpertise, option_A, @"Sat\m\\UnknownOption", true, "1", true, true);
            yield return (domainOfExpertise, option_A, @"Sat\m\\UnknownOption", true, "3", true, false);
            yield return (domainOfExpertise, option_A, @"Sat\m\\OPT_A", true, "1.1", true, true);
            yield return (domainOfExpertise, option_A, @"Sat\m\\OPT_A", true, "1.1.1", false, true);
            yield return (domainOfExpertise, option_A, @"Sat\m\\OPT_A", true, "", true, true);
            yield return (domainOfExpertise, option_A, @"Sat\m\\OPT_A", true, "-", true, true);
            yield return (domainOfExpertise, option_A, @"Sat.bat_a\v\1\OPT_A", false, "3", true, false);
            yield return (domainOfExpertise, option_A, @"Sat.bat_a\v\2\OPT_A", false, "3", true, false);
            yield return (domainOfExpertise, option_B, @"Sat.bat_a\v\1\OPT_B", false, "220", true, false);
            yield return (domainOfExpertise, option_B, @"Sat.bat_a\v\2\OPT_B", false, "220", true, false);
            yield return (domainOfExpertise_2, option_B, @"Sat.bat_a\v\1\OPT_B", true, "220", true, false);
            yield return (domainOfExpertise_2, option_B, @"Sat.bat_a\v\2\OPT_B", true, "220", true, true);
            yield return (domainOfExpertise_3, option_A, @"Sat\m\\OPT_A", true, "1", true, true);
            yield return (domainOfExpertise_3, option_A, @"Sat\m\\OPT_A", true, "456", true, false);
            yield return (domainOfExpertise_3, option_B, @"Sat\m\\OPT_B", true, "1", true, true);
            yield return (domainOfExpertise_3, option_B, @"Sat\m\\OPT_B", true, "456", true, false);
        }

        [Test]
        public void VerifyThatExportCommandWorksWithoutSavingFile()
        {
            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(string.Empty);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.ExportScriptCommand.Execute(null));
            Assert.AreEqual(null, this.reportDesignerViewModel.CodeFilePath);

            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(string.Empty);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.SaveReportCommand.Execute(null));
            Assert.AreEqual(null, this.reportDesignerViewModel.CurrentReportProjectFilePath);
        }

        [Test]
        public async Task VerifySavingExistingReportWorks()
        {
            var reportStream = new MemoryStream(Encoding.UTF8.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.UTF8.GetBytes(DATASOURCE_CODE));

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
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.OpenReportCommand.Execute(null));

            Assert.DoesNotThrow(() => this.reportDesignerViewModel.SaveReportCommand.Execute(null));
            Assert.AreEqual(this.zipPathSave, this.reportDesignerViewModel.CurrentReportProjectFilePath);
        }

        [Test]
        public async Task VerifyThatExportCommandWorksBySavingFile()
        {
            System.IO.File.WriteAllText(this.dsPathSave, DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.UTF8.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.UTF8.GetBytes(DATASOURCE_CODE));

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
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.ExportScriptCommand.Execute(null));

            this.openSaveFileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(this.zipPathSave);
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.SaveReportCommand.Execute(null));
        }

        [Test]
        public void VerifyThatImportCommandWorksWithoutOpeningFile()
        {
            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.ImportScriptCommand.Execute(null));
            Assert.AreEqual(null, this.reportDesignerViewModel.CodeFilePath);

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.OpenReportCommand.Execute(null));
            Assert.AreEqual(null, this.reportDesignerViewModel.CurrentReportProjectFilePath);
        }

        [Test]
        public async Task VerifyThatImportCommandWorksByOpeningFile()
        {
            System.IO.File.WriteAllText(this.dsPathOpen, DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.UTF8.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.UTF8.GetBytes(DATASOURCE_CODE));

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
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.ImportScriptCommand.Execute(null));

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.OpenReportCommand.Execute(null));
        }

        [Test]
        public void VerifyThatCompileScriptCommandWorks()
        {
            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.CompileScriptCommand.ExecuteAsyncTask(null));
        }

        [Test]
        public async Task VerifyThatCompileScriptCommandPass()
        {
            var textDocument = new TextDocument
            {
                Text = DATASOURCE_CODE
            };

            this.reportDesignerViewModel.Document = textDocument;
            await this.reportDesignerViewModel.CompileScriptCommand.ExecuteAsyncTask(null);

            Assert.AreEqual(0, this.reportDesignerViewModel.CompileResult.Errors.Count);
            Assert.AreEqual(string.Empty, this.reportDesignerViewModel.Errors);
        }

        [Test]
        public async Task VerifyThatCompileScriptCommandFailed()
        {
            var textDocument = new TextDocument
            {
                Text = DATASOURCE_CODE_INVALID
            };

            this.reportDesignerViewModel.Document = textDocument;
            await this.reportDesignerViewModel.CompileScriptCommand.ExecuteAsyncTask(null);

            Assert.AreNotEqual(0, this.reportDesignerViewModel.CompileResult.Errors.Count);
            Assert.AreNotEqual(string.Empty, this.reportDesignerViewModel.Errors);
        }

        [Test]
        public void VerifyThatNewReportCommandWorks()
        {
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.NewReportCommand.Execute(null));
            Assert.AreEqual(string.Empty, this.reportDesignerViewModel.Document.Text);
        }

        [Test]
        public void VerifyThatNewReportCommandWorksWithSwitchReport()
        {
            var textDocument = new TextDocument
            {
                Text = DATASOURCE_CODE
            };

            this.reportDesignerViewModel.Document = textDocument;

            Assert.DoesNotThrow(() => this.reportDesignerViewModel.NewReportCommand.Execute(null));
            Assert.AreEqual(DATASOURCE_CODE, this.reportDesignerViewModel.Document.Text);
        }

        [Test]
        public void VerifyThatAutoCompileScriptWorks()
        {
            Assert.AreEqual(false, this.reportDesignerViewModel.IsAutoCompileEnabled);

            Assert.DoesNotThrow(() => this.reportDesignerViewModel.DataSourceTextChangedCommand.Execute(null));
        }

        [Test]
        public void VerifyThatAutoCompileScriptWorksWithAutoCompile()
        {
            this.reportDesignerViewModel.IsAutoCompileEnabled = true;
            Assert.AreEqual(true, this.reportDesignerViewModel.IsAutoCompileEnabled);

            Assert.DoesNotThrow(() => this.reportDesignerViewModel.DataSourceTextChangedCommand.Execute(null));
        }

        [Test]
        public void VerifyThatRebuildDataSourceCommandWorks()
        {
            System.IO.File.WriteAllText(this.dsPathOpen, DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.UTF8.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.UTF8.GetBytes(DATASOURCE_CODE));

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

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.OpenReportCommand.Execute(null));

            Assert.AreEqual(true, this.reportDesignerViewModel.Output.Contains("File succesfully compiled"));

            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.RebuildDatasourceCommand.ExecuteAsyncTask(null));

            Assert.Zero(this.reportDesignerViewModel.Errors.Length);
        }

        [Test]
        public void VerifyThatRebuildDataSourceWithParamsCommandWorks()
        {
            System.IO.File.WriteAllText(this.dsPathOpen, DATASOURCE_CODE_WITH_PARAMS);

            var reportStream = new MemoryStream(Encoding.UTF8.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.UTF8.GetBytes(DATASOURCE_CODE_WITH_PARAMS));

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

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.OpenReportCommand.Execute(null));

            Assert.AreEqual(true, this.reportDesignerViewModel.Output.Contains("File succesfully compiled"));

            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.RebuildDatasourceCommand.ExecuteAsyncTask(null));

            Assert.Zero(this.reportDesignerViewModel.Errors.Length);
        }

        [Test]
        public void VerifyThatRebuildDataSourceAndRefreshPreviewWorks()
        {
            System.IO.File.WriteAllText(this.dsPathOpen, DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.UTF8.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.UTF8.GetBytes(DATASOURCE_CODE));

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

            this.openSaveFileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.zipPathOpen });
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.OpenReportCommand.Execute(null));

            Assert.AreEqual(true, this.reportDesignerViewModel.Output.Contains("File succesfully compiled"));

            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.RebuildDatasourceAndRefreshPreviewCommand.ExecuteAsyncTask(null));

            Assert.Zero(this.reportDesignerViewModel.Errors.Length);

            //**********************************************************************************************
            //since ReportDesignerViewModel.currentReportDesignerDocument is null during test execution,
            //no Parameters will be added to the report designer EVER during report execution.
            //That's why we can Verify things the way we do in the following section:
            //**********************************************************************************************
            this.messageBoxService.Verify(
                x => x.Show(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(),
                    It.IsAny<MessageBoxImage>()), Times.Never);

            this.reportDesignerViewModel.Document.Text = DATASOURCE_CODE_WITH_PARAMS;

            Assert.AreEqual(true, this.reportDesignerViewModel.Output.Contains("File succesfully compiled"));

            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.RebuildDatasourceAndRefreshPreviewCommand.ExecuteAsyncTask(null));

            Assert.Zero(this.reportDesignerViewModel.Errors.Length);

            this.messageBoxService.Verify(
                x => x.Show(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(),
                    It.IsAny<MessageBoxImage>()), Times.Once);

            //**********************************************************************************************
        }

        [Test]
        public async Task VerifyThatRebuildDataSourceCommandFails()
        {
            System.IO.File.WriteAllText(this.dsPathOpen, REBUILD_ERROR_DATASOURCE_CODE);

            var reportStream = new MemoryStream(Encoding.UTF8.GetBytes(REPORT_CODE));
            var dataSourceStream = new MemoryStream(Encoding.UTF8.GetBytes(REBUILD_ERROR_DATASOURCE_CODE));

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
            Assert.DoesNotThrow(() => this.reportDesignerViewModel.OpenReportCommand.Execute(null));

            Assert.AreEqual(true, this.reportDesignerViewModel.Output.Contains("File succesfully compiled"));

            Assert.NotZero(this.reportDesignerViewModel.Errors.Length);

            await this.reportDesignerViewModel.RebuildDatasourceCommand.ExecuteAsyncTask(null);

            Assert.NotZero(this.reportDesignerViewModel.Errors.Length);
        }

        [Test]
        public void VerifyThatRebuildDataSourceCommandWorksWithNoDataSource()
        {
            Assert.DoesNotThrowAsync(async () => await this.reportDesignerViewModel.RebuildDatasourceCommand.ExecuteAsyncTask(null));

            Assert.AreEqual(true, this.reportDesignerViewModel.Output.Contains("Nothing to compile"));
        }
    }
}
