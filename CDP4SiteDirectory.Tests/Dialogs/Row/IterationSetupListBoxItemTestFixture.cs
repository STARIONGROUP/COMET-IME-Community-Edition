// -------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupListBoxItemTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs.Row
{
    using System;
    using System.Collections.Concurrent;
    using CDP4Common.CommonData;    
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4SiteDirectory.ViewModels;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="IterationSetupListBoxItem"/> class.
    /// </summary>
    [TestFixture]
    public class IterationSetupListBoxItemTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private IterationSetup iteration;

        private IterationSetupListBoxItem iterationSetupListBoxItem;

        [SetUp]
        public void Setup()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.iteration = new IterationSetup(Guid.NewGuid(), this.cache, null);

            this.iterationSetupListBoxItem = new IterationSetupListBoxItem(this.iteration);
        }

        [Test]
        public void Verify_that_IsActive_returns_expected_result()
        {
            this.iteration.FrozenOn = null;
            Assert.IsTrue(this.iterationSetupListBoxItem.IsActive);

            this.iteration.FrozenOn = DateTime.Now;
            Assert.IsFalse(this.iterationSetupListBoxItem.IsActive);
        }

        [Test]
        public void Verify_that_IterationNumber_returns_expected_result()
        {
            this.iteration.IterationNumber = 1;
            Assert.AreEqual(1, this.iterationSetupListBoxItem.IterationNumber);
        }
    }
}