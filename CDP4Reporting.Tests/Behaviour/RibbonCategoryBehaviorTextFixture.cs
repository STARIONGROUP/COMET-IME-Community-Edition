// -------------------------------------------------------------------------------------------------
// <copyright file="RibbonCategoryBehaviorTextFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

using CDP4Dal;
using NUnit.Framework;
using ReactiveUI;
using System.Reactive.Concurrency;
using System.Threading;
using CDP4Composition.Mvvm.Behaviours;

namespace CDP4Reporting.Tests.Behaviour
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RibbonCategoryBehaviorTextFixture
    {
        private RibbonCategoryBehavior ribbonCategoryBehavior;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.ribbonCategoryBehavior = new RibbonCategoryBehavior();
        }

        [Test]
        public void VerifyInitializedMembers()
        {
            Assert.IsNotNull(this.ribbonCategoryBehavior.RegionManager);
            Assert.IsNull(this.ribbonCategoryBehavior.RibbonRegion);
            Assert.IsNull(this.ribbonCategoryBehavior.CategoryName);
        }

        [Test]
        public void VerifyTheDefaultValueOfTheDependencyPropertyContainer()
        {
            Assert.AreEqual(RibbonCategoryBehavior.CategoryNameProperty.Name, "CategoryName");
        }

        [Test]
        public void VerifyIfCategoryNameOfTheTargetIsSet()
        {
            this.ribbonCategoryBehavior.CategoryName = "abc";
            Assert.AreEqual(this.ribbonCategoryBehavior.CategoryName, "abc");
        }

        [Test]
        public void VerifyIfTheCategoryNameIsChanged()
        {
            Assert.IsNull(this.ribbonCategoryBehavior.CategoryName);
            RibbonCategoryBehavior.SetCategoryName(this.ribbonCategoryBehavior, "abc");
            Assert.AreEqual(this.ribbonCategoryBehavior.CategoryName, "abc");
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }
    }
}
