// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyElementDefinitionCreatorTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4EngineeringModel.Tests.Utilities
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CDP4DalCommon.Protocol.Operations;

    using CDP4EngineeringModel.Utilities;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CopyElementDefinitionCreator"/> class
    /// </summary>
    [TestFixture]
    public class CopyElementDefinitionCreatorTestFixture
    {
        private Uri uri = new Uri("http://test.com");
        private Mock<ISession> session;
        private Assembler asembler;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private CDPMessageBus messageBus;

        private EngineeringModel model;
        private Iteration iteration;
        private ElementDefinition elementDef1;
        private ElementDefinition elementDef2;
        private Parameter parameter1;
        private ParameterValueSet valueSet1;
        private Parameter parameter2;
        private ParameterValueSet valueSet2;
        private ParameterOverride parameterOverride;
        private ParameterOverrideValueSet overrideValueset;
        private ElementUsage usage;

        private ParameterSubscription sub1;
        private ParameterSubscriptionValueSet subValueset1;
        private ParameterSubscription sub2;
        private ParameterSubscriptionValueSet subValueset2;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.asembler = new Assembler(this.uri, this.messageBus);
            this.cache = this.asembler.Cache;

            this.session.Setup(x => x.Assembler).Returns(this.asembler);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);

            this.elementDef1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            this.elementDef2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);

            this.parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri);
            this.valueSet1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.parameter2 = new Parameter(Guid.NewGuid(), this.cache, this.uri);
            this.valueSet2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.usage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri);

            this.parameterOverride = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri);
            this.parameterOverride.Parameter = this.parameter2;
            this.overrideValueset = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.overrideValueset.ParameterValueSet = this.valueSet2;

            this.model.Iteration.Add(this.iteration);
            this.iteration.Element.Add(this.elementDef1);
            this.iteration.Element.Add(this.elementDef2);

            this.elementDef1.Parameter.Add(this.parameter1);
            this.parameter1.ValueSet.Add(this.valueSet1);

            this.usage.ElementDefinition = this.elementDef2;
            this.usage.ParameterOverride.Add(this.parameterOverride);
            this.parameterOverride.ValueSet.Add(this.overrideValueset);

            this.elementDef1.ContainedElement.Add(this.usage);

            this.sub1 = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri);
            this.sub2 = new ParameterSubscription(Guid.NewGuid(), this.cache, this.uri);
            this.subValueset1 = new ParameterSubscriptionValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.subValueset1.SubscribedValueSet = this.valueSet1;
            this.subValueset2 = new ParameterSubscriptionValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.subValueset2.SubscribedValueSet = this.overrideValueset;

            this.sub1.ValueSet.Add(this.subValueset1);
            this.sub2.ValueSet.Add(this.subValueset2);

            this.parameter1.ParameterSubscription.Add(this.sub1);
            this.parameterOverride.ParameterSubscription.Add(this.sub2);

            this.cache.TryAdd(new CacheKey(this.model.Iid, null), new Lazy<Thing>(() => this.model));
            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.cache.TryAdd(new CacheKey(this.elementDef1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.elementDef1));
            this.cache.TryAdd(new CacheKey(this.elementDef2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.elementDef2));
            this.cache.TryAdd(new CacheKey(this.usage.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.usage));
            this.cache.TryAdd(new CacheKey(this.parameter1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameter1));
            this.cache.TryAdd(new CacheKey(this.parameter2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameter2));
            this.cache.TryAdd(new CacheKey(this.valueSet1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.valueSet1));
            this.cache.TryAdd(new CacheKey(this.valueSet2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.valueSet2));
            this.cache.TryAdd(new CacheKey(this.parameterOverride.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameterOverride));
            this.cache.TryAdd(new CacheKey(this.overrideValueset.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.overrideValueset));
            this.cache.TryAdd(new CacheKey(this.sub1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.sub1));
            this.cache.TryAdd(new CacheKey(this.sub2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.sub2));
            this.cache.TryAdd(new CacheKey(this.subValueset1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.subValueset1));
            this.cache.TryAdd(new CacheKey(this.subValueset2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.subValueset2));
        }

        [Test]
        public void VerifyThatCopyWithUsageWorks()
        {
            var copy = new CopyElementDefinitionCreator(this.session.Object);
            copy.Copy(this.elementDef1, true);
            this.session.Verify(x => x.Write(It.Is<OperationContainer>(c => c.Operations.Count(op => op.OperationKind == OperationKind.Create) == 10)));
        }

        [Test]
        public void VerifyThatCopyWithoutUsageWorks()
        {
            var copy = new CopyElementDefinitionCreator(this.session.Object);
            copy.Copy(this.elementDef1, false);
            this.session.Verify(x => x.Write(It.Is<OperationContainer>(c => c.Operations.Count(op => op.OperationKind == OperationKind.Create) == 5)));
        }
    }
}
