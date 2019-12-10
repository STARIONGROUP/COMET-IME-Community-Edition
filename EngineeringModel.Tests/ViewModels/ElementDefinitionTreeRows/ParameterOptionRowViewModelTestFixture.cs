// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterOptionRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.ElementDefinitionTreeRows
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using CDP4EngineeringModel.ViewModels;
    
    using Moq;
    using NUnit.Framework;
    using System.Collections.Generic;
    using CDP4Dal.Events;

    /// <summary>
    /// Suite of tests for the <see cref="ParameterOptionRowViewModelTestFixture"/>
    /// </summary>
    [TestFixture]
    public class ParameterOptionRowViewModelTestFixture
    {
        private Mock<IThingDialogNavigationService> thingDialognavigationService;        
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<IPermissionService> permissionService;
        private Mock<ISession> session;

        private Participant participant;
        private Person person;
        private DomainOfExpertise activeDomain;
        private DomainOfExpertise otherDomain;

        private SiteDirectory siteDirectory;
        private EngineeringModelSetup engineeringModelSetup;
        private EngineeringModel engineeringModel;
        private Iteration iteration;
        private Option option;
        private ElementDefinition elementDefinition;
        private ElementDefinition otherElementDefinition;
        private ElementUsage elementUsage;

        [SetUp]
        public void SetUp()
        {
            this.thingDialognavigationService = new Mock<IThingDialogNavigationService>();
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();
            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.activeDomain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "active", ShortName = "active" };
            this.otherDomain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "other", ShortName = "other" };

            this.person = new Person(Guid.NewGuid(), null, this.uri) { GivenName = "test", Surname = "test" };
            this.participant = new Participant(Guid.NewGuid(), null, this.uri) { Person = this.person, SelectedDomain = this.activeDomain };
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>());

            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.engineeringModelSetup.Participant.Add(this.participant);
            
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.engineeringModel.EngineeringModelSetup = this.engineeringModelSetup;
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            this.otherElementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            this.elementUsage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri);
            this.elementUsage.ElementDefinition = otherElementDefinition;
            this.elementDefinition.ContainedElement.Add(this.elementUsage);

            this.engineeringModel.Iteration.Add(this.iteration);
            this.iteration.Option.Add(this.option);
            this.iteration.Element.Add(this.elementDefinition);
            this.iteration.Element.Add(this.otherElementDefinition);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatThingStatusIsNotNull()
        {
            var parameter = new Parameter(Guid.NewGuid(), this.cache, this.uri);
            var textParameterType = new TextParameterType(Guid.NewGuid(), this.cache, this.uri);

            parameter.ParameterType = textParameterType;
            parameter.IsOptionDependent = true;

            var row = new ParameterOptionRowViewModel(parameter, this.option, this.session.Object, null, false);

            Assert.IsNotNull(row.ThingStatus);
        }
    }
}