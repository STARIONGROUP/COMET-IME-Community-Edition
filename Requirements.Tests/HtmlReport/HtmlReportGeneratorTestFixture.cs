// -------------------------------------------------------------------------------------------------
// <copyright file="HtmlReportGeneratorTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.HtmlReport
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Requirements.ViewModels.HtmlReport;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="HtmlReportGenerator"/> class
    /// </summary>
    [TestFixture]
    public class HtmlReportGeneratorTestFixture
    {
        private HtmlReportGenerator reportGenerator;
        private RequirementsSpecification requirementsSpecification;
        
        [SetUp]
        public void SetUp()
        {
            var category = new Category(Guid.NewGuid(), null, null) { ShortName = "Refinements" };
            
            var iteration = new Iteration(Guid.NewGuid(), null, null);
            var iterationSetup = new IterationSetup(Guid.NewGuid(), null, null) { Description = "This is an iteration setup", IterationNumber = 1 };

            iteration.IterationSetup = iterationSetup;

            this.requirementsSpecification = new RequirementsSpecification() { ShortName = "TST", Name = "TEST" };
            iteration.RequirementsSpecification.Add(this.requirementsSpecification);

            var requirement_01 = new Requirement(Guid.NewGuid(), null, null) { ShortName = "REQ_1", Name = "Requirement 1" };
            this.requirementsSpecification.Requirement.Add(requirement_01);

            var definition_01 = new Definition(Guid.NewGuid(), null, null) { Content = "this is the english text definition one", LanguageCode = "en" };
            requirement_01.Definition.Add(definition_01);

            var requirement_02 = new Requirement(Guid.NewGuid(), null, null) { ShortName = "REQ_2", Name = "Requirement 2" };
            this.requirementsSpecification.Requirement.Add(requirement_02);

            var definition_02 = new Definition(Guid.NewGuid(), null, null) { Content = "this is the english text definition two", LanguageCode = "en-GB" };
            requirement_02.Definition.Add(definition_02);

            var requirement_03 = new Requirement(Guid.NewGuid(), null, null) { ShortName = "REQ_3", Name = "Requirement 3" , IsDeprecated = true };
            this.requirementsSpecification.Requirement.Add(requirement_03);

            var definition_03 = new Definition(Guid.NewGuid(), null, null) { Content = "this is the english text definition three", LanguageCode = "en" };
            requirement_03.Definition.Add(definition_03);


            var binaryRelationship = new BinaryRelationship(Guid.NewGuid(), null, null);
            binaryRelationship.Source = requirement_01;
            binaryRelationship.Target = requirement_02;
            binaryRelationship.Category.Add(category);

            iteration.Relationship.Add(binaryRelationship);
        }
       
        [Test]
        public void VerifyThatTheHTMLReportIsRenderedAsExpected()
        {
            this.reportGenerator = new HtmlReportGenerator();

            var path = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "requirements.html");
            var html = this.reportGenerator.Render(this.requirementsSpecification);

            Console.WriteLine(html);
            Assert.IsNotEmpty(html);            
        }

        [Test]
        public void VerifyThatArgumentNullExceptionIsThrownOnReder()
        {
            this.reportGenerator = new HtmlReportGenerator();
            Assert.Throws<ArgumentNullException>(() => this.reportGenerator.Render(null));
        }
    }
}
