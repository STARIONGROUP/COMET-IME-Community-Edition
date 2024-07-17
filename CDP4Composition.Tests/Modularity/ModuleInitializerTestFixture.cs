// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModuleInitializerTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.Tests.Modularity
{
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Composition.Modularity;

    using NUnit.Framework;

    [TestFixture]
    public class ModuleInitializerTestFixture
    {
        [Test]
        public void VerifyAllModulesGetInitialized()
        {
            const int ModuleCount = 10;

            var modules = CreateModules(ModuleCount).ToList();
            IModuleInitializer moduleInitializer = new ModuleInitializer(modules);
            moduleInitializer.Initialize();

            Assert.That(modules.Select(m => m.InitializeCount), Has.All.EqualTo(1));
        }

        private IEnumerable<ModuleStub> CreateModules(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new ModuleStub();
            }
        }
    }

    class ModuleStub : IModule
    {
        public int InitializeCount { get; private set; }

        public void Initialize()
        {
            InitializeCount++;
        }
    }
}
