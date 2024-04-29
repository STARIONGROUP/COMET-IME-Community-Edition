// -------------------------------------------------------------------------------------------------
// <copyright file="TooltipService.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Services
{
    using System;
    using System.Text;
    using NUnit.Framework;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Services;

    /// <summary>
    /// Suite of tests for the <see cref="TooltipService"/> class
    /// </summary>
    [TestFixture]
    public class TooltipServiceTestFixture
    {
        private Category equippmentCategory;
        private Category productCategory;
        private DomainOfExpertise domainOfExpertise;

        [SetUp]
        public void SetUp()
        {
            this.equippmentCategory = new Category(Guid.NewGuid(), null, null) {ShortName = "EQT"};
            this.productCategory = new Category(Guid.NewGuid(), null, null) {ShortName = "PROD"};
            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, null) {ShortName = "SYS"};
        }

        [Test]
        public void Verif_that_ElementDefinition_tooltip_returns_expected_result()
        {
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                ShortName = "Bat",
                Name = "Battery",
                Owner = this.domainOfExpertise
            };
            elementDefinition.Category.Add(this.productCategory);
            elementDefinition.Category.Add(this.equippmentCategory);

            var definition = new CDP4Common.CommonData.Definition(Guid.NewGuid(), null, null)
            {
                LanguageCode = "en-GB", Content = "this is a definition"
            };
            elementDefinition.Definition.Add(definition);

            var tooltip = TooltipService.Tooltip(elementDefinition);
            var expectedToolTip = new StringBuilder();
            expectedToolTip.AppendLine("Short Name: Bat");
            expectedToolTip.AppendLine("Name: Battery");
            expectedToolTip.AppendLine("Owner: SYS");
            expectedToolTip.AppendLine("Category: PROD");
            expectedToolTip.AppendLine("          EQT");
            expectedToolTip.AppendLine("Model Code: Bat");
            expectedToolTip.AppendLine("Definition [en-GB]: this is a definition");
            expectedToolTip.Append("Type: ElementDefinition");
            
            Assert.That(tooltip, Is.EqualTo(expectedToolTip.ToString()));
        }

        [Test]
        public void Verif_that_ElementUsage_tooltip_returns_expected_result()
        {
            this.equippmentCategory.SuperCategory.Add(this.productCategory);

            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                ShortName = "Bat",
                Name = "Battery",
                Owner = this.domainOfExpertise
            };            
            elementDefinition.Category.Add(this.equippmentCategory);

            var elementUsage = new ElementUsage(Guid.NewGuid(), null, null)
            {
                ShortName = "bat",
                Name = "battery",
                Owner = this.domainOfExpertise,
                ElementDefinition = elementDefinition
            };

            var definition = new CDP4Common.CommonData.Definition(Guid.NewGuid(), null, null)
            {
                LanguageCode = "en-GB", Content = "this is a definition"
            };
            elementUsage.Definition.Add(definition);

            var tooltip = TooltipService.Tooltip(elementUsage);
            var expectedToolTip = new StringBuilder();
            expectedToolTip.AppendLine("Short Name: bat");
            expectedToolTip.AppendLine("Name: battery");
            expectedToolTip.AppendLine("Owner: SYS");
            expectedToolTip.AppendLine("Category: -");
            expectedToolTip.AppendLine("ED Category: EQT {PROD}");            
            expectedToolTip.AppendLine("Model Code: Invalid Model Code");
            expectedToolTip.AppendLine("Definition [en-GB]: this is a definition");
            expectedToolTip.Append("Type: ElementUsage");
            
            Assert.That(tooltip, Is.EqualTo(expectedToolTip.ToString()));
        }
    }
}