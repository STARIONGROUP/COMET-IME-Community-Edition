// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4ScriptingModuleTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Tests
{
    using CDP4Composition.Navigation;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CDP4ScriptingModule"/>
    /// </summary>
    [TestFixture]
    public class CDP4ScriptingModuleTestFixture
    {
        private CDP4ScriptingModule scriptingModule;

        private Mock<IPanelNavigationService> panelNavigationService;

        [SetUp]
        public void SetUp()
        {
            this.panelNavigationService = new Mock<IPanelNavigationService>();            
        }

        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            this.scriptingModule = new CDP4ScriptingModule(this.panelNavigationService.Object);

            Assert.AreEqual(this.panelNavigationService.Object, this.scriptingModule.PanelNavigationService);        
        }
    }
}
