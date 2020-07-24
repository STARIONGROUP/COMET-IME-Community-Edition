

namespace CDP4Composition.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using CDP4Composition.Behaviors;

    using NUnit.Framework;

    using CDP4Composition.Modularity;
    using CDP4Composition.ViewModels;

    using Moq;

    [TestFixture]
    public class PluginInstallerViewModelTestFixture
    {
        private IEnumerable<(FileInfo cdp4ckFile, Manifest manifest)> updatablePlugins;
        private Mock<IPluginUpdateInstallerBehavior> behavior;

        [SetUp]
        public void Setup()
        {
            this.updatablePlugins = new List<(FileInfo cdp4ckFile, Manifest manifest)>()
            {
                (new FileInfo("test"), new Manifest()),
                (new FileInfo("test"), new Manifest()),
                (new FileInfo("test"), new Manifest())
            };

            this.behavior = new Mock<IPluginUpdateInstallerBehavior>();
            this.behavior.Setup(x => x.Close());
        }

        [Test]
        public void VerifyPropertiesAreSet()
        {
            var viewModel = new PluginInstallerViewModel(this.updatablePlugins);
            Assert.IsNotEmpty(viewModel.AvailablePlugins);
            Assert.AreSame(viewModel.UpdatablePlugins, this.updatablePlugins);
            Assert.IsTrue(viewModel.ThereIsNoInstallationInProgress);
            Assert.IsNull(viewModel.CancellationTokenSource);
        }
        
        [Test]
        public void VerifySelectAllUpdateCheckBoxCommand()
        {
            var viewModel = new PluginInstallerViewModel(this.updatablePlugins);
            Assert.IsTrue(viewModel.SelectAllUpdateCheckBoxCommand.CanExecute(null));

            viewModel.AvailablePlugins.FirstOrDefault().IsSelectedForInstallation = true;
            viewModel.SelectAllUpdateCheckBoxCommand.Execute(null);
            Assert.IsTrue(viewModel.AvailablePlugins.All(p => p.IsSelectedForInstallation));

            viewModel.SelectAllUpdateCheckBoxCommand.Execute(null);
            Assert.IsTrue(viewModel.AvailablePlugins.All(p => !p.IsSelectedForInstallation));
        }
        
        [Test]
        public void VerifyCancelCommand()
        {
            var viewModel = new PluginInstallerViewModel(this.updatablePlugins);
            Assert.IsTrue(viewModel.CancelCommand.CanExecute(null));
            this.behavior.Verify(x => x.Close(), Times.Once);
        }
    }
}
