// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherBaseTestData.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Grapher.Tests.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;

    using CDP4Grapher.ViewModels;

    using Moq;

    public class GrapherBaseTestData
    {
        protected ElementDefinition TopElement;
        protected ElementDefinition ElementDefinition1;
        protected ElementDefinition ElementDefinition2;
        protected ElementDefinition ElementDefinition3;

        protected ElementUsage ElementUsage1;
        protected ElementUsage ElementUsage2;
        protected ElementUsage ElementUsage3;

        protected DomainOfExpertise Domain;
        protected Person Person;
        protected Participant Participant;
        protected EngineeringModelSetup EngineeringModelSetup;
        protected EngineeringModel EngineeringModel;
        protected IterationSetup IterationSetup;
        protected Iteration Iteration;
        protected ElementUsage ElementUsage;
        protected NestedElement NestedElement;
        protected ConcurrentDictionary<CacheKey, Lazy<Thing>> Cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
        protected Uri Uri = new Uri("http://test.org");

        public Option Option;
        public Mock<ISession> Session;

        public virtual void Setup()
        {
            this.Domain = new DomainOfExpertise(Guid.NewGuid(), this.Cache, this.Uri)
            {
                ShortName = "test"
            };

            this.ElementUsage = new ElementUsage(Guid.NewGuid(), this.Cache, this.Uri)
            {
                Name = "testName",
                ShortName = "testShortName",
                Owner = this.Domain,
                Category = new List<Category>() { new Category(Guid.NewGuid(), this.Cache, this.Uri) { ShortName = "Test" } }
            };

            this.NestedElement = new NestedElement(Guid.NewGuid(), this.Cache, this.Uri)
            {
                RootElement = this.TopElement, Container = this.Option,
                ElementUsage = new OrderedItemList<ElementUsage>(null) { this.ElementUsage }
            };

            this.Person = new Person(Guid.NewGuid(), this.Cache, this.Uri)
            {
                DefaultDomain = this.Domain
            };

            this.Participant = new Participant(Guid.NewGuid(), this.Cache, this.Uri);
            
            this.Option = new Option(Guid.NewGuid(), this.Cache, this.Uri)
            {
                Name = "TestOption"
            };

            this.SetupElements();

            this.EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.Cache, this.Uri)
            {
                Name = "test",
                Participant = { new Participant(Guid.NewGuid(), this.Cache, this.Uri) { Person = this.Person } }
            };

            this.EngineeringModel = new EngineeringModel(Guid.NewGuid(), this.Cache, this.Uri)
            {
                EngineeringModelSetup = this.EngineeringModelSetup
            };

            this.IterationSetup = new IterationSetup(Guid.NewGuid(), this.Cache, this.Uri)
            {
                IterationNumber = int.MaxValue
            };

            this.Iteration = new Iteration(Guid.NewGuid(), this.Cache, this.Uri)
            {
                IterationSetup = this.IterationSetup,
                TopElement = this.TopElement
            };

            this.Iteration.Option.Add(this.Option);
            this.EngineeringModel.Iteration.Add(this.Iteration);
            this.Session = new Mock<ISession>();
            this.Session.Setup(x => x.ActivePerson).Returns(this.Person);
            this.Session.Setup(x => x.QueryCurrentDomainOfExpertise()).Returns(this.Domain);
            this.Session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>() { { this.Iteration, new Tuple<DomainOfExpertise, Participant>(this.Domain, this.Participant) } });
        }

        private void SetupElements()
        {
            this.TopElement = new ElementDefinition(Guid.NewGuid(), this.Cache, this.Uri) { Owner = this.Domain, Container = this.Option };

            this.ElementDefinition1 = new ElementDefinition(Guid.NewGuid(), this.Cache, this.Uri) { Owner = this.Domain };
            this.ElementDefinition2 = new ElementDefinition(Guid.NewGuid(), this.Cache, this.Uri) { Owner = this.Domain };
            this.ElementDefinition3 = new ElementDefinition(Guid.NewGuid(), this.Cache, this.Uri) { Owner = this.Domain };

            this.ElementUsage1 = new ElementUsage(Guid.NewGuid(), this.Cache, this.Uri) { Owner = this.Domain, ElementDefinition = this.ElementDefinition1, Container = this.Option };
            this.ElementUsage2 = new ElementUsage(Guid.NewGuid(), this.Cache, this.Uri) { Owner = this.Domain, ElementDefinition = this.ElementDefinition2, Container = this.Option };
            this.ElementUsage3 = new ElementUsage(Guid.NewGuid(), this.Cache, this.Uri) { Owner = this.Domain, ElementDefinition = this.ElementDefinition3, Container = this.Option };
        }
    }
}
