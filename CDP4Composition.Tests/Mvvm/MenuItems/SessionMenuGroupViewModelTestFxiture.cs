﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionMenuGroupViewModelTestFxiture.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Tests.Mvvm.MenuItems
{
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="SessionMenuGroupViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class SessionMenuGroupViewModelTestFxiture
    {
        private string dataSourceUri;
        private Mock<ISession> session;
        private SiteDirectory siteDirectory;
        private SessionMenuGroupViewModel sessionMenuGroupViewModel;

        [SetUp]
        public void SetUp()
        {
            this.dataSourceUri = "https://www.stariongroup.eu";
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.DataSourceUri).Returns(this.dataSourceUri);
            this.session.Setup(x => x.Name).Returns(this.dataSourceUri + " John Doe");
            this.session.Setup(x => x.CDPMessageBus).Returns(new CDPMessageBus());
            this.siteDirectory = new SiteDirectory();

            this.sessionMenuGroupViewModel = new SessionMenuGroupViewModel(this.siteDirectory, this.session.Object);
        }

        [Test]
        public void VerifyThatDeriveNameReturnsExpectedResult()
        {
            Assert.AreEqual(this.dataSourceUri + " John Doe", this.sessionMenuGroupViewModel.Name);
        }

        [Test]
        public void VerifyThatEngineeringModelsIsEmpty()
        {
            CollectionAssert.IsEmpty(this.sessionMenuGroupViewModel.EngineeringModels);
        }
    }
}
