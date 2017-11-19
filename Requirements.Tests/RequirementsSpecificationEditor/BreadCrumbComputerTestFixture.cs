// ------------------------------------------------------------------------------------------------
// <copyright file="BreadCrumbComputerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.RequirementsSpecificationEditor
{
    using CDP4Common;
    using CDP4Common.EngineeringModelData;

    using CDP4Requirements.Utils;
    using CDP4Requirements.ViewModels.RequirementsSpecificationEditor;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="BreadCrumbComputer"/> Test
    /// </summary>
    [TestFixture]
    public class BreadCrumbComputerTestFixture
    {
        private Iteration iteration;
        private RequirementsSpecification requirementsSpecification;

        [SetUp]
        public void SetUp()
        {
            this.iteration = new Iteration();
            this.requirementsSpecification = new RequirementsSpecification() {ShortName = "URD" };

            this.iteration.RequirementsSpecification.Add(this.requirementsSpecification);
        }

        [Test]
        public void VerifyThatBreadCrumOfNonGroupedRequirementGroupCanBeComputed()
        {
            var requirementsGroup = new RequirementsGroup() { ShortName = "GRP1"};

            this.requirementsSpecification.Group.Add(requirementsGroup);

            Assert.AreEqual("S:URD.RG:GRP1", requirementsGroup.BreadCrumb());
        }

        [Test]
        public void VerifyThatBreadCrumOfGroupedRequirementGroupCanBeComputed()
        {
            var requirementsGroupA = new RequirementsGroup() { ShortName = "GRPA" };
            var requirementsGroupA_A = new RequirementsGroup() { ShortName = "GRPA_A" };

            this.requirementsSpecification.Group.Add(requirementsGroupA);
            requirementsGroupA.Group.Add(requirementsGroupA_A);

            Assert.AreEqual("S:URD.RG:GRPA", requirementsGroupA.BreadCrumb());
            Assert.AreEqual("S:URD.RG:GRPA.RG:GRPA_A", requirementsGroupA_A.BreadCrumb());
        }

        [Test]
        public void VerifyThatBreadCrumbOfNonGroupRequirementCanBeComputed()
        {
            var requirement = new Requirement() { ShortName = "REQA" };
            this.requirementsSpecification.Requirement.Add(requirement);

            Assert.AreEqual("S:URD.R:REQA", requirement.BreadCrumb());
        }

        [Test]
        public void VerifyThatBreadCrumbOfGroupRequirementCanBeComputed()
        {
            var requirement = new Requirement() { ShortName = "REQA" };
            this.requirementsSpecification.Requirement.Add(requirement);

            var requirementsGroupA = new RequirementsGroup() { ShortName = "GRPA" };
            this.requirementsSpecification.Group.Add(requirementsGroupA);

            requirement.Group = requirementsGroupA;

            Assert.AreEqual("S:URD.RG:GRPA.R:REQA", requirement.BreadCrumb());
        }

        [Test]
        public void VerifyThatWhenRequirementIsNotContainedAContainmentExceptionIsThrown()
        {
            var requirement = new Requirement() { ShortName = "REQA" };

            Assert.Throws<ContainmentException>(() => requirement.BreadCrumb());
        }
    }
}
