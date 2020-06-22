// -------------------------------------------------------------------------------------------------
// <copyright file=IterationTrackParameterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.ViewModels.Rows
{
    using System;
    using System.Reactive.Concurrency;
    using System.Threading;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4Dashboard.ViewModels.Widget;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Tests if <see cref="IterationTrackParameter"/> works correctly
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class IterationTrackParameterTestFixture
    {
        public Parameter parameter;
        public ParameterOverride parameterOverride;
        private ElementDefinition elementDefinition;
        private ElementUsage elementUsage;
        private MeasurementScale scale;
        private QuantityKind simpleQuantityKind;
        private Iteration iteration;
        public IterationTrackParameter iterationTrackParameterForParameter;
        public IterationTrackParameter iterationTrackParameterForParameterOverride;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.iteration = new Iteration(Guid.NewGuid(), null, null);

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null)
            {
                ShortName = "ED"
            };

            this.iteration.Element.Add(this.elementDefinition);

            this.elementUsage = new ElementUsage(Guid.NewGuid(), null, null)
            {
                ElementDefinition = this.elementDefinition,
                ShortName = "EU"
            };

            this.elementDefinition.ContainedElement.Add(this.elementUsage);

            this.simpleQuantityKind = new SimpleQuantityKind(Guid.NewGuid(), null, null) { ShortName = "SQ" };

            this.scale = new RatioScale(Guid.NewGuid(), null, null) { ShortName = "SC" };

            this.parameter = new Parameter(Guid.NewGuid(), null, null)
            {
                ParameterType = this.simpleQuantityKind,
                Scale = this.scale,
                Container = this.elementDefinition
            };

            this.parameterOverride = new ParameterOverride(Guid.NewGuid(), null, null)
            {
                Parameter = this.parameter,
                Container = this.elementUsage
            };

            this.iterationTrackParameterForParameter = new IterationTrackParameter(this.parameter);

            this.iterationTrackParameterForParameterOverride = new IterationTrackParameter(this.parameterOverride);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void verify_that_objects_are_setup_correctly()
        {
            Assert.AreEqual(this.parameter, this.iterationTrackParameterForParameter.ParameterOrOverride);
            Assert.AreEqual(this.parameterOverride, this.iterationTrackParameterForParameterOverride.ParameterOrOverride);

            Assert.AreEqual($"{this.elementDefinition.ShortName}.{this.simpleQuantityKind.ShortName}", this.iterationTrackParameterForParameter.ModelCode);
            Assert.AreEqual($"{this.elementDefinition.ShortName}.{this.elementUsage.ShortName}.{this.simpleQuantityKind.ShortName}", this.iterationTrackParameterForParameterOverride.ModelCode);

            Assert.AreEqual(this.scale.UserFriendlyShortName, this.iterationTrackParameterForParameter.UnitSymbol);
            Assert.AreEqual(this.scale.UserFriendlyShortName, this.iterationTrackParameterForParameterOverride.UnitSymbol);
        }
    }
}
