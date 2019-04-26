// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Dialogs.Rows
{
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4EngineeringModel.ViewModels.Dialogs.Rows;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CategoryRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class CategoryRowViewModelTestFixture
    {
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private Category productCategory;
        private Category elementDefinitionCategory;
        private Category elementUsageCategory;
        private ElementDefinition elementDefinition;
        private ElementUsage elementUsage;
        
        [SetUp]
        public void SetUp()
        {
            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary {ShortName = "Gemeric"};
            

            this.productCategory = new Category { Name = "Product", ShortName = "PROD", IsAbstract = true};
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.productCategory);
            
            this.elementDefinitionCategory = new Category { Name = "EDCategory", ShortName = "ED_CAT"};
            this.elementDefinitionCategory.SuperCategory.Add(this.productCategory);
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.elementDefinitionCategory);

            this.elementUsageCategory = new Category { Name = "EUCategory", ShortName = "EU_CAT"};
            this.elementUsageCategory.SuperCategory.Add(this.productCategory);
            this.siteReferenceDataLibrary.DefinedCategory.Add(this.elementUsageCategory);

            this.elementDefinition = new ElementDefinition();
            this.elementDefinition.Category.Add(this.elementDefinitionCategory);

            this.elementUsage = new ElementUsage {ElementDefinition = this.elementDefinition};
            this.elementUsage.Category.Add(this.elementUsageCategory);
        }

        [Test]
        public void Verify_that_ED_properties_are_set_when_rowviewmodel_is_constructed()
        {
            var rowViewModel = new CategoryRowViewModel(this.elementDefinitionCategory, this.elementDefinition);

            Assert.That(rowViewModel.Category, Is.EqualTo(this.elementDefinitionCategory));
            Assert.That(rowViewModel.Name, Is.EqualTo("EDCategory"));
            Assert.That(rowViewModel.ShortName, Is.EqualTo("ED_CAT"));
            Assert.That(rowViewModel.SuperCategories, Is.EqualTo("PROD"));
            Assert.That(rowViewModel.ContainerRdl, Is.EqualTo("Gemeric"));
            Assert.That(rowViewModel.Level, Is.EqualTo("ED"));
        }

        [Test]
        public void Verify_that_EU_properties_are_set_when_rowviewmodel_is_constructed()
        {
            var rowViewModel = new CategoryRowViewModel(this.elementUsageCategory, this.elementUsage);

            Assert.That(rowViewModel.Category, Is.EqualTo(this.elementUsageCategory));
            Assert.That(rowViewModel.Name, Is.EqualTo("EUCategory"));
            Assert.That(rowViewModel.ShortName, Is.EqualTo("EU_CAT"));
            Assert.That(rowViewModel.SuperCategories, Is.EqualTo("PROD"));
            Assert.That(rowViewModel.ContainerRdl, Is.EqualTo("Gemeric"));
            Assert.That(rowViewModel.Level, Is.EqualTo("EU"));
        }
    }
}