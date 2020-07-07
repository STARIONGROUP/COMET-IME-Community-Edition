// -------------------------------------------------------------------------------------------------
// <copyright file="ExtendedRibbonPageCategoryTextFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

using CDP4Dal;
using NUnit.Framework;
using ReactiveUI;
using System.Reactive.Concurrency;
using System.Threading;
using CDP4Composition.Ribbon;

namespace CDP4Reporting.Tests.Behaviour
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ExtendedRibbonPageCategoryTextFixture
    {
        private ExtendedRibbonPageCategory extendedRibbonPageCategory;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.extendedRibbonPageCategory = new ExtendedRibbonPageCategory();
        }

        [Test]
        public void VerifyTheDefaultValueOfTheDependencyPropertyContainer()
        {
            Assert.AreEqual(ExtendedRibbonPageCategory.ContainerRegionNameProperty.Name, "ContainerRegionName");
        }

        [Test]
        public void GetSetTheDefaultValueOfTheDependencyPropertyContainer()
        {
            Assert.IsNull(extendedRibbonPageCategory.ContainerRegionName);

            extendedRibbonPageCategory.ContainerRegionName = "abc";

            Assert.AreEqual(extendedRibbonPageCategory.ContainerRegionName, "abc");
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }
    }
}
