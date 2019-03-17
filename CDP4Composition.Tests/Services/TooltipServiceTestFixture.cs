// -------------------------------------------------------------------------------------------------
// <copyright file="TooltipService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

using System;
using System.Text;

namespace CDP4Composition.Tests.Services
{
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
            expectedToolTip.AppendLine("Category: EQT PROD");
            expectedToolTip.AppendLine("Model Code: Bat");
            expectedToolTip.AppendLine("Definition [en-GB]: this is a definition");
            expectedToolTip.Append("Type: ElementDefinition");
            
            Assert.That(tooltip, Is.EqualTo(expectedToolTip.ToString()));
        }
    }
}