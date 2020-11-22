// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicationBrowserViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4EngineeringModel.Tests.ViewModels
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4EngineeringModel.ViewModels;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="PublicationBrowserViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class PublicationBrowserViewModelTestFixture
    {
        private Mock<ISession> session;
        private Assembler assembler;
        private Mock<IPermissionService> permissionService;
        private Mock<IThingDialogNavigationService> thingDialogNavigationService;
        private Mock<IPanelNavigationService> panelNavigationService;
        private readonly Uri uri = new Uri("http://test.com");

        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationsetup;
        private Person person;
        private Participant participant;
        private EngineeringModel model;
        private Iteration iteration;
        private ElementDefinition elementDefinition;
        private Publication publication;
        private Parameter parameter1;
        private Parameter parameter2;
        private Parameter parameter3;
        private ParameterOverride parameterOverride1;
        private DomainOfExpertise domain;
        private DomainOfExpertise otherDomain;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.permissionService = new Mock<IPermissionService>();
            this.thingDialogNavigationService = new Mock<IThingDialogNavigationService>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.cache = this.assembler.Cache;
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.cache, this.uri) { Name = "model" };
            this.iterationsetup = new IterationSetup(Guid.NewGuid(), this.cache, this.uri);
            this.person = new Person(Guid.NewGuid(), this.cache, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain", ShortName = "DMN" };
            this.otherDomain = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "otherDomain", ShortName = "ODMN" };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri) { Person = this.person, SelectedDomain = this.domain };

            this.modelsetup.ActiveDomain = new List<DomainOfExpertise>() { this.domain, this.otherDomain };

            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri);

            this.sitedir.Model.Add(this.modelsetup);
            this.sitedir.Person.Add(this.person);
            this.sitedir.Domain.Add(this.domain);
            this.sitedir.Domain.Add(this.otherDomain);
            this.modelsetup.IterationSetup.Add(this.iterationsetup);
            this.modelsetup.Participant.Add(this.participant);

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri) { IterationSetup = this.iterationsetup };
            this.iteration.Element.Add(this.elementDefinition);
            this.publication = new Publication(Guid.NewGuid(), this.cache, this.uri);

            this.parameter1 = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "paramtype1", ShortName = "pat1" }, Owner = this.domain };
            this.parameter2 = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "paramtype2", ShortName = "pat2" }, Owner = this.domain };
            this.parameter3= new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "paramtypeadd", ShortName = "padd" }, Owner = this.domain };

            var parameterforoverride = new Parameter(Guid.NewGuid(), this.cache, this.uri) { ParameterType = new SimpleQuantityKind(Guid.NewGuid(), this.cache, this.uri) { Name = "paramtype3", ShortName = "pat3" } };

            // Test input
            var valueSet = new ParameterValueSet(Guid.NewGuid(), this.cache, this.uri);

            this.SetScalarValueSet(valueSet);
            parameterforoverride.ValueSet.Add(valueSet);

            this.parameter1.ValueSet.Add(valueSet.Clone(false));
            this.parameter3.ValueSet.Add(valueSet.Clone(false));

            this.parameterOverride1 = new ParameterOverride(Guid.NewGuid(), this.cache, this.uri) { Parameter = parameterforoverride };

            this.elementDefinition.Parameter.Add(this.parameter1);
            this.elementDefinition.Parameter.Add(this.parameter2);
            this.elementDefinition.Parameter.Add(this.parameter3);
            this.elementDefinition.Parameter.Add(parameterforoverride);

            var elementusage = new ElementUsage(Guid.NewGuid(), this.cache, this.uri);

            this.elementDefinition.ContainedElement.Add(elementusage);
            elementusage.ParameterOverride.Add(this.parameterOverride1);

            this.publication.PublishedParameter.Add(this.parameter1);
            this.publication.PublishedParameter.Add(this.parameter2);
            this.publication.PublishedParameter.Add(this.parameter3);
            this.publication.PublishedParameter.Add(this.parameterOverride1);

            this.publication.CreatedOn = DateTime.Now;
            this.publication.Domain.Add(this.domain);

            this.model.Iteration.Add(this.iteration);
            this.iteration.Publication.Add(this.publication);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.sitedir);
            this.session.Setup(x => x.ActivePerson).Returns(this.person);
            this.session.Setup(x => x.Write(It.IsAny<OperationContainer>())).Returns(Task.FromResult("some result"));
            

            this.cache.TryAdd(new CacheKey(this.iteration.Iid, null), new Lazy<Thing>(() => this.iteration));
        }

        /// <summary>
        /// Set a ValueSet for a scalar ParameterType
        /// </summary>
        /// <param name="valueset">The <see cref="ParameterValueSetBase"/> to set</param>
        private void SetScalarValueSet(ParameterValueSetBase valueset)
        {
            var manualSet = new ValueArray<string>(new List<string> { "-" });
            var referenceSet = new ValueArray<string>(new List<string> { "-" });
            var computedSet = new ValueArray<string>(new List<string> { "-" });
            var publishedSet = new ValueArray<string>(new List<string> { "-" });
            var formulaSet = new ValueArray<string>(new List<string> { "-" });

            valueset.Manual = manualSet;
            valueset.Reference = referenceSet;
            valueset.Computed = computedSet;
            valueset.Published = publishedSet;
            valueset.Formula = formulaSet;
            valueset.ValueSwitch = ParameterSwitchKind.REFERENCE;
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatRowsAreCreated()
        {
            var viewmodel = new PublicationBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.AreEqual(1, viewmodel.Publications.Count);
            Assert.AreEqual(4, viewmodel.Publications[0].ContainedRows.Count);
            Assert.That(viewmodel.Caption, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.ToolTip, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.DataSource, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.DomainOfExpertise, Is.Not.Null.Or.Empty);
            Assert.That(viewmodel.CurrentModel, Is.Not.Null.Or.Empty);

            var publicationRowViewModel = viewmodel.Publications.Single();
            Assert.That(publicationRowViewModel.OwnerShortName, Is.Not.Null.Or.Empty);
            Assert.That(publicationRowViewModel.Name, Is.Not.Null.Or.Empty);
        }

        [Test]
        public void VerifyThatPublicationRowsAreUpdated()
        {
            var viewmodel = new PublicationBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            var newpublication = new Publication(Guid.NewGuid(), null, this.uri);
            this.iteration.Publication.Add(newpublication);

            var revision = typeof(Iteration).GetProperty("RevisionNumber");
            revision.SetValue(this.iteration, 2);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(2, viewmodel.Publications.Count);

            this.iteration.Publication.Clear();
            revision.SetValue(this.iteration, 3);

            CDPMessageBus.Current.SendObjectChangeEvent(this.iteration, EventKind.Updated);
            Assert.AreEqual(0, viewmodel.Publications.Count);
        }

        [Test]
        public void VerifyThatDomainsAreAdded()
        {
            var viewmodel = new PublicationBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            Assert.AreEqual(2, viewmodel.Domains.Count);

            var domain2 = new DomainOfExpertise(Guid.NewGuid(), this.cache, this.uri) { Name = "domain2", ShortName = "DM2" };
            this.sitedir.Domain.Add(domain2);
            this.publication.Domain.Add(domain2);

            Assert.AreEqual(2, viewmodel.Domains.Count);

            this.modelsetup.ActiveDomain.Add(domain2);
            CDPMessageBus.Current.SendObjectChangeEvent(this.modelsetup, EventKind.Updated);
            Assert.AreEqual(3, viewmodel.Domains.Count);

        }

        [Test]
        public void VerifyThatParameterRowsAreAddedAndRemoved()
        {
            var viewmodel = new PublicationBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            this.parameter3.ValueSet.First().Published = new ValueArray<string>(new List<string>() { "-" },
                this.parameter3.ValueSet.First());
            this.parameter3.ValueSet.First().ValueSwitch = ParameterSwitchKind.REFERENCE;
            this.parameter3.ValueSet.First().Reference = new ValueArray<string>(new List<string>() { "20" },
                this.parameter3.ValueSet.First());

            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter3, EventKind.Added);

            Assert.AreEqual(2, viewmodel.Domains.Count);
            Assert.AreEqual(1, viewmodel.Domains[0].ContainedRows.Count);

            this.parameter1.ValueSet.First().Published = new ValueArray<string>(new List<string>() { "-" },
                this.parameter1.ValueSet.First());
            this.parameter1.ValueSet.First().ValueSwitch = ParameterSwitchKind.MANUAL;
            this.parameter1.ValueSet.First().Manual = new ValueArray<string>(new List<string>() { "-" },
                this.parameter1.ValueSet.First());

            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1.ValueSet[0], EventKind.Updated);

            Assert.AreEqual(2, viewmodel.Domains.Count);
            Assert.AreEqual(1, viewmodel.Domains[0].ContainedRows.Count);

            this.parameter1.ValueSet.First().Manual = new ValueArray<string>(new List<string> { "134" });

            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1.ValueSet.First(), EventKind.Updated);
            Assert.AreEqual(2, viewmodel.Domains[0].ContainedRows.Count);

            // verify that same row is updated
            this.parameter1.ValueSet.First().Manual = new ValueArray<string>(new List<string>() { "213" });

            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1.ValueSet[0], EventKind.Updated);
            Assert.AreEqual(2, viewmodel.Domains[0].ContainedRows.Count);

            // verify that ownership of the parameter is changed
            this.parameter1.Owner = otherDomain;
            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1, EventKind.Updated);
            Assert.AreEqual(1, viewmodel.Domains[0].ContainedRows.Count);
            Assert.AreEqual(1, viewmodel.Domains[1].ContainedRows.Count);

            // verify that value is removed
            this.parameter1.ValueSet.First().Manual = new ValueArray<string>(new List<string> { "-" });

            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1.ValueSet[0], EventKind.Updated);
            Assert.AreEqual(0, viewmodel.Domains[1].ContainedRows.Count);

            // verify that parameter is removed
            this.parameter1.ValueSet.First().Manual = new ValueArray<string>(new List<string> { "268" });
            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1.ValueSet[0], EventKind.Updated);
            Assert.AreEqual(1, viewmodel.Domains[1].ContainedRows.Count);

            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1, EventKind.Removed);
            Assert.AreEqual(0, viewmodel.Domains[1].ContainedRows.Count);
        }

        [Test]
        public async Task VerifyThatCanPublish()
        {
            var viewmodel = new PublicationBrowserViewModel(this.iteration, this.session.Object, this.thingDialogNavigationService.Object, this.panelNavigationService.Object, null, null);

            this.parameter1.ValueSet.First().Published = new ValueArray<string>(new List<string>() { "-" },
                this.parameter1.ValueSet.First());
            this.parameter1.ValueSet.First().ValueSwitch = ParameterSwitchKind.MANUAL;

            // verify that same row is updated
            this.parameter1.ValueSet.First().Manual = new ValueArray<string>(new List<string>() { "213" },
                this.parameter1.ValueSet.First());

            CDPMessageBus.Current.SendObjectChangeEvent(this.parameter1.ValueSet[0], EventKind.Updated);
            Assert.AreEqual(1, viewmodel.Domains[0].ContainedRows.Count);

            viewmodel.Domains[0].ToBePublished = true;
            ((PublicationParameterOrOverrideRowViewModel)viewmodel.Domains[0].ContainedRows[0]).ToBePublished = true;

            Assert.IsTrue(viewmodel.Domains.Any(x => x.ToBePublished));

            Assert.DoesNotThrowAsync(async () => viewmodel.ExecutePublishCommand());

            Assert.IsFalse(viewmodel.Domains.Any(x => x.ToBePublished));
        }

        [Test]
        public void VerifyThatActiveDomainIsDisplayed()
        {
            var testDomain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "domain" };
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(testDomain);

            var vm = new PublicationBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("domain []", vm.DomainOfExpertise);

            testDomain = null;
            this.session.Setup(x => x.QuerySelectedDomainOfExpertise(this.iteration)).Returns(testDomain);

            vm = new PublicationBrowserViewModel(this.iteration, this.session.Object, null, null, null, null);
            Assert.AreEqual("None", vm.DomainOfExpertise);
        }
    }
}