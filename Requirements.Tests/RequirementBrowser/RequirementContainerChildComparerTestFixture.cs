// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementContainerChildComparerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


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
        private RequirementContainerChildRowComparer comparer = new RequirementContainerChildRowComparer();
        private Mock<ISession> session = new Mock<ISession>();

        [Test]
        public void VerifyThatComparerWorks()
        {
            var model = new EngineeringModel() { EngineeringModelSetup = new EngineeringModelSetup()};
            var iteration = new Iteration();
            var spec = new RequirementsSpecification();
            var grp = new RequirementsGroup() { ShortName = "a" };
            var req1 = new Requirement { ShortName = "a"};
            var req2 = new Requirement { ShortName = "b"};
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

            Assert.AreEqual(-1, comparer.Compare(reqrow, reqrow2));
            Assert.AreEqual(-1, comparer.Compare(reqrow, grprow));
            Assert.AreEqual(1, comparer.Compare(grprow, reqrow));
            Assert.AreEqual(-1, comparer.Compare(reqrow3, grprow));
            Assert.AreEqual(1, comparer.Compare(grprow, reqrow3));
        }
    }
}