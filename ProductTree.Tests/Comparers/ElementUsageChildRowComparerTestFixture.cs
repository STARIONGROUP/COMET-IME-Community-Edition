﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageChildRowComparerTestFixture.cs" company="Starion Group S.A.">
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

namespace ProductTree.Tests.Comparers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using CDP4ProductTree.Comparers;
    using CDP4ProductTree.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class ElementUsageChildRowComparerTestFixture
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
        private ParameterType type1;
        private ParameterType type2;
        private ParameterValueSet vs1;
        private Parameter p1;
        private Parameter p2;
        private ParameterOverride po1;
        private ParameterValueSet vs2;
        private ParameterOverrideValueSet ovs1;

        private ParameterGroup gr;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
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
            this.type1 = new EnumerationParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "a" };
            this.type2 = new EnumerationParameterType(Guid.NewGuid(), this.cache, this.uri) { Name = "p" };

            this.vs1 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Published = new ValueArray<string>(new List<string> { "1" }),
                Manual = new ValueArray<string>(new List<string> { "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.vs2 = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Published = new ValueArray<string>(new List<string> { "1" }),
                Manual = new ValueArray<string>(new List<string> { "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.ovs1 = new ParameterOverrideValueSet(Guid.NewGuid(), this.cache, this.uri)
            {
                Published = new ValueArray<string>(new List<string> { "1" }),
                Manual = new ValueArray<string>(new List<string> { "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL,
                ParameterValueSet = this.vs1
            };

            this.gr = new ParameterGroup(Guid.NewGuid(), this.cache, this.uri) { Name = "gr" };

            this.p1 = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.type1 };
            this.p2 = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = this.type2 };
            this.po1 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri) { Parameter = this.p1 };
            this.p1.ValueSet.Add(this.vs1);
            this.p2.ValueSet.Add(this.vs2);
            this.po1.ValueSet.Add(this.ovs1);

            this.elementDef2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { ElementDefinition = this.elementDef2, Owner = this.domain };

            this.elementDef2.Parameter.Add(this.p1);
            this.elementDef2.Parameter.Add(this.p2);
            this.elementDef2.ParameterGroup.Add(this.gr);
            this.elementUsage.ParameterOverride.Add(this.po1);

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
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatElementUsagEcomparerWorksAsExpected()
        {
            var e1Row = new ElementDefinitionRowViewModel(this.elementDef, this.option, this.session.Object, null);

            var usageRow = (ElementUsageRowViewModel)e1Row.ContainedRows.Single();

            Assert.AreEqual(3, usageRow.ContainedRows.Count);
            Assert.AreEqual(usageRow.ContainedRows[0].Thing, this.po1);
            Assert.AreEqual(usageRow.ContainedRows[1].Thing, this.p2);
            Assert.AreEqual(usageRow.ContainedRows[2].Thing, this.gr);
        }

        [Test]
        public void VerifyThatComparerSameAsElementDef()
        {
            var comparer = new ElementUsageChildRowComparer();
            Assert.Throws<ArgumentNullException>(() => comparer.Compare(null, null));
        }
    }
}
