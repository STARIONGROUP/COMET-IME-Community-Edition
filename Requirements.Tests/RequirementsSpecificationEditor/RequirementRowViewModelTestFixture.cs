// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementsSpecificationEditor
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Requirements.ViewModels.RequirementsSpecificationEditor;
    using Moq;
    using NUnit.Framework;
    
    /// <summary>
    /// Suite of tests for the <see cref="RequirementRowViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class RequirementRowViewModelTestFixture
    {
        private Mock<ISession> session;
        private DomainOfExpertise domainOfExpertise;
        private Category category_1;
        private Category category_2;
        private RequirementsSpecification requirementsSpecification;
        private Requirement requirement;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();

            this.domainOfExpertise = new DomainOfExpertise {ShortName = "SYS", Name = "System"};
            this.category_1 = new Category {ShortName = "REQ", Name = "Requirements"};
            this.category_2 = new Category { ShortName = "FUNC", Name = "Functions" };

            this.requirementsSpecification = new RequirementsSpecification {ShortName = "MRD", Name = "Mission Requirements Document"};
            this.requirement = new Requirement {ShortName = "REQ_1", Name = "Requirement 1", Owner = this.domainOfExpertise};
            this.requirementsSpecification.Requirement.Add(this.requirement);
        }

        [Test]
        public void Verify_that_when_requirement_is_not_categorized_categories_property_is_default_hyphen()
        {
            var rowViewModel = new CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementRowViewModel(this.requirement, this.session.Object, null);
            Assert.That(rowViewModel.Categories, Is.EqualTo("-"));
        }

        [Test]
        public void Verify_that_when_the_requirement_categorized_the_category_shortnames_are_set()
        {
            this.requirement.Category.Add(this.category_1);
            this.requirement.Category.Add(this.category_2);

            var rowViewModel = new CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementRowViewModel(this.requirement, this.session.Object, null);
            Assert.That(rowViewModel.Categories, Is.EqualTo("REQ, FUNC"));
        }
    }
}