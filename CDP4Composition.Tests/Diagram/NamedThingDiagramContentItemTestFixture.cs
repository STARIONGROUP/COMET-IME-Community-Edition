// -------------------------------------------------------------------------------------------------
// <copyright file="NamedThingDiagramContentItemTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Diagram
{
    using System;
    using System.Threading;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Diagram;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="NamedThingDiagramContentItemViewModel"/> class
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class NamedThingDiagramContentItemTestFixture
    {
        private DomainOfExpertise domainOfExpertise;

        private Iteration iteration;

        [SetUp]
        public void SetUp()
        {
            this.domainOfExpertise = new DomainOfExpertise();
            this.iteration = new Iteration();
        }

        [Test]
        public void VerifyThatNamedWithThingCanBeConstructed()
        {
            var namedThingDiagramContentItem = new NamedThingDiagramContentItemViewModel(this.domainOfExpertise);

            Assert.AreEqual(this.domainOfExpertise, namedThingDiagramContentItem.Thing);
        }
    }
}
