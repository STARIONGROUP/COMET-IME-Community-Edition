// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterStateRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace ProductTree.Tests.ProductTreeRows
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Services.NestedElementTreeService;

    using CDP4Dal;

    using CDP4ProductTree.ViewModels;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class ParameterStateRowViewModelTestFixture
    {
        private Mock<IServiceLocator> serviceLocator;
        private Mock<INestedElementTreeService> nestedElementTreeService;
        private Mock<ISession> session;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private Parameter parameter1;
        private ParameterType parameterType1;
        private ActualFiniteStateList stateList;
        private ActualFiniteState state1;
        private ParameterValueSet valueset;
        private ElementDefinition elementdef1;
        private Iteration iteration;
        private IterationSetup iterationSetup;
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private SiteDirectory siteDirectory;
        private Participant participant;
        private Person person;
        private Option option;
        private DomainOfExpertise domain;
        private ParameterRowViewModel parameterRowViewModel;
        private readonly string nestedParameterPath = "PATH";
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.messageBus = new CDPMessageBus();
            this.nestedElementTreeService = new Mock<INestedElementTreeService>();
            this.nestedElementTreeService.Setup(x => x.GetNestedParameterPath(It.IsAny<ParameterBase>(), It.IsAny<Option>(), It.IsAny<ActualFiniteState>())).Returns(this.nestedParameterPath);

            this.serviceLocator.Setup(x => x.GetInstance<INestedElementTreeService>()).Returns(this.nestedElementTreeService.Object);

            this.session = new Mock<ISession>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain", ShortName = "dom" };
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "test", Surname = "test" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person };
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.elementdef1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);

            this.siteDirectory.Model.Add(this.modelSetup);
            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);
            this.siteDirectory.Person.Add(this.person);

            this.model.Iteration.Add(this.iteration);
            this.model.EngineeringModelSetup = this.modelSetup;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Option.Add(this.option);
            this.iteration.Element.Add(this.elementdef1);

            this.parameterType1 = new EnumerationParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "pt1" };
            this.parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.parameterType1, Owner = this.domain };
            this.stateList = new ActualFiniteStateList(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.state1 = new ActualFiniteState(Guid.NewGuid(), this.cache, this.uri);
            this.valueset = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);
            this.stateList.ActualState.Add(this.state1);
            this.elementdef1.Parameter.Add(this.parameter1);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.cache.TryAdd(new CacheKey(this.parameter1.Iid, null), new Lazy<Thing>(() => this.parameter1));
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            this.parameterRowViewModel = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);
            var row = new ParameterStateRowViewModel(this.parameter1, this.state1, this.session.Object, this.parameterRowViewModel);

            Assert.AreEqual(row.ActualState, this.state1);
            Assert.AreEqual(row.IsDefault, row.ActualState.IsDefault);
            Assert.AreSame(this.state1.Name, row.ActualState.Name);
        }

        [Test]
        public void VerifyThatGetPathWorks()
        {
            this.parameterRowViewModel = new ParameterRowViewModel(this.parameter1, this.option, this.session.Object, null);
            var row = new ParameterStateRowViewModel(this.parameter1, this.state1, this.session.Object, this.parameterRowViewModel);

            Assert.AreEqual(this.nestedParameterPath, row.GetPath());
        }

        [Test]
        public void VerifyThatSetScalarValueIsProperly()
        {
            var published = new ValueArray<string>(new List<string> { "manual" }, this.valueset);
            var actual = new ValueArray<string>(new List<string> { "manual" }, this.valueset);

            this.valueset.Published = published;
            this.valueset.Manual = actual;
            this.valueset.ValueSwitch = ParameterSwitchKind.MANUAL;
            this.valueset.ActualOption = this.option;

            this.parameter1.ValueSet.Add(this.valueset);
            this.parameter1.IsOptionDependent = true;

            var row = new ParameterStateRowViewModel(this.parameter1, this.state1, this.session.Object, this.parameterRowViewModel);
            row.SetScalarValue(this.valueset);

            Assert.AreEqual(row.Switch, this.valueset.ValueSwitch);
            Assert.AreEqual(row.ModelCode, this.valueset.ModelCode());
        }
    }
}
