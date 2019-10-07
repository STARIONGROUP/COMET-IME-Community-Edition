// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementArrayAssemblerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Assemblers
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4Requirements.Assemblers;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementArrayAssembler"/> class.
    /// </summary>
    [TestFixture]
    public class RequirementArrayAssemblerTestFixture
    {
        private Mock<ISession> session;

        private EngineeringModelSetup engineeringModelSetup;

        private IterationSetup iterationSetup;

        private EngineeringModel engineeringModel;

        private Iteration iteration;

        private DomainOfExpertise domainOfExpertise;

        private DateParameterType dateParameterType;

        private SimpleQuantityKind simpleQuantityKind;

        private BooleanParameterType booleanParameterType;

        private RatioScale ratioScale;

        [SetUp]
        public void SetUp()
        {
            this.domainOfExpertise = new DomainOfExpertise { ShortName = "SYS", Name = "System" };
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(It.IsAny<Iteration>()))
                .Returns(this.domainOfExpertise);

            this.engineeringModelSetup = new EngineeringModelSetup { ShortName = "TST", Name = "Test" };
            this.iterationSetup = new IterationSetup { IterationNumber = 1 };
            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);

            this.engineeringModel = new EngineeringModel { EngineeringModelSetup = this.engineeringModelSetup };
            this.iteration = new Iteration { IterationSetup = this.iterationSetup };
            this.engineeringModel.Iteration.Add(this.iteration);

            this.dateParameterType = new DateParameterType { ShortName = "LD", Name = "Launch Date" };
            this.ratioScale = new RatioScale{ ShortName = "kg", Name = "kilogram" };
            this.simpleQuantityKind = new SimpleQuantityKind { ShortName = "m", Name = "mass" };
            this.booleanParameterType = new BooleanParameterType { ShortName = "C", Name = "Compliant" };
        }

        [Test]
        public void Verify_that_RequirementArray_is_populated_as_expected()
        {
            var launchDate = new SimpleParameterValue {ParameterType = this.dateParameterType, Scale = null, Value = new ValueArray<string>(new List<string> { "2050-12-12" }) };
            var launchMass = new SimpleParameterValue { ParameterType = this.simpleQuantityKind, Scale = this.ratioScale, Value = new ValueArray<string>(new List<string> { "100000000" }) };
            var MR_1_compliance = new SimpleParameterValue { ParameterType = this.booleanParameterType, Scale = null, Value = new ValueArray<string>(new List<string> { "True" }) };
            var MR_2_compliance = new SimpleParameterValue { ParameterType = this.booleanParameterType, Scale = null, Value = new ValueArray<string>(new List<string> { "False" }) };
            var SR_1_compliance = new SimpleParameterValue { ParameterType = this.booleanParameterType, Scale = null, Value = new ValueArray<string>(new List<string> { "True" }) };

            var requirementsSpecification_MRD = new RequirementsSpecification
            {
                ShortName = "MRD",
                Name = "Mission Requirements Document",
                Owner = this.domainOfExpertise
            };

            var MR_1 = new Requirement
            {
                ShortName = "MR-001",
                Name = "Launch Date",
                Owner = this.domainOfExpertise
            };

            MR_1.Definition.Add(new Definition
            {
                LanguageCode = "en-GB",
                Content = "The Launch Date shall be in the future"
            });

            MR_1.ParameterValue.Add(launchDate);
            MR_1.ParameterValue.Add(MR_1_compliance);

            var MR_2 = new Requirement
            {
                ShortName = "MR-002",
                Name = "Launch Mass",
                Owner = this.domainOfExpertise
            };

            MR_2.Definition.Add(new Definition
            {
                LanguageCode = "en-GB",
                Content = "The Launch Mass shall be a lot"
            });

            MR_2.ParameterValue.Add(launchMass);
            MR_2.ParameterValue.Add(MR_2_compliance);

            requirementsSpecification_MRD.Requirement.Add(MR_1);
            requirementsSpecification_MRD.Requirement.Add(MR_2);

            var requirementsSpecification_SRD = new RequirementsSpecification
            {
                ShortName = "SRD",
                Name = "System Requirements Document",
                Owner = this.domainOfExpertise
            };

            var SR_1 = new Requirement
            {
                ShortName = "SR-001",
                Name = "Awesomeness",
                Owner = this.domainOfExpertise
            };

            SR_1.Definition.Add(new Definition
            {
                LanguageCode = "en-GB",
                Content = "The System shall be awesome"
            });
            
            SR_1.ParameterValue.Add(SR_1_compliance);

            requirementsSpecification_SRD.Requirement.Add(SR_1);

            this.iteration.RequirementsSpecification.Add(requirementsSpecification_MRD);
            this.iteration.RequirementsSpecification.Add(requirementsSpecification_SRD);

            var requirements = iteration.RequirementsSpecification.SelectMany(x => x.Requirement); ;
            var requirementArrayAssembler = new RequirementArrayAssembler(requirements);

            Assert.That(requirementArrayAssembler.ContentArray[0, 0], Is.EqualTo("Specification"));
            Assert.That(requirementArrayAssembler.ContentArray[0, 1], Is.EqualTo("Group"));
            Assert.That(requirementArrayAssembler.ContentArray[0, 2], Is.EqualTo("Short Name"));
            Assert.That(requirementArrayAssembler.ContentArray[0, 3], Is.EqualTo("Name"));
            Assert.That(requirementArrayAssembler.ContentArray[0, 4], Is.EqualTo("Definition"));
            Assert.That(requirementArrayAssembler.ContentArray[0, 5], Is.EqualTo("Owner"));
            Assert.That(requirementArrayAssembler.ContentArray[0, 6], Is.EqualTo("Category"));
            Assert.That(requirementArrayAssembler.ContentArray[0, 7], Is.EqualTo("C"));
            Assert.That(requirementArrayAssembler.ContentArray[0, 8], Is.EqualTo("LD"));
            Assert.That(requirementArrayAssembler.ContentArray[0, 9], Is.EqualTo("m [kg]"));

            Assert.That(requirementArrayAssembler.ContentArray[1, 0], Is.EqualTo("Mission Requirements Document [MRD]"));
            Assert.That(requirementArrayAssembler.ContentArray[2, 0], Is.EqualTo("Mission Requirements Document [MRD]"));
            Assert.That(requirementArrayAssembler.ContentArray[3, 0], Is.EqualTo("System Requirements Document [SRD]"));

            Assert.That(requirementArrayAssembler.ContentArray[1, 1], Is.EqualTo("-"));
            Assert.That(requirementArrayAssembler.ContentArray[2, 1], Is.EqualTo("-"));
            Assert.That(requirementArrayAssembler.ContentArray[3, 1], Is.EqualTo("-"));

            Assert.That(requirementArrayAssembler.ContentArray[1, 2], Is.EqualTo("MR-001"));
            Assert.That(requirementArrayAssembler.ContentArray[2, 2], Is.EqualTo("MR-002"));
            Assert.That(requirementArrayAssembler.ContentArray[3, 2], Is.EqualTo("SR-001"));

            Assert.That(requirementArrayAssembler.ContentArray[1, 3], Is.EqualTo("Launch Date"));
            Assert.That(requirementArrayAssembler.ContentArray[2, 3], Is.EqualTo("Launch Mass"));
            Assert.That(requirementArrayAssembler.ContentArray[3, 3], Is.EqualTo("Awesomeness"));

            Assert.That(requirementArrayAssembler.ContentArray[1, 4], Is.EqualTo("The Launch Date shall be in the future"));
            Assert.That(requirementArrayAssembler.ContentArray[2, 4], Is.EqualTo("The Launch Mass shall be a lot"));
            Assert.That(requirementArrayAssembler.ContentArray[3, 4], Is.EqualTo("The System shall be awesome"));

            Assert.That(requirementArrayAssembler.ContentArray[1, 5], Is.EqualTo("SYS"));
            Assert.That(requirementArrayAssembler.ContentArray[2, 5], Is.EqualTo("SYS"));
            Assert.That(requirementArrayAssembler.ContentArray[3, 5], Is.EqualTo("SYS"));

            Assert.That(requirementArrayAssembler.ContentArray[1, 6], Is.EqualTo(""));
            Assert.That(requirementArrayAssembler.ContentArray[2, 6], Is.EqualTo(""));
            Assert.That(requirementArrayAssembler.ContentArray[3, 6], Is.EqualTo(""));

            Assert.That(requirementArrayAssembler.ContentArray[1, 7], Is.EqualTo("True"));
            Assert.That(requirementArrayAssembler.ContentArray[2, 7], Is.EqualTo("False"));
            Assert.That(requirementArrayAssembler.ContentArray[3, 7], Is.EqualTo("True"));

            Assert.That(requirementArrayAssembler.ContentArray[1, 8], Is.EqualTo("2050-12-12"));
            Assert.That(requirementArrayAssembler.ContentArray[2, 8], Is.Null);
            Assert.That(requirementArrayAssembler.ContentArray[3, 8], Is.Null);

            Assert.That(requirementArrayAssembler.ContentArray[1, 9], Is.Null);
            Assert.That(requirementArrayAssembler.ContentArray[3, 9], Is.Null);
            Assert.That(requirementArrayAssembler.ContentArray[3, 9], Is.Null);
        }
    }
}