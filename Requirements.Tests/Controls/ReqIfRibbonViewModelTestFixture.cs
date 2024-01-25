// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Controls
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Requirements.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using ReactiveUI;

    using ReqIFSharp;

    [TestFixture]
    internal class ReqIfRibbonViewModelTestFixture
    {
        private Mock<ISession> session;
        private Iteration iteration;
        private readonly Uri uri = new Uri("http://test.com");

        private Mock<IServiceLocator> serviceLocator;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;

        private Assembler assembler;
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private Mock<IPluginSettingsService> pluginSettingService;

        private Person person;
        private Participant participant;
        private DomainOfExpertise domainOfExpertise;
        private EngineeringModelSetup engineeringModelSetup;
        private EngineeringModel engineeringModel;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.serviceLocator = new Mock<IServiceLocator>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfExportDialogViewModel>()));
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>()));

            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();

            this.pluginSettingService = new Mock<IPluginSettingsService>();

            this.pluginSettingService.Setup(
                x =>
                    x.Read<RequirementsModuleSettings>(
                        It.IsAny<bool>(), It.IsAny<JsonConverter[]>())).Returns(
                new RequirementsModuleSettings());

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>())
                .Returns(this.thingDialogNavigationService.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>())
                .Returns(this.dialogNavigationService.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>())
                .Returns(this.fileDialogService.Object);

            this.serviceLocator.Setup(x => x.GetInstance<IPluginSettingsService>())
                .Returns(this.pluginSettingService.Object);

            this.serviceLocator.Setup(x => x.GetInstance<ICDPMessageBus>())
                .Returns(this.messageBus);

            this.assembler = new Assembler(this.uri, this.messageBus);

            this.person = new Person { GivenName = "John", Surname = "Doe" };
            this.domainOfExpertise = new DomainOfExpertise { ShortName = "SYS", Name = "System" };
            this.participant = new Participant { Person = this.person, SelectedDomain = this.domainOfExpertise };

            this.session = new Mock<ISession>();

            this.session.Setup(x => x.ActivePerson).Returns(this.person);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            var iterationSetup = new IterationSetup() { IterationNumber = 42 };

            this.engineeringModelSetup = new EngineeringModelSetup() { Name = "42" };
            this.engineeringModel = new EngineeringModel() { EngineeringModelSetup = this.engineeringModelSetup };
            this.engineeringModelSetup.Participant.Add(this.participant);

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                IterationSetup = iterationSetup,
                Container = this.engineeringModel
            };

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri) { RequiredRdl = this.srdl };
            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatSessionEventsAreCaught()
        {
            var vm = new ReqIfRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);

            Assert.AreEqual(1, vm.Sessions.Count);

            sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Closed);
            this.messageBus.SendMessage(sessionEvent);

            Assert.AreEqual(0, vm.Sessions.Count);
        }

        [Test]
        public void VerifyThatIterationEventAreCaughtFailed()
        {
            var vm = new ReqIfRibbonViewModel();
            Assert.Throws<InvalidOperationException>(() => this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added));
        }

        [Test]
        public void VerifyThatITerationEventAreCaught()
        {
            var vm = new ReqIfRibbonViewModel();
            var sessionEvent = new SessionEvent(this.session.Object, SessionStatus.Open);
            this.messageBus.SendMessage(sessionEvent);

            Assert.IsFalse(((ICommand)vm.ExportCommand).CanExecute(null));

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Added);

            Assert.IsTrue(((ICommand)vm.ExportCommand).CanExecute(null));
            Assert.AreEqual(1, vm.Iterations.Count);

            this.messageBus.SendObjectChangeEvent(this.iteration, EventKind.Removed);

            Assert.AreEqual(0, vm.Iterations.Count);
        }

        [Test]
        public async Task VerifyExportCommand()
        {
            var vm = new ReqIfRibbonViewModel();
            vm.Iterations.Add(this.iteration);
            Assert.IsTrue(((ICommand)vm.ExportCommand).CanExecute(null));
            await vm.ExportCommand.Execute();
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<ReqIfExportDialogViewModel>()), Times.Once);
        }

        [Test]
        public void VerifyImportCommand()
        {
            var vm = new ReqIfRibbonViewModel();
            Assert.IsFalse(((ICommand)vm.ImportCommand).CanExecute(null));
            vm.Iterations.Add(this.iteration);
            vm.Sessions.Add(this.session.Object);
            Assert.IsTrue(((ICommand)vm.ImportCommand).CanExecute(null));
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>())).Returns(new ReqIfImportResult(new ReqIF(), this.iteration, new ImportMappingConfiguration(), null));
            Assert.DoesNotThrowAsync(async () => await vm.ImportCommand.Execute());
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>())).Returns(new ReqIfImportResult(new ReqIF(), this.iteration, new ImportMappingConfiguration(), false));
            Assert.DoesNotThrowAsync(async () => await vm.ImportCommand.Execute());
            this.dialogNavigationService.Setup(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>())).Returns(new ReqIfImportResult(new ReqIF() { CoreContent = new ReqIFContent() }, this.iteration, new ImportMappingConfiguration(), true));
            Assert.DoesNotThrow(() => _ = Observable.Return(Unit.Default).InvokeCommand(vm.ImportCommand));
            this.dialogNavigationService.Verify(x => x.NavigateModal(It.IsAny<ReqIfImportDialogViewModel>()), Times.Exactly(3));
        }
    }
}
