// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttributeDefinitionMappingRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.ReqIF
{
    using System.Reactive.Concurrency;

    using CDP4Requirements.ReqIFDal;
    using CDP4Requirements.ViewModels;

    using NUnit.Framework;

    using ReactiveUI;

    using ReqIFSharp;

    /// <summary>
    /// Suite of tests for the <see cref="AttributeDefinitionMappingRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class AttributeDefinitionMappingRowViewModelTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
        }

        [Test]
        public void VerifyThatReservedReqIfNamesArePreMapped()
        {
            var textDefinition = new AttributeDefinitionXHTML { LongName = ThingToReqIfMapper.RequirementTextXhtmlAttributeDefName };
            var nameDefinition = new AttributeDefinitionString { LongName = ThingToReqIfMapper.ReqIfNameAttributeDefName };
            var foreignIdDefinition = new AttributeDefinitionString { LongName = ThingToReqIfMapper.ReqIfForeignIdAttributeDefName };
            var otherDefinition = new AttributeDefinitionString { LongName = "Something else" };

            Assert.AreEqual(AttributeDefinitionMapKind.FIRST_DEFINITION, new AttributeDefinitionMappingRowViewModel(textDefinition, null, () => { }).AttributeDefinitionMapKind);
            Assert.AreEqual(AttributeDefinitionMapKind.NAME, new AttributeDefinitionMappingRowViewModel(nameDefinition, null, () => { }).AttributeDefinitionMapKind);
            Assert.AreEqual(AttributeDefinitionMapKind.SHORTNAME, new AttributeDefinitionMappingRowViewModel(foreignIdDefinition, null, () => { }).AttributeDefinitionMapKind);
            Assert.AreEqual(AttributeDefinitionMapKind.NONE, new AttributeDefinitionMappingRowViewModel(otherDefinition, null, () => { }).AttributeDefinitionMapKind);
        }
    }
}
