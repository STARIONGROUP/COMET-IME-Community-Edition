// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkbookRowViewModelTestFixtureTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4CrossViewEditor.Tests.ViewModels
{
    using CDP4CrossViewEditor.ViewModels;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="WorkbookRowViewModel"/> class
    /// </summary>
    [TestFixture]
    [Ignore("Debug AppVeyor build failing")]
    public class WorkbookRowViewModelTestFixtureTestFixture
    {
        private WorkbookRowViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.viewModel = null;
        }

        [Test]
        public void CheckIfModelIsInitialized()
        {
            this.viewModel = new WorkbookRowViewModel(null);

            Assert.IsNull(this.viewModel.Workbook);
            Assert.IsNull(this.viewModel.Name);
            Assert.IsNull(this.viewModel.Path);
        }
    }
}
