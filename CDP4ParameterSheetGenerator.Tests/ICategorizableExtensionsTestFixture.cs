// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICategorizableExtensionsTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ICategorizableExtensionsTestFixture"/> class
    /// </summary>
    [TestFixture]
    public class ICategorizableExtensionsTestFixture
    {
        private ElementDefinition elementDefinition;

        private Category product;

        private Category equipment;

        [SetUp]
        public void SetUp()
        {
            this.product = new Category(Guid.NewGuid(), null, null);
            this.product.ShortName = "PROD";

            this.equipment = new Category(Guid.NewGuid(), null, null);
            this.equipment.ShortName = "EQT";

            this.equipment.SuperCategory.Add(product);

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            
        }

        [Test]
        public void VerifyThatAllCategoriesAreReturned()
        {
            this.elementDefinition.Category.Add(this.equipment);

            var categories = this.elementDefinition.GetAllCategories();

            CollectionAssert.Contains(categories, this.product);
            CollectionAssert.Contains(categories, this.equipment);
        }

        [Test]
        public void VerifytThatAllCategoryShortNamesAreReturned()
        {
            this.elementDefinition.Category.Add(this.equipment);

            Assert.AreEqual("PROD EQT", this.elementDefinition.GetAllCategoryShortNames());
        }
    }
}
