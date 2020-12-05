// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StateToParameterTypeMapperBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4ReferenceDataMapper.Tests.ViewModels.StateToParameterTypeMapper
{
    using System;
    using System.Collections.Concurrent;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using CDP4ReferenceDataMapper.ViewModels;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="StateToParameterTypeMapperBrowserViewModel"/> class
    /// </summary>
    [TestFixture]
    public class StateToParameterTypeMapperBrowserViewModelTestFixture
    {
        private Mock<IFluentRibbonManager> ribbonManager;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IDialogNavigationService> dialogNavigationService;
        private Mock<IPluginSettingsService> pluginSettingsService;

        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        private StateToParameterTypeMapperBrowserViewModel stateToParameterTypeMapperBrowserViewModel;

        private SiteDirectory siteDirectory;
        private Person person;
        private Participant participant;
        private DomainOfExpertise domain;
        private EngineeringModelSetup engineeringModelSetup;
        private IterationSetup iterationSetup;

        private EngineeringModel engineeringModel;

        private Iteration iteration;

        [SetUp]
        public void SetUp()
        {
            this.ribbonManager = new Mock<IFluentRibbonManager>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.dialogNavigationService = new Mock<IDialogNavigationService>();
            this.pluginSettingsService = new Mock<IPluginSettingsService>();

            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { ShortName = "TST", Name = "Test" };
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri) { IterationNumber = 1, Description = "iteraiton 1" };
            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);
            this.siteDirectory.Model.Add(this.engineeringModelSetup);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { GivenName = "John", Surname = "Doe" };
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };
            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.engineeringModelSetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationSetup };
            this.engineeringModel.Iteration.Add(this.iteration);

            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDirectory);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
        }

        [Test]
        public void Verify_that_properties_are_set_on_newed_up()
        {
            this.stateToParameterTypeMapperBrowserViewModel = new StateToParameterTypeMapperBrowserViewModel(
                this.iteration,
                this.session.Object,
                this.thingDialogNavigationService.Object,
                this.panelNavigationService.Object,
                this.dialogNavigationService.Object,
                this.pluginSettingsService.Object);

            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.Caption, Is.EqualTo("Actual Finite State to ParameterType mapping, iteration_1"));
            Assert.That(this.stateToParameterTypeMapperBrowserViewModel.ToolTip, Is.EqualTo("Test\nhttp://www.rheagroup.com/\nJohn Doe"));
        }
    }
}
