// --------------------------------------------------------------------------------------------------------------------
// <copyright file="COMETBootstrapperTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Composition
{
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using CDP4Composition.Composition;
    using CDP4Composition.Modularity;
    using CDP4Composition.Services.AppSettingService;
    using NUnit.Framework;

    [TestFixture]
    public class COMETBootstrapperTestFixture
    {
        [Test]
        public void VerifyCOMETBootstrapperComposition()
        {
            TestBootstrapper bootstrapper = new TestBootstrapper();
            bootstrapper.Run();

            Assert.IsTrue(bootstrapper.TestModuleInitialized);
            Assert.IsTrue(bootstrapper.TestComponentAdded);
            Assert.IsTrue(bootstrapper.AddCustomCatalogsCalled);
        }
    }

    internal class TestBootstrapper : COMETBootstrapper<TestAppSettings>
    {
        public bool TestModuleInitialized { get; private set; }
        public bool TestComponentAdded { get; private set; }
        public bool AddCustomCatalogsCalled { get; private set; }

        protected override void AddCustomCatalogs(AggregateCatalog catalog)
        {
            AddCustomCatalogsCalled = true;
        }

        protected override void OnComposed(CompositionContainer container)
        {
            var module = container.GetExportedValue<IModule>() as TestModule;
            TestModuleInitialized = module.IsInitialized;

            var testComponent = container.GetExportedValue<TestComponent>();
            TestComponentAdded = testComponent is not null;
        }
    }

    [Export()]
    internal class TestComponent
    {
    }

    internal class TestAppSettings : AppSettings
    {
    }

    [Export(typeof(IAppSettingsService<TestAppSettings>))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class TestAppSettingsService : AppSettingsService<TestAppSettings>
    {
        
    }

    [Export(typeof(IModule))]
    internal class TestModule : IModule
    {
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            IsInitialized = true;
        }
    }
}
