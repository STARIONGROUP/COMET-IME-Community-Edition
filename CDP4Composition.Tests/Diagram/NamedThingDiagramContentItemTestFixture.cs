﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NamedThingDiagramContentItemTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Diagram
{
    using System.Threading;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Diagram;

    using CDP4Dal;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="NamedThingDiagramContentItemViewModel"/> class
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class NamedThingDiagramContentItemTestFixture
    {
        private DomainOfExpertise domainOfExpertise;

        [SetUp]
        public void SetUp()
        {
            this.domainOfExpertise = new DomainOfExpertise();
        }

        [Test]
        public void VerifyThatNamedWithThingCanBeConstructed()
        {
            var namedThingDiagramContentItem = new NamedThingDiagramContentItemViewModel(this.domainOfExpertise);

            Assert.AreEqual(this.domainOfExpertise, namedThingDiagramContentItem.Thing);
        }
    }
}
