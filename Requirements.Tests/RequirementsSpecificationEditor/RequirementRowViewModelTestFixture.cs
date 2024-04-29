﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementRowViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.RequirementsSpecificationEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4Requirements.ViewModels.RequirementsSpecificationEditor;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementRowViewModel" /> class.
    /// </summary>
    [TestFixture]
    public class RequirementRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private DomainOfExpertise domainOfExpertise;
        private Category category_1;
        private Category category_2;
        private RequirementsSpecification requirementsSpecification;
        private Requirement requirement;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.permissionService = new Mock<IPermissionService>();
            this.session = new Mock<ISession>();

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.domainOfExpertise = new DomainOfExpertise { ShortName = "SYS", Name = "System" };
            this.category_1 = new Category { ShortName = "REQ", Name = "Requirements" };
            this.category_2 = new Category { ShortName = "FUNC", Name = "Functions" };

            this.requirementsSpecification = new RequirementsSpecification { ShortName = "MRD", Name = "Mission Requirements Document" };
            this.requirement = new Requirement { ShortName = "REQ_1", Name = "Requirement 1", Owner = this.domainOfExpertise };
            this.requirementsSpecification.Requirement.Add(this.requirement);
        }

        [Test]
        public void Verify_that_when_requirement_is_not_categorized_categories_property_is_default_hyphen()
        {
            var rowViewModel = new RequirementRowViewModel(this.requirement, this.session.Object, null);
            Assert.That(rowViewModel.Categories, Is.EqualTo("-"));
        }

        [Test]
        public void Verify_that_when_the_requirement_categorized_the_category_shortnames_are_set()
        {
            this.requirement.Category.Add(this.category_1);
            this.requirement.Category.Add(this.category_2);

            var rowViewModel = new RequirementRowViewModel(this.requirement, this.session.Object, null);
            Assert.That(rowViewModel.Categories, Is.EqualTo("REQ, FUNC"));
        }
    }
}
