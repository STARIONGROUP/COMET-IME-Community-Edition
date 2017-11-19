// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProperyHelperTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ParameterSheetGenerator.Tests.RowModels.ParameterSheet
{
    using System;
    using System.Collections.Concurrent;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4ParameterSheetGenerator.RowModels;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ProperyHelper"/> class
    /// </summary>
    [TestFixture]
    public class PropertyHelperTestFixture
    {
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;
        private Uri uri;
        private SimpleQuantityKind length;
        private DomainOfExpertise systemEngineering;
        private DomainOfExpertise powerEngineering;
        private Iteration iteration;
        private Option option;
        private ElementDefinition satellite;
        private ElementDefinition battery;
        private ElementUsage batteryUsage;
        private Parameter parameter;
        private ParameterValueSet parameterValueSet;
        private ParameterSubscription parameterSubscription;
        private ParameterSubscriptionValueSet parameterSubscriptionValueSet;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>>();

            this.uri = new Uri("http://www.rheagroup.com");

            this.length = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "l",
                Name = "Length"
            };

            this.systemEngineering = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "SYS",
                Name = "System Engineering"
            };

            this.powerEngineering = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "PWR",
                Name = "Power Engineering"
            };

            this.parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri)
                                 {
                                     Owner = this.systemEngineering,
                                     ParameterType = this.length
                                 };
            this.parameterValueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.parameter.ValueSet.Add(this.parameterValueSet);
            
            this.parameterSubscription = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri)
                                             {
                                                 Owner = this.powerEngineering
                                             };
            this.parameter.ParameterSubscription.Add(this.parameterSubscription);

            this.parameterSubscriptionValueSet = new ParameterSubscriptionValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.parameterSubscriptionValueSet.SubscribedValueSet = this.parameterValueSet;
            this.parameterSubscription.ValueSet.Add(this.parameterSubscriptionValueSet);
        }

        [Test]
        public void VerifyThatComputeContainerOwnerShortNameReturnsExpectedResultForParameterSubscription()
        {
            Assert.AreEqual("SYS", PropertyHelper.ComputeContainerOwnerShortName(this.parameterSubscription)); 
        }

        [Test]
        public void VerifyThatComputeContainerOwnerShortNameReturnsExpectedResultForParameterSubscriptionValueSet()
        {
            Assert.AreEqual("SYS", PropertyHelper.ComputeContainerOwnerShortName(this.parameterSubscriptionValueSet));
        }
    }
}
