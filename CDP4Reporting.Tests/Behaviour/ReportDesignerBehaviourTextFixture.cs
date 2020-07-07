// -------------------------------------------------------------------------------------------------
// <copyright file="ReportDesignerBehaviourTextFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

using CDP4Dal;
using CDP4Reporting.Behaviours;
using NUnit.Framework;
using ReactiveUI;
using System.Reactive.Concurrency;
using System.Threading;

namespace CDP4Reporting.Tests.Behaviour
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ReportDesignerBehaviourTextFixture
    {
        private ReportDesignerBehaviour reportDesignerBehaviour;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.reportDesignerBehaviour = new ReportDesignerBehaviour();
        }

        [Test]
        public void VerifyTheDefaultValueOfTheDependencyPropertyContainer()
        {
            Assert.AreEqual(ReportDesignerBehaviour.RibbonMergeCategoryNameProperty.Name, "RibbonMergeCategoryName");
        }

        [Test]
        public void VerifyGetSetRibbonMergeCategoryName()
        {
            Assert.IsNull(reportDesignerBehaviour.RibbonMergeCategoryName);
            reportDesignerBehaviour.RibbonMergeCategoryName = "abc";
            Assert.AreEqual(reportDesignerBehaviour.RibbonMergeCategoryName, "abc");
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }
    }
}
