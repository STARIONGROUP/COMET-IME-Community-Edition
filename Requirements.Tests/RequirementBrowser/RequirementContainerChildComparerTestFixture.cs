// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementContainerChildComparerTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Requirements.Test
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4Requirements.Comparers;
    using CDP4Requirements.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class RequirementContainerChildComparerTestFixture
    {
        private RequirementContainerChildRowComparer comparer;
        private Mock<ISession> session;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.comparer = new RequirementContainerChildRowComparer();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatComparerWorks()
        {
            var model = new EngineeringModel() { EngineeringModelSetup = new EngineeringModelSetup() };
            var iteration = new Iteration();
            var spec = new RequirementsSpecification();
            var grp = new RequirementsGroup() { ShortName = "a" };
            var req1 = new Requirement { ShortName = "a" };
            var req2 = new Requirement { ShortName = "b" };
            var req3 = new Requirement { ShortName = "x" };

            model.Iteration.Add(iteration);
            iteration.RequirementsSpecification.Add(spec);
            spec.Requirement.Add(req1);
            spec.Requirement.Add(req2);
            spec.Requirement.Add(req3);

            var specRow = new RequirementsSpecificationRowViewModel(spec, this.session.Object, null);
            var grprow = new RequirementsGroupRowViewModel(grp, this.session.Object, null, specRow);
            var reqrow = new RequirementRowViewModel(req1, this.session.Object, specRow);
            var reqrow2 = new RequirementRowViewModel(req2, this.session.Object, specRow);
            var reqrow3 = new RequirementRowViewModel(req3, this.session.Object, specRow);

            Assert.AreEqual(-1, this.comparer.Compare(reqrow, reqrow2));
            Assert.AreEqual(-1, this.comparer.Compare(reqrow, grprow));
            Assert.AreEqual(1, this.comparer.Compare(grprow, reqrow));
            Assert.AreEqual(-1, this.comparer.Compare(reqrow3, grprow));
            Assert.AreEqual(1, this.comparer.Compare(grprow, reqrow3));
        }
    }
}
