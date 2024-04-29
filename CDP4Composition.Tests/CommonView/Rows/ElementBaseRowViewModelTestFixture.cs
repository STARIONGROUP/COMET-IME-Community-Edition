// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementBaseRowViewModelTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4CommonView.Tests
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ElementBaseRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class ElementBaseRowViewModelTestFixture
    {
        private Mock<ISession> session;

        private ElementDefinitionRowViewModel rowViewModel;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);

            this.rowViewModel = new ElementDefinitionRowViewModel(
                elementDefinition,
                this.session.Object,
                null);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void VerifyThatOwnerIsSet()
        {
            var owner = new DomainOfExpertise();

            this.rowViewModel.Owner = owner;

            Assert.AreSame(owner, this.rowViewModel.Owner);
        }
    }
}
