// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherRibbonViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Grapher.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;

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

    using NUnit.Framework;

    [TestFixture, Apartment(ApartmentState.STA)]
    public class GrapherRibbonViewModelTestFixture
    {
        private Option option;
        private Mock<ISession> session;
        private Mock<IThingDialogNavigationService> thingNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IPluginSettingsService> pluginSettingService;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
        private Uri uri = new Uri("http://test.org");

        private ElementDefinition topElement;
        private ElementDefinition elementDefinition1;
        private ElementDefinition elementDefinition2;
        private ElementDefinition elementDefinition3;

        private ElementUsage elementUsage1;
        private ElementUsage elementUsage2;
        private ElementUsage elementUsage3;

        private DomainOfExpertise domain;
        private Person person;
        private Participant participant;
        private EngineeringModelSetup engineeringModelSetup;
        private EngineeringModel engineeringModel;
        private IterationSetup iterationSetup;
        private Iteration iteration;

        [SetUp]
        public void Setup()
        {
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri);

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri)
            {
                DefaultDomain = this.domain
            };

            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri);

            this.SetupElements();

            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri)
            {
                Participant = { new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person } }
            };

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri)
            {
                EngineeringModelSetup = this.engineeringModelSetup
            };

            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri)
            {
                IterationNumber = int.MaxValue
            };

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri)
            {
                IterationSetup = this.iterationSetup,
                TopElement = this.topElement
            };

            this.option = new Option(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "TestOption"
            };

            this.iteration.Option.Add(this.option);
            this.engineeringModel.Iteration.Add(this.iteration);
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.QueryCurrentDomainOfExpertise()).Returns(this.domain);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>() { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });
            this.thingNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.pluginSettingService = new Mock<IPluginSettingsService>();
        }

        private void SetupElements()
        {
            this.topElement = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };

            this.elementDefinition1 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.elementDefinition2 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };
            this.elementDefinition3 = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain };

            this.elementUsage1 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain, ElementDefinition = this.elementDefinition1 };
            this.elementUsage2 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain, ElementDefinition = this.elementDefinition2 };
            this.elementUsage3 = new ElementUsage(Guid.NewGuid(), this.cache, this.uri) { Owner = this.domain, ElementDefinition = this.elementDefinition3 };
        }
        
        [Test]
        public void VerifyStaticConstructor()
        {
            Assert.IsNotNull(GrapherRibbonViewModel.InstantiatePanelViewModel(this.option, this.session.Object, this.thingNavigationService.Object, this.panelNavigationService.Object, this.dialogNavigationService.Object, this.pluginSettingService.Object));
        }

        [Test]
        public void VerifyConstructor()
        {
            var vm = new GrapherRibbonViewModel();
            Assert.IsNotNull(vm);
        }
    }
}
