// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfImportDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
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

namespace CDP4Requirements.Tests.ReqIF
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4Requirements.ViewModels;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using ReactiveUI;

    using ReqIFSharp;

    [TestFixture]
    public class ReqIfImportDialogViewModelTestFixture
    {
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private Mock<IPluginSettingsService> pluginSettingService;
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;
        private DomainOfExpertise domain;
        private EngineeringModel model;
        private Iteration iteration;
        private Person person;
        private Participant participant;

        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        private Assembler assembler;
        private Uri uri = new Uri("http://test.com");

        private ReqIF reqIf;
        private DatatypeDefinitionString stringDatadef;
        private SpecificationType spectype;
        private AttributeDefinitionString attribute;

        private ParameterType pt;

        private ReqIfImportDialogViewModel dialog;
        private Mock<IReqIFDeSerializer> reqIfSerialiser;
        private RequirementsModuleSettings settings;
        private string path;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.permissionService = new Mock<IPermissionService>();
            this.path = Path.Combine(TestContext.CurrentContext.TestDirectory, "ReqIf", "testreq.reqif");
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.fileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(new string[] { this.path });
            this.reqIf = new ReqIF { Lang = "en" };
            this.reqIf.TheHeader = new ReqIFHeader() { Identifier = Guid.NewGuid().ToString() };
            var corecontent = new ReqIFContent();
            this.reqIf.CoreContent = corecontent;
            this.stringDatadef = new DatatypeDefinitionString();
            this.spectype = new SpecificationType();
            this.attribute = new AttributeDefinitionString() { DatatypeDefinition = this.stringDatadef };

            this.spectype.SpecAttributes.Add(this.attribute);

            corecontent.DataTypes.Add(this.stringDatadef);

            this.settings = new RequirementsModuleSettings()
            {
                SavedConfigurations =
                {
                    new ImportMappingConfiguration()
                    {
                        ReqIfId = this.reqIf.TheHeader.Identifier, Name = "Test"
                    }
                }
            };

            this.pluginSettingService = new Mock<IPluginSettingsService>();
            this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>())).Returns(this.settings);
            this.pluginSettingService.Setup(x => x.Read<RequirementsModuleSettings>(false)).Returns(this.settings);

            this.reqIfSerialiser = new Mock<IReqIFDeSerializer>();
            this.reqIfSerialiser.Setup(x => x.Deserialize(It.IsAny<string>(), It.IsAny<bool>(), null)).Returns(new[] { this.reqIf });
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.assembler = new Assembler(this.uri);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };
            this.sitedir.SiteReferenceDataLibrary.Add(this.srdl);
            this.modelsetup.RequiredRdl.Add(this.mrdl);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };

            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationSetup);
            this.sitedir.Domain.Add(this.domain);
            this.model.Iteration.Add(this.iteration);

            this.person = new Person(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), this.assembler.Cache, this.uri) { Person = this.person };
            this.sitedir.Person.Add(this.person);
            this.modelsetup.Participant.Add(this.participant);

            this.pt = new BooleanParameterType(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.srdl.ParameterType.Add(this.pt);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });

            this.assembler.Cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));

            this.dialog = new ReqIfImportDialogViewModel(new[] { this.session.Object }, new[] { this.iteration }, this.fileDialogService.Object, this.pluginSettingService.Object, this.reqIfSerialiser.Object);
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
        }

        [Test]
        public void VerifyThatCancelCommandWorks()
        {
            this.dialog.CancelCommand.Execute(null);
            Assert.IsFalse(this.dialog.DialogResult.Result.Value);
        }

        [Test]
        public void VerifyBrowseCommand()
        {
            this.dialog.BrowseCommand.Execute(null);
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1), Times.Once);
            Assert.IsNotNull(this.dialog.Path);
        }

        [Test]
        public async Task VerifyThatExecuteOkWorks()
        {
            Assert.IsFalse(this.dialog.CanExecuteImport);
            this.dialog.Path = this.path;
            this.dialog.SelectedIteration = this.dialog.Iterations.First();

            Assert.IsTrue(this.dialog.CanExecuteImport);
            _ = await this.dialog.OkCommand.ExecuteAsyncTask(null);
            this.reqIfSerialiser.Verify(x => x.Deserialize(It.IsAny<string>(), It.IsAny<bool>(), null), Times.Once);
            this.pluginSettingService.Verify(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>()), Times.Once);
            var result = this.dialog.DialogResult as ReqIfImportResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result?.Result.Value);
            Assert.AreSame(this.settings.SavedConfigurations[0], result.MappingConfiguration);
            Assert.IsNotNull(result.Iteration);
            Assert.IsNotNull(result.ReqIfObject);
        }

        [Test]
        public async Task VerifyThatSavedConfigurationArePickedUpCorrectly()
        {
            Assert.IsFalse(this.dialog.CanExecuteImport);
            this.dialog.Path = this.path;
            this.dialog.SelectedIteration = this.dialog.Iterations.First();

            // Without Any Configuration
            this.dialog.SelectedMappingConfiguration = this.dialog.AvailableMappingConfiguration.FirstOrDefault(x => x.Name == ReqIfImportDialogViewModel.NoConfigurationText);
            Assert.IsTrue(this.dialog.SelectedMappingConfiguration.Name == ReqIfImportDialogViewModel.NoConfigurationText);

            Assert.IsTrue(this.dialog.CanExecuteImport);
            _ = await this.dialog.OkCommand.ExecuteAsyncTask(null);
            var resultNoConfiguration = this.dialog.DialogResult as ReqIfImportResult;
            Assert.IsNotNull(resultNoConfiguration);
            Assert.IsTrue(resultNoConfiguration?.Result.Value);
            Assert.IsNull(resultNoConfiguration.MappingConfiguration);

            //With AUTO selection of the mapping configuration
            this.dialog.SelectedMappingConfiguration = this.dialog.AvailableMappingConfiguration.FirstOrDefault(x => x.Name == ReqIfImportDialogViewModel.AutoConfigurationText);
            Assert.IsTrue(this.dialog.SelectedMappingConfiguration.Name == ReqIfImportDialogViewModel.AutoConfigurationText);

            Assert.IsTrue(this.dialog.CanExecuteImport);
            _ = await this.dialog.OkCommand.ExecuteAsyncTask(null);

            Assert.IsNotNull(this.dialog.SelectedMappingConfiguration);
            var resultAutoSelectedConfiguration = this.dialog.DialogResult as ReqIfImportResult;
            Assert.IsNotNull(resultAutoSelectedConfiguration);
            Assert.IsTrue(resultAutoSelectedConfiguration?.Result.Value);
            Assert.AreSame(this.settings.SavedConfigurations[0], resultAutoSelectedConfiguration.MappingConfiguration);

            //With explicite selection
            this.dialog.SelectedMappingConfiguration = this.dialog.AvailableMappingConfiguration.Last();
            Assert.IsTrue(this.dialog.SelectedMappingConfiguration.Name == this.settings.SavedConfigurations.Last().Name);

            Assert.IsTrue(this.dialog.CanExecuteImport);
            _ = await this.dialog.OkCommand.ExecuteAsyncTask(null);
            Assert.IsNotNull(this.dialog.SelectedMappingConfiguration);
            var result = this.dialog.DialogResult as ReqIfImportResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result?.Result.Value);
            Assert.AreSame(this.settings.SavedConfigurations[0], result.MappingConfiguration);

            //Verifications on Mocks

            this.reqIfSerialiser.Verify(x => x.Deserialize(It.IsAny<string>(), It.IsAny<bool>(), null), Times.Exactly(3));
            this.pluginSettingService.Verify(x => x.Read<RequirementsModuleSettings>(false), Times.Once);
            this.pluginSettingService.Verify(x => x.Read<RequirementsModuleSettings>(true, It.IsAny<JsonConverter[]>()), Times.Exactly(2));
        }
    }
}
