// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderedDomainOfExpertiseListConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2019 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Tests.Converters
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4RelationshipMatrix.Converters;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="OrderedDomainOfExpertiseListConverter"/> class.
    /// </summary>
    [TestFixture]
    public class OrderedDomainOfExpertiseListConverterTestFixture
    {
        private OrderedDomainOfExpertiseListConverter orderedDomainOfExpertiseListConverter;

        [SetUp]
        public void SetUp()
        {
            this.orderedDomainOfExpertiseListConverter = new OrderedDomainOfExpertiseListConverter();
        }

        [Test]
        public void Verify_that_when_Convert_is_called_sorted_list_of_DomainOfExpertise_is_returned()
        {
            var domainOfExpertise_1 = new DomainOfExpertise() {Name = "CCC"};
            var domainOfExpertise_2 = new DomainOfExpertise() { Name = "BBB" };
            var domainOfExpertise_3 = new DomainOfExpertise() { Name = "AAA" };

            var domains = new List<DomainOfExpertise> {domainOfExpertise_1, domainOfExpertise_2, domainOfExpertise_3};

            var result = this.orderedDomainOfExpertiseListConverter.Convert(domains, null, null, null) as List<DomainOfExpertise>;

            Assert.That(result[0], Is.EqualTo(domainOfExpertise_3));
            Assert.That(result[1], Is.EqualTo(domainOfExpertise_2));
            Assert.That(result[2], Is.EqualTo(domainOfExpertise_1));
        }

        [Test]
        public void Verify_That_when_Convert_is_called_on_not_a_list_of_DomainOfExpertise_no_exceptions_thrown()
        {
            var things = new List<Thing>();
            
            Assert.DoesNotThrow(() =>
            {
                var result = this.orderedDomainOfExpertiseListConverter.Convert(things, null, null, null) as List<DomainOfExpertise>;

                Assert.That(result, Is.Empty);

            });
        }

        [Test]
        public void Verify_that_when_ConvertBack_is_called_sorted_list_of_DomainOfExpertise_is_returned()
        {
            var domainOfExpertise_1 = new DomainOfExpertise() { Name = "CCC" };
            var domainOfExpertise_2 = new DomainOfExpertise() { Name = "BBB" };
            var domainOfExpertise_3 = new DomainOfExpertise() { Name = "AAA" };

            var domains = new List<DomainOfExpertise> { domainOfExpertise_1, domainOfExpertise_2, domainOfExpertise_3 };

            var result = this.orderedDomainOfExpertiseListConverter.ConvertBack(domains, null, null, null) as List<DomainOfExpertise>;

            Assert.That(result[0], Is.EqualTo(domainOfExpertise_3));
            Assert.That(result[1], Is.EqualTo(domainOfExpertise_2));
            Assert.That(result[2], Is.EqualTo(domainOfExpertise_1));
        }
    }
}