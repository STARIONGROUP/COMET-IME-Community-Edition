// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace CDP4Requirements.Tests.RequirementBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4Requirements.ViewModels;
    
    using CDP4RequirementsVerification;
    using Microsoft.Practices.ServiceLocation;
    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="RequirementsBrowserViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class RequirementBrowserViewModelTestFixture
    {
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private readonly Uri uri = new Uri("http://www.rheagroup.com");
        private EngineeringModel model;
        private EngineeringModelSetup modelSetup;
        private Iteration iteration;
        private ElementDefinition elementDefinition;
        private IterationSetup iterationSetup;
        private RequirementsSpecification reqSpec;
        private RequirementsGroup reqGroup;
        private DomainOfExpertise domain;
        private Assembler assembler;
        private Mock<ISession> session;
        private Participant participant;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> dialogNavigation;
        private Mock<IPanelNavigationService> panelNavigation;
        private Mock<IPermissionService> permissionService;
        private Mock<IMessageBoxService> messageBoxService;
        private Person person;
        private Option option;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.cache = this.assembler.Cache;

            this.permissionService = new Mock<IPermissionService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<ClassKind>(), It.IsAny<Thing>())).Returns(true);

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.messageBoxService = new Mock<IMessageBoxService>();
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            this.session = new Mock<ISession>();

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri) { ShortName = "test" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { SelectedDomain = null, Person = this.person };
            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.modelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);
            this.option = new Option(Guid.NewGuid(), this.cache, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.reqGroup = new RequirementsGroup(Guid.NewGuid(), this.cache, this.uri);

            this.modelSetup.IterationSetup.Add(this.iterationSetup);
            this.modelSetup.Participant.Add(this.participant);

            this.panelNavigation = new Mock<IPanelNavigationService>();
            this.dialogNavigation = new Mock<IThingDialogNavigationService>();

            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "test", ShortName = "test"};
            this.reqSpec.Owner = this.domain;
            this.participant.Domain.Add(this.domain);
            
            this.iteration.RequirementsSpecification.Add(this.reqSpec);
            this.iteration.Option.Add(this.option);
            this.iteration.DefaultOption = this.option;
            this.iteration.IterationSetup = this.iterationSetup;
            this.iteration.Element.Add(this.elementDefinition);
            this.iteration.TopElement = this.elementDefinition;

            this.model.EngineeringModelSetup = this.modelSetup;
            this.model.Iteration.Add(this.iteration);
            this.reqSpec.Group.Add(this.reqGroup);

            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>> { { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, this.participant) } });
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(this.domain);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRequirementSpecificationMayBeAddedOrRemoved()
        {
            var revision = typeof(Thing).GetProperty("RevisionNumber");
            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, this.dialogNavigation.Object, this.panelNavigation.Object, null, null);

            Assert.AreEqual(1, vm.ReqSpecificationRows.Count);

            var reqspec2 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri);
            this.iteration.RequirementsSpecification.Add(reqspec2);
            revision.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(2, vm.ReqSpecificationRows.Count);

            this.iteration.RequirementsSpecification.Remove(reqspec2);
            revision.SetValue(this.iteration, 3);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(1, vm.ReqSpecificationRows.Count);
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual(1, vm.ReqSpecificationRows.Count);
            Assert.AreEqual(this.participant, vm.ActiveParticipant);
            Assert.AreEqual("Requirements, iteration_0", vm.Caption);
            Assert.AreEqual("model", vm.CurrentModel);
            Assert.AreEqual("test [test]", vm.DomainOfExpertise);
            Assert.AreEqual("model\nhttp://www.rheagroup.com/\n ", vm.ToolTip);
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("test [test]", vm.DomainOfExpertise);

            DomainOfExpertise domainOfExpertise = null;
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(domainOfExpertise);

            vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }

        [Test]
        public void VerifyThatCreateRequirementWorks()
        {
            this.session.Setup(x => x.OpenIterations).Returns(new Dictionary<Iteration, Tuple<DomainOfExpertise, Participant>>
            {
                { this.iteration, new Tuple<DomainOfExpertise, Participant>(this.domain, null) }
            });

            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, this.dialogNavigation.Object, this.panelNavigation.Object, null, null);
            var reqSpecRow = vm.ReqSpecificationRows.Single();

            vm.SelectedThing = reqSpecRow;
            Assert.IsTrue(vm.CanCreateRequirement);
            vm.CreateRequirementCommand.Execute(null);

            this.dialogNavigation.Verify(x => x.Navigate(It.IsAny<Requirement>(), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.dialogNavigation.Object, It.IsAny<RequirementsSpecification>(), null));

            vm.SelectedThing = (RequirementsGroupRowViewModel) reqSpecRow.ContainedRows.Single(x => x.Thing is RequirementsGroup);
            Assert.IsTrue(vm.CanCreateRequirement);
            vm.CreateRequirementCommand.Execute(null);

            this.dialogNavigation.Verify(x => x.Navigate(It.Is<Requirement>(r => r.Group != null), It.IsAny<IThingTransaction>(), this.session.Object, true, ThingDialogKind.Create, this.dialogNavigation.Object, It.IsAny<RequirementsSpecification>(), null));
        }

        [Test]
        public void VerifyThatRequirementVerificationWorks()
        {
            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, this.dialogNavigation.Object, this.panelNavigation.Object, null, null);
            var reqSpecRow = vm.ReqSpecificationRows.Single();

            Assert.IsTrue(vm.CanVerifyRequirements);
            vm.ExecuteVerifyRequirements(this.iteration.DefaultOption);
            Assert.AreEqual(RequirementStateOfCompliance.Inconclusive, reqSpecRow.RequirementStateOfCompliance);
        }

        [Test]
        public void VerifyThatDeprecatedRequirementSpecificationsAreNotVerified()
        {
            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, this.dialogNavigation.Object, this.panelNavigation.Object, null, null);
            var reqSpecRow = vm.ReqSpecificationRows.Single();

            Assert.IsTrue(vm.CanVerifyRequirements);
            reqSpecRow.RequirementStateOfCompliance = RequirementStateOfCompliance.Unknown;
            reqSpecRow.Thing.IsDeprecated = true;

            vm.ExecuteVerifyRequirements(this.iteration.DefaultOption);
            Assert.AreEqual(RequirementStateOfCompliance.Unknown, reqSpecRow.RequirementStateOfCompliance);
        }

        [Test]
        public void VerifyThatNoTopElementSetResultsInMessageBox()
        {
            this.iteration.TopElement = null;

            var vm = new RequirementsBrowserViewModel(this.iteration, this.session.Object, this.dialogNavigation.Object, this.panelNavigation.Object, null, null);
            var reqSpecRow = vm.ReqSpecificationRows.Single();

            Assert.IsTrue(vm.CanVerifyRequirements);
            vm.ExecuteVerifyRequirements(this.iteration.DefaultOption);
            
            Assert.AreEqual(RequirementStateOfCompliance.Unknown, reqSpecRow.RequirementStateOfCompliance);
            
            messageBoxService.Verify(
                x => x.Show(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(),
                    MessageBoxImage.Error
                    )
                );
        }
    }
}
