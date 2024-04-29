// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherBaseTestData.cs" company="Starion Group S.A.">
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

namespace CDP4Grapher.Tests.Data
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

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
        protected Uri Uri = new Uri("http://test.org");
        protected Assembler Assembler;

        public Option Option;
        public Mock<ISession> Session;
        public CDPMessageBus MessageBus;

        public virtual void Setup()
        {
            this.MessageBus = new CDPMessageBus();
            this.Assembler = new Assembler(this.Uri, this.MessageBus);

            this.Domain = new DomainOfExpertise(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                ShortName = "test"
            };

            this.Person = new Person(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                DefaultDomain = this.Domain
            };

            this.Participant = new Participant(Guid.NewGuid(), this.Assembler.Cache, this.Uri);

            this.Option = new Option(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                Name = "TestOption",
            };

            this.SetupElements();

            this.ElementUsage = new ElementUsage(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                Name = "testName",
                ShortName = "testShortName",
                Owner = this.Domain,
                ElementDefinition = this.ElementDefinition1,
                Category = new List<Category>() { new Category(Guid.NewGuid(), this.Assembler.Cache, this.Uri) { ShortName = "Test" } }
            };

            this.NestedElement = new NestedElement(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                RootElement = this.TopElement,
                Container = this.Option,
                ElementUsage = new OrderedItemList<ElementUsage>(null) { this.ElementUsage1 }
            };

            this.EngineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                Name = "test",
                Participant = { new Participant(Guid.NewGuid(), this.Assembler.Cache, this.Uri) { Person = this.Person } }
            };

            this.EngineeringModel = new EngineeringModel(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                EngineeringModelSetup = this.EngineeringModelSetup
            };

            this.IterationSetup = new IterationSetup(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                IterationNumber = int.MaxValue
            };

            this.Iteration = new Iteration(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                IterationSetup = this.IterationSetup,
                TopElement = this.TopElement, DefaultOption = this.Option
            };

            this.Iteration.Option.Add(this.Option);
            this.AddThingsToTheCache();
            this.EngineeringModel.Iteration.Add(this.Iteration);
            this.Session = new Mock<ISession>();
            this.Session.Setup(x => x.Assembler).Returns(this.Assembler);
            this.Session.Setup(x => x.ActivePerson).Returns(this.Person);
            this.Session.Setup(x => x.QuerySelectedDomainOfExpertise(this.Iteration)).Returns(this.Domain);
            this.Session.Setup(x => x.CDPMessageBus).Returns(this.MessageBus);
        }

        private void SetupElements()
        {
            this.TopElement = new ElementDefinition(Guid.NewGuid(), this.Assembler.Cache, this.Uri) { Name = "TopElement", ShortName = "TopElement", Owner = this.Domain, Container = this.Option };

            this.ElementDefinition1 = new ElementDefinition(Guid.NewGuid(), this.Assembler.Cache, this.Uri) { Name = "ElementDefinition1", ShortName = "ElementDefinition1", Owner = this.Domain, Container = this.TopElement };
            this.ElementDefinition2 = new ElementDefinition(Guid.NewGuid(), this.Assembler.Cache, this.Uri) { Name = "ElementDefinition2", ShortName = "ElementDefinition2", Owner = this.Domain, Container = this.TopElement };
            this.ElementDefinition3 = new ElementDefinition(Guid.NewGuid(), this.Assembler.Cache, this.Uri) { Name = "ElementDefinition3", ShortName = "ElementDefinition3", Owner = this.Domain, Container = this.TopElement };

            this.ElementUsage1 = new ElementUsage(Guid.NewGuid(), this.Assembler.Cache, this.Uri)
            {
                Owner = this.Domain, ElementDefinition = this.TopElement, Container = this.ElementDefinition1,
                Category = new List<Category>() { new Category(Guid.NewGuid(), this.Assembler.Cache, this.Uri) { ShortName = "Test" } }
            };

            this.ElementUsage2 = new ElementUsage(Guid.NewGuid(), this.Assembler.Cache, this.Uri) { Owner = this.Domain, ElementDefinition = this.ElementDefinition2, Container = this.ElementDefinition2 };
            this.ElementUsage3 = new ElementUsage(Guid.NewGuid(), this.Assembler.Cache, this.Uri) { Owner = this.Domain, ElementDefinition = this.ElementDefinition3, Container = this.ElementDefinition3 };
        }

        private void AddThingsToTheCache()
        {
            this.Assembler.Cache.TryAdd(new CacheKey(this.TopElement.Iid, this.Iteration.Iid), new Lazy<Thing>(() => this.TopElement));
            this.Assembler.Cache.TryAdd(new CacheKey(this.ElementDefinition1.Iid, this.Iteration.Iid), new Lazy<Thing>(() => this.ElementDefinition1));
            this.Assembler.Cache.TryAdd(new CacheKey(this.ElementDefinition2.Iid, this.Iteration.Iid), new Lazy<Thing>(() => this.ElementDefinition2));
            this.Assembler.Cache.TryAdd(new CacheKey(this.ElementDefinition3.Iid, this.Iteration.Iid), new Lazy<Thing>(() => this.ElementDefinition3));
            this.Assembler.Cache.TryAdd(new CacheKey(this.ElementUsage1.Iid, this.Iteration.Iid), new Lazy<Thing>(() => this.ElementUsage1));
            this.Assembler.Cache.TryAdd(new CacheKey(this.ElementUsage2.Iid, this.Iteration.Iid), new Lazy<Thing>(() => this.ElementUsage2));
            this.Assembler.Cache.TryAdd(new CacheKey(this.ElementUsage3.Iid, this.Iteration.Iid), new Lazy<Thing>(() => this.ElementUsage3));
        }
    }
}
