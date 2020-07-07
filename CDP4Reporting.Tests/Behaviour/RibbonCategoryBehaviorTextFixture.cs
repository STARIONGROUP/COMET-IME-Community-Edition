using CDP4Dal;
using NUnit.Framework;
using ReactiveUI;
using System.Reactive.Concurrency;
using System.Threading;
using CDP4Composition.Ribbon;
using CDP4Composition.Mvvm.Behaviours;
using Moq;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.Regions;
using CDP4Composition.Navigation;
using System;
using CDP4Composition;
using CDP4Reporting.Views;

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
