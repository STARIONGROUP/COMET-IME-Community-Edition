// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginInstallerViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net.Http;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Composition.Modularity;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services.AppSettingService;
    using CDP4Composition.Utilities;

    using CDP4UpdateServerDal;
    using CDP4UpdateServerDal.Enumerators;

    using COMET.Behaviors;
    using COMET.Services;
    using COMET.Settings;
    using COMET.ViewModels;

    using CommonServiceLocator;

    using DevExpress.Mvvm.Native;

    using Moq;

    using Newtonsoft.Json;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class UpdateDownloaderInstallerViewModelTestFixture : UpdateDownloaderInstallerDataSetup
    {
        private const string PluginName0 = "plugin0";
        private const string PluginName1 = "plugin1";
        private const string PluginName2 = "plugin2";
        private const string Version0 = "0.1.0.0";
        private const string Version1 = "0.2.0.0";
        private IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> updatablePlugins;
        private Mock<IUpdateDownloaderInstallerBehavior> behavior;
        private Mock<IAssemblyInformationService> assemblyInformationService;
        private Mock<IProcessRunnerService> processRunner;
        private Mock<IDialogNavigationService> dialogNavigation;
        private Mock<IAppSettingsService<ImeAppSettings>> appSettingsService;
        private Mock<IUpdateServerClient> updateServerClient;
        private UpdateDownloaderInstallerViewModel viewModel;

        private Mock<IServiceLocator> serviceLocator;
        private List<Manifest> installedManifest;
        private ProcessorArchitecture processorArchitecture;
        private Platform platform;
        private ImeAppSettings appSettings;
        private List<(string ThingName, string Version)> updateResultFomApi;
        private FileInfo alreadyDownloadedMsi;
        private FileInfo realAlreadyDownloadedplugin0;
        private FileInfo realAlreadyDownloadedplugin1;

        [SetUp]
        public override void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            base.Setup();

            this.updatablePlugins = new List<(FileInfo cdp4ckFile, Manifest manifest)>()
            {
                this.Plugin
            };

            this.behavior = new Mock<IUpdateDownloaderInstallerBehavior>();
            this.behavior.Setup(x => x.Close());

            this.viewModel = new UpdateDownloaderInstallerViewModel(this.updatablePlugins);
            this.appSettings = new ImeAppSettings();

            this.serviceLocator = new Mock<IServiceLocator>();

            this.updateServerClient = new Mock<IUpdateServerClient>();

            this.installedManifest = new List<Manifest>()
            {
                new Manifest() { Name = PluginName0, Version = Version0 },
                new Manifest() { Name = PluginName1, Version = Version0 },
                new Manifest() { Name = PluginName2, Version = Version0 }
            };

            this.processorArchitecture = ProcessorArchitecture.Amd64;
            this.platform = this.processorArchitecture == ProcessorArchitecture.Amd64 ? Platform.X64 : Platform.X86;

            this.updateResultFomApi = new List<(string ThingName, string Version)>()
            {
                (UpdateServerClient.ImeKey, Version1),
                (PluginName0, Version1),
                (PluginName1, Version1),
            };

            this.updateServerClient.
                Setup(x => x.CheckForUpdate(It.IsAny<List<Manifest>>(), It.IsAny<Version>(), It.IsAny<ProcessorArchitecture>())).
                Returns(Task.FromResult<IEnumerable<(string ThingName, string Version)>>(this.updateResultFomApi));
            
            this.updateServerClient.Setup(x => x.DownloadIme(Version1, this.platform)).Returns(Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes("msi"))));
            this.updateServerClient.Setup(x => x.DownloadPlugin(PluginName0, It.IsAny<string>())).Returns(Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes("plugin"))));
            this.updateServerClient.Setup(x => x.DownloadPlugin(PluginName1, It.IsAny<string>())).Returns(Task.FromResult<Stream>(new MemoryStream(Encoding.UTF8.GetBytes("plugin"))));

            this.assemblyInformationService = new Mock<IAssemblyInformationService>();
            this.assemblyInformationService.Setup(x => x.GetVersion()).Returns(new Version(Version0));
            this.assemblyInformationService.Setup(x => x.GetProcessorArchitecture()).Returns(this.processorArchitecture);
            this.assemblyInformationService.Setup(x => x.GetLocation()).Returns(this.BasePath);

            this.processRunner = new Mock<IProcessRunnerService>();
            this.processRunner.Setup(x => x.Restart());

            this.dialogNavigation = new Mock<IDialogNavigationService>();
            this.dialogNavigation.Setup(x => x.NavigateModal(It.IsAny<IDialogViewModel>())).Returns(new BaseDialogResult(true));

            this.appSettingsService = new Mock<IAppSettingsService<ImeAppSettings>>();
            this.appSettingsService.Setup(x => x.AppSettings).Returns(this.appSettings);

            this.appSettingsService.Setup(x => x.Save());

            this.serviceLocator.Setup(x => x.GetInstance<IUpdateServerClient>()).Returns(this.updateServerClient.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IAssemblyInformationService>()).Returns(this.assemblyInformationService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IProcessRunnerService>()).Returns(this.processRunner.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IDialogNavigationService>()).Returns(this.dialogNavigation.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IAppSettingsService<ImeAppSettings>>()).Returns(this.appSettingsService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IUpdateFileSystemService>()).Returns(this.UpdateFileSystem);

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
        }

        [TearDown]
        public void Teardown()
        {
            try
            {
                if (this.realAlreadyDownloadedplugin0?.Directory?.Exists == true)
                {
                    this.realAlreadyDownloadedplugin0.Directory.Delete(true);
                }

                if (this.realAlreadyDownloadedplugin1?.Directory?.Exists == true)
                {
                    this.realAlreadyDownloadedplugin1.Directory.Delete(true);
                }

                if (Directory.Exists(this.BasePath))
                {
                    Directory.Delete(this.BasePath, true);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        [Test]
        public void VerifyPropertiesAreSet()
        {
            var vm = new UpdateDownloaderInstallerViewModel(this.updatablePlugins);
            Assert.IsNotEmpty(vm.AvailablePlugins);
            Assert.AreSame(vm.UpdatablePlugins, this.updatablePlugins);
            Assert.IsFalse(vm.IsInstallationOrDownloadInProgress);
            Assert.IsNull(vm.CancellationTokenSource);
            Assert.IsFalse(vm.IsInDownloadMode);
            Assert.IsFalse(vm.IsCheckingApi);
            Assert.IsNull(vm.DialogResult);
            Assert.IsNull(vm.LoadingMessage);
            Assert.False(vm.IsBusy);
        }

        [Test]
        public async Task VerifyCheckApi()
        {
            var vm = new UpdateDownloaderInstallerViewModel(false);
            await vm.CheckApiForUpdate().ConfigureAwait(true);
            Assert.AreEqual(vm.DownloadableThings, this.updateResultFomApi);

            this.updateServerClient.Setup(x => x.CheckForUpdate(It.IsAny<List<Manifest>>(), It.IsAny<Version>(), It.IsAny<ProcessorArchitecture>())).Throws(new HttpRequestException(string.Empty));
            await vm.CheckApiForUpdate().ConfigureAwait(true);
            this.dialogNavigation.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Once);
            
            this.updateServerClient.Setup(x => x.CheckForUpdate(It.IsAny<List<Manifest>>(), It.IsAny<Version>(), It.IsAny<ProcessorArchitecture>())).Throws(new Exception(string.Empty));
            Assert.DoesNotThrowAsync(() => vm.CheckApiForUpdate()); 
            this.dialogNavigation.Verify(x => x.NavigateModal(It.IsAny<IDialogViewModel>()), Times.Once);
        }

        [Test]
        public async Task VerifyDownload()
        {
            var vm = new UpdateDownloaderInstallerViewModel(true);
            Assert.AreEqual(this.updateResultFomApi, vm.DownloadableThings);
            await vm.DownloadCommand.Execute();

            vm.AvailablePlugins.ForEach(p => p.IsSelected = true);
            vm.AvailableIme.ForEach(i => i.IsSelected = true);
            Assert.IsTrue(((ICommand)vm.DownloadCommand).CanExecute(null));

            await vm.DownloadCommand.Execute();
            Thread.Sleep(100);
            var basePath = new DirectoryInfo(this.BasePath);
            var allCdp4Ck = basePath.EnumerateFiles("*.cdp4ck", SearchOption.AllDirectories).Select(f => f.Name).ToList();
            Assert.AreEqual(3, allCdp4Ck.Count());
            Assert.IsNotNull(allCdp4Ck.SingleOrDefault(p => p.Contains(PluginName1)));
            Assert.IsNotNull(allCdp4Ck.SingleOrDefault(p => p.Contains(PluginName0)));
            var msi = basePath.EnumerateFiles("*.msi", SearchOption.AllDirectories).Single();
            Assert.IsTrue(msi.Name.Contains(UpdateServerClient.ImeKey));
            Assert.IsTrue(msi.Name.Contains(Version1));
        }

        [Test]
        public async Task VerifySelectAllUpdateCheckBoxCommand()
        {
            var vm = new UpdateDownloaderInstallerViewModel(this.updatablePlugins);
            Assert.IsTrue(((ICommand)vm.SelectAllUpdateCheckBoxCommand).CanExecute(null));

            await vm.SelectAllUpdateCheckBoxCommand.Execute();
            Assert.IsTrue(vm.AvailablePlugins.All(p => p.IsSelected));

            await vm.SelectAllUpdateCheckBoxCommand.Execute();
            Assert.IsTrue(vm.AvailablePlugins.All(p => !p.IsSelected));

            vm = new UpdateDownloaderInstallerViewModel(true);

            await vm.SelectAllUpdateCheckBoxCommand.Execute();
            Assert.IsTrue(vm.AvailablePlugins.All(p => p.IsSelected));
        }
        
        [Test]
        public async Task VerifyCancelCommand()
        {
            this.viewModel.Behavior = this.behavior.Object;

            this.viewModel.IsInstallationOrDownloadInProgress = true;
            Assert.IsTrue(((ICommand)this.viewModel.CancelCommand).CanExecute(null));
            await this.viewModel.CancelCommand.Execute();
            this.behavior.Verify(x => x.Close(), Times.Once);
            this.viewModel.IsInstallationOrDownloadInProgress = false;
        }

        [Test]
        public async Task VerifyInstallCommand()
        {
            this.viewModel.Behavior = this.behavior.Object;
            this.viewModel.AvailablePlugins.First().FileSystem = this.UpdateFileSystem;
            Assert.IsTrue(((ICommand)this.viewModel.InstallCommand).CanExecute(null));
            this.viewModel.AvailablePlugins.First().IsSelected = false;
            await this.viewModel.InstallCommand.Execute();
            this.behavior.Verify(x => x.Close(), Times.Never);

            this.viewModel.AvailablePlugins.First().IsSelected = true;
            await this.viewModel.InstallCommand.Execute();
        }
        
        [Test]
        public async Task VerifyFailingInstallCommand()
        {
            this.UpdateFileSystem.UpdateCdp4CkFileInfo.Delete();
            this.viewModel.Behavior = this.behavior.Object;
            this.viewModel.AvailablePlugins.First().FileSystem = this.UpdateFileSystem;
            Assert.IsTrue(((ICommand)this.viewModel.InstallCommand).CanExecute(null));
            this.viewModel.AvailablePlugins.First().IsSelected = true;
            await this.viewModel.InstallCommand.Execute();
            this.behavior.Verify(x => x.Close(), Times.Never);
        }

        /// <summary>
        /// This test is creating a large file (100MB) so it can have a chance to Invoke the cancellation on the CancellationToken
        /// The Task may fail on some system.
        /// </summary>
        /// <returns>The Task may fail on some system</returns>
        [Ignore("Test should run with mocked IUpdateFileSystemService implementation")]
        public async Task VerifyCancellationToken()
        {
            this.SetupTestContentForInstallationCancellationPurpose(this.UpdateFileSystem.InstallationPath.FullName);

            using (var largeFile = File.OpenWrite(this.UpdateFileSystem.UpdateCdp4CkFileInfo.FullName))
            {
                largeFile.Seek(100L * 1024 * 1024, SeekOrigin.Begin);
                largeFile.WriteByte(0);
                largeFile.Close();
            }

            this.viewModel.Behavior = this.behavior.Object;
            this.viewModel.AvailablePlugins.First().FileSystem = this.UpdateFileSystem;
            Assert.IsTrue(((ICommand)this.viewModel.InstallCommand).CanExecute(null));
            this.viewModel.AvailablePlugins.First().IsSelected = true;

            await Task.WhenAll(
                new Task(() => this.viewModel.InstallCommand.Execute()),
                new Task(() => this.viewModel.CancelCommand.Execute()));
            
            this.AssertInstalledTestFileHasBeenRestored();
        }

        [Test]
        public async Task VerifyRestartAfterDownloadCommand()
        {
            var vm = new UpdateDownloaderInstallerViewModel(false);
            Assert.IsTrue(((ICommand)vm.RestartAfterDownloadCommand).CanExecute(null));
            Assert.IsFalse(vm.HasToRestartClientAfterDownload);
            vm.IsInstallationOrDownloadInProgress = false;
            var downloadText = vm.DownloadButtonText;
            await vm.RestartAfterDownloadCommand.Execute();
            Assert.AreNotEqual(downloadText, vm.DownloadButtonText);
            Assert.IsTrue(vm.HasToRestartClientAfterDownload);
        }

        [Test]
        public async Task VerifyCancelDownload()
        {
            var vm = new UpdateDownloaderInstallerViewModel(true);
            vm.AvailablePlugins.First().IsSelected = true;
            vm.AvailableIme.First().IsSelected = true;
            Assert.IsTrue(((ICommand)vm.DownloadCommand).CanExecute(null));
            await vm.DownloadCommand.Execute();
            Assert.IsTrue(((ICommand)vm.CancelCommand).CanExecute(null));
            await vm.CancelCommand.Execute();

            Assert.False(
                this.UpdateFileSystem.PluginDownloadPath.EnumerateFiles().Any(
                    f => f.Name == PluginName0 || f.Name == PluginName1));
        }
        
        [Test]
        public async Task VerifyCheckApiWhenAllHasBeenDownloadedAlready()
        {
            this.alreadyDownloadedMsi = new FileInfo(Path.Combine(this.UpdateFileSystem.ImeDownloadPath.FullName, $"CDP4{UpdateServerClient.ImeKey}-CE.x64-{Version1}.msi"));
            File.Create(this.alreadyDownloadedMsi.FullName).Dispose();

            this.realAlreadyDownloadedplugin0 = this.CreateRealCdp4Ck(PluginName0);
            this.realAlreadyDownloadedplugin1 = this.CreateRealCdp4Ck(PluginName1);
            
            var vm = new UpdateDownloaderInstallerViewModel(false);
            await vm.CheckApiForUpdate().ConfigureAwait(true);
            Assert.IsEmpty(vm.DownloadableThings);
        }

        private FileInfo CreateRealCdp4Ck(string pluginName)
        {
            var cdp4CkFile = new FileInfo(Path.Combine(PluginUtilities.GetDownloadDirectory(true, pluginName).FullName, $"{pluginName}.cdp4ck"));
            
            var manifestBytes = Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(
                    new Manifest() { Name = pluginName, Version = Version1 }));

            using (var plugin0Cdp4CkFile = new FileStream(cdp4CkFile.FullName, FileMode.CreateNew))
            {
                using (var archive = new ZipArchive(plugin0Cdp4CkFile, ZipArchiveMode.Create, true))
                {
                    var entry = archive.CreateEntry($"{pluginName}.plugin.manifest", CompressionLevel.Fastest);

                    using (var entryStream = entry.Open())
                    {
                        entryStream.Write(manifestBytes, 0, manifestBytes.Length);
                    }
                }
            }

            return cdp4CkFile;
        }

        [Test]
        public async Task VerifyInstallThrowsException()
        {
            var mockedRow = new Mock<IPluginRowViewModel>();
            mockedRow.Setup(x => x.Install(It.IsAny<CancellationToken>())).Throws<UnauthorizedAccessException>();
            mockedRow.Setup(x => x.IsSelected).Returns(true);
            mockedRow.Setup(x => x.HandlingCancelationOfInstallation());
            mockedRow.Setup(x => x.FileSystem).Returns(this.UpdateFileSystem);

            var vm = new UpdateDownloaderInstallerViewModel(new List<(FileInfo cdp4ckFile, Manifest manifest)>())
            {
                Behavior = this.behavior.Object
            };

            vm.AvailablePlugins.Add(mockedRow.Object);
            await  vm.InstallCommand.Execute();
            mockedRow.Verify(x => x.Install(It.IsAny<CancellationToken>()), Times.Once);
            mockedRow.Verify(x => x.HandlingCancelationOfInstallation(), Times.Once);
            Assert.IsNull(vm.CancellationTokenSource);
        }
        
        [Test]
        public async Task VerifyDownloadThrowsException()
        {
            var mockedPluginRow = new Mock<IPluginRowViewModel>();
            mockedPluginRow.Setup(x => x.Download(this.updateServerClient.Object)).Throws<UnauthorizedAccessException>();
            mockedPluginRow.Setup(x => x.IsSelected).Returns(true);
            mockedPluginRow.Setup(x => x.HandlingCancelationOfDownload());
            mockedPluginRow.Setup(x => x.FileSystem).Returns(this.UpdateFileSystem);

            var mockedImeRow = new Mock<IImeRowViewModel>();
            mockedImeRow.Setup(x => x.Download(this.updateServerClient.Object)).Throws<UnauthorizedAccessException>();
            mockedImeRow.Setup(x => x.IsSelected).Returns(true);
            mockedImeRow.Setup(x => x.HandlingCancelationOfDownload());
            mockedImeRow.Setup(x => x.FileSystem).Returns(this.UpdateFileSystem);

            var vm = new UpdateDownloaderInstallerViewModel(false)
            {
                Behavior = this.behavior.Object
            };

            vm.AvailablePlugins.Add(mockedPluginRow.Object);
            vm.AvailableIme.Add(mockedImeRow.Object);
            await vm.DownloadCommand.Execute();
            mockedPluginRow.Verify(x => x.HandlingCancelationOfDownload(), Times.Once);
            mockedImeRow.Verify(x => x.HandlingCancelationOfDownload(), Times.Once);
            Assert.IsNull(vm.CancellationTokenSource);
        }
    }
}
