// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixItemExtensionsTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
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

namespace CDP4RelationshipMatrix.Tests.ViewModel
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4RelationshipMatrix.ViewModels;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="MatrixItemExtensions"/> class.
    /// </summary>
    [TestFixture]
    public class MatrixItemExtensionsTestFixture
    {
        private readonly Uri uri = new Uri("http://starion.test.com");

        [Test]
        public void VerifyThatDefinedThingUsesItsNameAndShortName()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, this.uri) { Name = "battery", ShortName = "bat" };

            Assert.That(elementDefinition.QueryDisplayName(), Is.EqualTo("battery"));
            Assert.That(elementDefinition.QueryDisplayShortName(), Is.EqualTo("bat"));
        }

        [Test]
        public void VerifyThatFileUsesCurrentRevisionNameAsNameAndShortName()
        {
            var file = new File(Guid.NewGuid(), null, this.uri);
            var firstRevision = new FileRevision(Guid.NewGuid(), null, this.uri) { Name = "old.stp", CreatedOn = new DateTime(2024, 1, 1) };
            var lastRevision = new FileRevision(Guid.NewGuid(), null, this.uri) { Name = "geometry.stp", CreatedOn = new DateTime(2024, 6, 1) };
            file.FileRevision.Add(firstRevision);
            file.FileRevision.Add(lastRevision);

            Assert.That(file.QueryDisplayName(), Is.EqualTo("geometry.stp"));
            Assert.That(file.QueryDisplayShortName(), Is.EqualTo("geometry.stp"));
        }
    }
}
