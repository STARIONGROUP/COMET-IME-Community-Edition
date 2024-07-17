// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequirementsGroupDialogViewModelTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.Dialogs
{
    using System;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using CDP4Requirements.ViewModels;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class RequirementsGroupDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Mock<IPermissionService> permissionService;
        private ThingTransaction thingTransaction;
        private SiteDirectory siteDir;

        private EngineeringModel engineeringModel;
        private Iteration iteration;
        private RequirementsSpecification requirementsSpecification;
        private EngineeringModelSetup engineeringModelSetup;
        private ModelReferenceDataLibrary mrdl1;
        private IterationSetup iterationSetup;
        private DomainOfExpertise domainOfExpertise;

        private RequirementsSpecification reqSpec;
        private RequirementsGroup reqGroup;
        private DomainOfExpertise domain;
        private EngineeringModelSetup modelsetup;
        private ModelReferenceDataLibrary mrdl2;

        private Uri uri;
        private CDPMessageBus messageBus;

        [SetUp]
        public void Setup()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();

            this.uri = new Uri("http://test.com");
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.domain = new DomainOfExpertise(Guid.NewGuid(), null, this.uri);
            this.siteDir.Domain.Add(this.domain);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);
            this.modelsetup.ActiveDomain.Add(this.domain);
            this.mrdl1 = new ModelReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            this.modelsetup.RequiredRdl.Add(this.mrdl1);

            this.reqSpec = new RequirementsSpecification(Guid.NewGuid(), null, this.uri);
            this.reqGroup = new RequirementsGroup(Guid.NewGuid(), null, this.uri);

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), null, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.requirementsSpecification = new RequirementsSpecification(Guid.NewGuid(), null, this.uri);
            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), null, this.uri);

            this.mrdl2 = new ModelReferenceDataLibrary(Guid.NewGuid(), null, this.uri);
            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl2);

            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.iteration.IterationSetup = this.iterationSetup;

            var person = new Person(Guid.NewGuid(), null, this.uri);
            this.domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "test" };
            person.DefaultDomain = this.domainOfExpertise;

            this.engineeringModelSetup.ActiveDomain.Add(this.domainOfExpertise);
            this.engineeringModelSetup.IterationSetup.Add(this.iterationSetup);

            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(person);
            this.siteDir.Domain.Add(this.domainOfExpertise);

            this.engineeringModel.Iteration.Add(this.iteration);
            this.iteration.RequirementsSpecification.Add(this.requirementsSpecification);
            this.iteration.RequirementsSpecification.Add(this.reqSpec);
            this.requirementsSpecification.Group.Add(this.reqGroup);

            this.engineeringModel.EngineeringModelSetup = this.engineeringModelSetup;

            var transactionContext = TransactionContextResolver.ResolveContext(this.engineeringModel);
            this.thingTransaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPopulatePossibleOwnerWorks()
        {
            var clone = this.reqSpec.Clone(true);
            this.thingTransaction.CreateOrUpdate(clone);

            var vm = new RequirementsGroupDialogViewModel(this.reqGroup, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, null, clone);

            Assert.AreEqual(1, vm.PossibleOwner.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new RequirementsGroupDialogViewModel());
        }
    }
}
