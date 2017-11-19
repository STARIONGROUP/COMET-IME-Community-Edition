// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CopyElementDefinitionCreatorTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.Utilities
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Services;
    using CDP4Dal;    
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
        private ConcurrentDictionary<Tuple<Guid, Guid?>, Lazy<Thing>> cache;

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
            this.session = new Mock<ISession>();            
            this.asembler = new Assembler(this.uri);
            this.cache = this.asembler.Cache;

            this.session.Setup(x => x.Assembler).Returns(this.asembler);

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

            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.model.Iid, null), new Lazy<Thing>(() => this.model));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.elementDef1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.elementDef1));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.elementDef2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.elementDef2));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.usage.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.usage));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.parameter1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameter1));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.parameter2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameter2));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.valueSet1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.valueSet1));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.valueSet2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.valueSet2));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.parameterOverride.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.parameterOverride));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.overrideValueset.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.overrideValueset));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.sub1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.sub1));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.sub2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.sub2));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.subValueset1.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.subValueset1));
            this.cache.TryAdd(new Tuple<Guid, Guid?>(this.subValueset2.Iid, this.iteration.Iid), new Lazy<Thing>(() => this.subValueset2));
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
