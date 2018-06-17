// -------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupListBoxItemTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Dialogs.Row
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Concurrency;
    using CDP4Common.CommonData;
    using CDP4Common.MetaInfo;
    using CDP4Dal.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Permission;
    using CDP4SiteDirectory.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="IterationSetupListBoxItem"/> class.
    /// </summary>
    [TestFixture]
    public class IterationSetupListBoxItemTestFixture
    {
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

        private IterationSetup iteration;

        private IterationSetupListBoxItem iterationSetupListBoxItem;

        [SetUp]
        public void Setup()
        {
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

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