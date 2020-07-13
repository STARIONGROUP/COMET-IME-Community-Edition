namespace CDP4Composition.Tests.Reporting
{
    using System;
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Reporting;

    using NUnit.Framework;

    [TestFixture]
    public class CategoryHierarchyTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private Iteration iteration;

        private Category cat1;
        private Category cat2;
        private Category cat3;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, null);

            #region Categories

            this.cat1 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat1",
                Name = "cat1"
            };

            this.cache.TryAdd(new CacheKey(this.cat1.Iid, null), new Lazy<Thing>(() => this.cat1));

            this.cat2 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat2",
                Name = "cat2"
            };

            this.cache.TryAdd(new CacheKey(this.cat2.Iid, null), new Lazy<Thing>(() => this.cat2));

            this.cat3 = new Category(Guid.NewGuid(), this.cache, null)
            {
                ShortName = "cat3",
                Name = "cat3"
            };

            this.cache.TryAdd(new CacheKey(this.cat3.Iid, null), new Lazy<Thing>(() => this.cat3));

            #endregion
        }

        [Test]
        public void VerifyThatBuilderThrowsOnUnkownCategory()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new CategoryHierarchy.Builder(this.iteration, "unknown_cat"));
        }

        [Test]
        public void VerifyThatBuilderBuids()
        {
            var hierarchy = new CategoryHierarchy
                .Builder(this.iteration, this.cat1.ShortName)
                .Build();

            Assert.IsInstanceOf<CategoryHierarchy>(hierarchy);
        }

        [Test]
        public void VerifyThatCategoryHierarchyIsCorrect()
        {
            var hierarchy = new CategoryHierarchy
                    .Builder(this.iteration, this.cat1.ShortName)
                .AddLevel(this.cat2.ShortName)
                .AddLevel(this.cat3.ShortName)
                .Build();

            Assert.AreEqual(this.cat1, hierarchy.Category);

            hierarchy = hierarchy.Child;
            Assert.AreEqual(this.cat2, hierarchy.Category);

            hierarchy = hierarchy.Child;
            Assert.AreEqual(this.cat3, hierarchy.Category);

            Assert.IsNull(hierarchy.Child);
        }
    }
}
