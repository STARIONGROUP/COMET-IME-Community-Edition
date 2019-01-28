// ------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionChildRowComparerTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------

namespace ProductTree.Tests.Comparers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4ProductTree.Comparers;
    using CDP4ProductTree.ViewModels;
    using Moq;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Integrated with ReactiveList.SortInsert(row, comparer) 
    /// </summary>
    [TestFixture]
    internal class ElementDefinitionChildRowComparerTestFixture
    {
        private Mock<ISession> session;
        private readonly Uri uri = new Uri("http://test.com");

        private SiteDirectory siteDir;
        private EngineeringModel model;
        private Iteration iteration;
        private EngineeringModelSetup modelSetup;
        private IterationSetup iterationSetup;
        private Person person;
        private Participant participant;
        private Option option;
        private ElementDefinition elementDef;
        private ElementDefinition elementDef2;
        private DomainOfExpertise domain;
        private ElementUsage elementUsage;
        private ParameterType type;
        private ParameterValueSet valueSet;

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };

            this.siteDir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri);
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.elementDef = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.type = new EnumerationParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "a" };
            this.valueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Published = new ValueArray<string>(new List<string> {"1"}),
                Manual = new ValueArray<string>(new List<string> { "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.elementDef2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { ElementDefinition = this.elementDef2, Owner = this.domain };

            this.siteDir.Person.Add(this.person);
            this.siteDir.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.participant.Person = this.person;

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration.TopElement = this.elementDef;
            this.iteration.Element.Add(this.elementDef);
            this.iteration.Element.Add(this.elementDef2);
            this.elementDef.ContainedElement.Add(this.elementUsage);

            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRowsAreInsertedCorrectly()
        {
            var list = new ReactiveList<IRowViewModelBase<Thing>>();
            var comparer = new ElementDefinitionChildRowComparer();

            var parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.type, Owner = this.domain, Container = this.elementDef};
            parameter1.ValueSet.Add(this.valueSet);

            var parameterRow1 = new ParameterRowViewModel(parameter1, this.option, this.session.Object, null);

            var typeClone = this.type.Clone(false);
            typeClone.Name = "b";
            var paraClone = parameter1.Clone(false);
            paraClone.ParameterType = typeClone;
            
            var parameterRow2 = new ParameterRowViewModel(paraClone, this.option, this.session.Object, null) { Name = "b" };

            var group1 = new ParameterGroup(Guid.NewGuid(), null, this.uri) { Name = "a" };
            var groupRow1 = new ParameterGroupRowViewModel(group1, this.session.Object, null);

            var group2 = new ParameterGroup(Guid.NewGuid(), null, this.uri) { Name = "b" };
            var groupRow2 = new ParameterGroupRowViewModel(group2, this.session.Object, null);

            var usage1 = this.elementUsage.Clone(false);
            usage1.Name = "def";
            var usageRow1 = new ElementUsageRowViewModel(usage1, this.option, this.session.Object, null);

            var usage2 = this.elementUsage.Clone(false);
            usage2.Name = "abc";
            var usageRow2 = new ElementUsageRowViewModel(usage2, this.option, this.session.Object, null);

            var usage3 = this.elementUsage.Clone(false);
            usage3.Name = "ghi";
            var usageRow3 = new ElementUsageRowViewModel(usage3, this.option, this.session.Object, null);

            list.SortedInsert(usageRow1, comparer);
            list.SortedInsert(usageRow3, comparer);

            list.SortedInsert(parameterRow1, comparer);
            list.SortedInsert(groupRow1, comparer);
            list.SortedInsert(parameterRow2, comparer);
            list.SortedInsert(groupRow2, comparer);

            list.SortedInsert(usageRow2, comparer);

            Assert.AreSame(parameterRow1, list[0]);
            Assert.AreSame(parameterRow2, list[1]);
            Assert.AreSame(groupRow1, list[2]);
            Assert.AreSame(groupRow2, list[3]);
            Assert.AreSame(usageRow2, list[4]);
            Assert.AreSame(usageRow1, list[5]);
            Assert.AreSame(usageRow3, list[6]);
        }

        [Test]
        public void VerifyThatExceptionThrown()
        {
            var list = new ReactiveList<IRowViewModelBase<Thing>>();
            var comparer = new ElementDefinitionChildRowComparer();

            Assert.Throws<ArgumentNullException>(() => list.SortedInsert(null, comparer));
        }

        [Test]
        public void VerifyThatExceptionThrownNullComparer()
        {
            var list = new ReactiveList<IRowViewModelBase<Thing>>();

            var usage1 = this.elementUsage.Clone(false);
            usage1.Name = "def";
            var usageRow1 = new ElementUsageRowViewModel(usage1, this.option, this.session.Object, null);

            Assert.Throws<ArgumentNullException>(() => list.SortedInsert(usageRow1, null));
        }
    }
}