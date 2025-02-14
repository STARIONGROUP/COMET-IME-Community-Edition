// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationTrackParameterTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Dashboard.Tests.ViewModels
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
        private CDPMessageBus messageBus;
        public IterationTrackParameter iterationTrackParameterForParameter;
        public IterationTrackParameter iterationTrackParameterForParameterOverride;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.messageBus = new CDPMessageBus();

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
            this.messageBus.ClearSubscriptions();
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
