// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphElementViewModelTestFxture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
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


namespace CDP4Grapher.Tests.ViewModels
{
    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4Grapher.Tests.Data;
    using CDP4Grapher.ViewModels;

    using NUnit.Framework;

    [TestFixture]
    public class GraphElementViewModelTestFixture : GrapherBaseTestData
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
        }

        [Test]
        public void VerifyProperties()
        {
            var vm = new GraphElementViewModel(this.NestedElement);
            Assert.AreSame(vm.Thing, this.NestedElement);
            Assert.AreSame(vm.NestedElementElement, this.ElementUsage);
            Assert.AreEqual(vm.Name, this.ElementUsage.Name);
            Assert.AreEqual(vm.ShortName, this.ElementUsage.ShortName);
            Assert.AreEqual(vm.OwnerShortName, this.Domain.ShortName);
            Assert.AreNotEqual(vm.Category, "-");
            Assert.IsNotNull(vm.NestedElementElementListener);
        }
        
        [Test]
        public void VerifyObjectChanged()
        {
            var vm = new GraphElementViewModel(this.NestedElement);
            Assert.AreEqual(vm.Name, this.ElementUsage.Name);
            this.ElementUsage.Name = "ItHasNewNameNow";
            vm.RevisionNumber = -1;
            CDPMessageBus.Current.SendObjectChangeEvent(this.ElementUsage, EventKind.Updated);
            Assert.AreEqual(vm.Name, this.ElementUsage.Name);
        }
    }
}
