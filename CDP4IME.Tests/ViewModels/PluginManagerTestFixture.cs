// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginManagerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4IME.ViewModels;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.ServiceLocation;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PluginManagerTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private List<IModule> modules;

        [SetUp]
        public void Setup()
        {
            this.modules = new List<IModule>();
            this.modules.Add(new TestModule());

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(new ServiceLocatorProvider(() => this.serviceLocator.Object));
            this.serviceLocator.Setup(x => x.GetAllInstances(typeof(IModule))).Returns(this.modules);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var viewmodel = new PluginManagerViewModel();

            this.serviceLocator.Verify(x => x.GetAllInstances(typeof(IModule)));
            Assert.AreEqual(1, viewmodel.Plugins.Count);
        }

        [Test]
        public void VerifyThatCloseUpdateNotification()
        {
            var viewmodel = new PluginManagerViewModel();

            viewmodel.CloseCommand.Execute(null);
            Assert.IsFalse(viewmodel.DialogResult.Result.Value);
        }

        [Test]
        public void VerifyThatSelectedPluginIsSet()
        {
            var viewmodel = new PluginManagerViewModel();

            viewmodel.SelectedPlugin = viewmodel.Plugins.First();
            Assert.AreEqual(viewmodel.Plugins.First(), viewmodel.SelectedPlugin);
            Assert.IsNotNull(viewmodel.SelectedPlugin.AssemblyName);
            Assert.IsNotNull(viewmodel.SelectedPlugin.Name);
            Assert.IsNotNull(viewmodel.SelectedPlugin.Description);
            Assert.IsNotNull(viewmodel.SelectedPlugin.Company);
            Assert.IsNotNull(viewmodel.SelectedPlugin.Version);
        }

        public class TestModule : IModule
        {
            public void Initialize()
            {
                throw new NotImplementedException();
            }
        }
    }
}
