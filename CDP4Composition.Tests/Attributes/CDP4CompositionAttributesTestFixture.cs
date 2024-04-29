// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4CompositionAttributesTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Attributes
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    using NUnit.Framework;

    [TestFixture]
    public class CDP4CompositionAttributesTestFixture
    {
        [Test]
        public void TestDescriptionExport()
        {
            var attributes = new DescribeExportAttribute("name", "description", typeof(IPanelViewModel));
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
            Assert.AreEqual(typeof(IPanelViewModel), attributes.ContractType);

            attributes = new DescribeExportAttribute("name", "description", typeof(IDialogView));
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
            Assert.AreEqual(typeof(IDialogView), attributes.ContractType);
        }

        [Test]
        public void TestDialogViewExport()
        {
            var attributes = new DialogViewExportAttribute("name", "description");
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
        }

        [Test]
        public void TestDialogViewModelExport()
        {
            var attributes = new DialogViewModelExportAttribute("name", "description");
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
        }

        [Test]
        public void TestPanelViewModelExport()
        {
            var attributes = new PanelViewModelExportAttribute("name", "description");
            Assert.AreEqual("name", attributes.Name);
            Assert.AreEqual("description", attributes.Description);
            Assert.AreEqual(typeof(IPanelViewModel), attributes.ContractType);
        }
    }

    public class TestClass
    {}
}


