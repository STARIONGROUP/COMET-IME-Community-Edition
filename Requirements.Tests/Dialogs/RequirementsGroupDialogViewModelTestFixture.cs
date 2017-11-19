// -------------------------------------------------------------------------------------------------
// <copyright file="RequirementsGroupDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Dialogs
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.Operations;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.DAL;
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
        private IterationSetup iterationSetup;

        private RequirementsGroup reqGroup;
        private Uri uri;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.permissionService = new Mock<IPermissionService>();
            
            this.uri = new Uri("http://test.com");
            this.siteDir = new SiteDirectory(Guid.NewGuid(), null, this.uri);
            this.reqGroup = new RequirementsGroup(Guid.NewGuid(), null, this.uri);

            this.engineeringModel = new EngineeringModel(Guid.NewGuid(), null, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), null, this.uri);
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), null, this.uri);
            this.iteration.IterationSetup = this.iterationSetup;

            this.engineeringModel.Iteration.Add(this.iteration);

            var transactionContext = TransactionContextResolver.ResolveContext(this.engineeringModel);
            this.thingTransaction = new ThingTransaction(transactionContext, null);

            var dal = new Mock<IDal>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(dal.Object);
            dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }


        [Test]
        public void VerifyThatPopulatePossibleOwnerWorks()
        {
            this.session.Setup(x => x.RetrieveSiteDirectory()).Returns(this.siteDir);
            this.session.Setup(x => x.ActivePerson).Returns(new Person(Guid.NewGuid(), null, this.uri));
            this.siteDir.Domain.Add(new DomainOfExpertise(Guid.NewGuid(), null, this.uri) { Name = "test" });

            var vm = new RequirementsGroupDialogViewModel(this.reqGroup, this.thingTransaction, this.session.Object,
                true, ThingDialogKind.Create, null);

            Assert.AreEqual(1, vm.PossibleOwner.Count);
        }

        [Test]
        public void VerifyThatParameterlessContructorExists()
        {
            Assert.DoesNotThrow(() => new RequirementsGroupDialogViewModel());
        }
    }
}