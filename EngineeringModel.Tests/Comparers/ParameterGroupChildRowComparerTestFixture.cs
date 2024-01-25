// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupChildRowComparerTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.Tests.Comparers
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.Comparers;
    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class ParameterGroupChildRowComparerTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
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
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domain" };

            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.person = new Person(Guid.NewGuid(), null, this.uri);
            this.model = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.participant = new Participant(Guid.NewGuid(), null, this.uri);
            this.option = new Option(Guid.NewGuid(), null, this.uri);
            this.elementDef = new ElementDefinition(Guid.NewGuid(), null, this.uri) { Owner = this.domain };
            this.type = new EnumerationParameterType(Guid.NewGuid(), null, this.uri) { Name = "a" };

            this.valueSet = new ParameterValueSet(Guid.NewGuid(), null, this.uri)
            {
                Published = new ValueArray<string>(new List<string> { "1" }),
                Manual = new ValueArray<string>(new List<string> { "1" }),
                ValueSwitch = ParameterSwitchKind.MANUAL
            };

            this.elementDef2 = new ElementDefinition(Guid.NewGuid(), null, this.uri) { Owner = this.domain };
            this.elementUsage = new ElementUsage(Guid.NewGuid(), null, this.uri) { ElementDefinition = this.elementDef2, Owner = this.domain };

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

            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRowsAreInsertedCorrectly()
        {
            var list = new ReactiveList<IRowViewModelBase<Thing>>();
            var comparer = new ParameterGroupChildRowComparer();

            var parameter1 = new Parameter(Guid.NewGuid(), null, this.uri)
            {
                ParameterType = this.type, Owner = this.domain, Container = this.elementDef
            };

            parameter1.ValueSet.Add(this.valueSet);

            var subscription = new ParameterSubscription(Guid.NewGuid(), null, this.uri);
            parameter1.ParameterSubscription.Add(subscription);

            var parameterRow1 = new ParameterRowViewModel(parameter1, this.session.Object, null, false);

            var group1 = new ParameterGroup(Guid.NewGuid(), null, this.uri) { Name = "a" };
            var groupRow1 = new ParameterGroupRowViewModel(group1, this.domain, this.session.Object, null);

            var group2 = new ParameterGroup(Guid.NewGuid(), null, this.uri) { Name = "b" };
            var groupRow2 = new ParameterGroupRowViewModel(group2, this.domain, this.session.Object, null);

            var subscriptionRow = new ParameterSubscriptionRowViewModel(subscription, this.session.Object, null, false);

            list.SortedInsert(parameterRow1, comparer);
            list.SortedInsert(groupRow1, comparer);
            list.SortedInsert(groupRow2, comparer);
            list.SortedInsert(subscriptionRow, comparer);

            Assert.AreSame(subscriptionRow, list[0]);
            Assert.AreSame(parameterRow1, list[1]);
            Assert.AreSame(groupRow1, list[2]);
            Assert.AreSame(groupRow2, list[3]);
        }
    }
}
