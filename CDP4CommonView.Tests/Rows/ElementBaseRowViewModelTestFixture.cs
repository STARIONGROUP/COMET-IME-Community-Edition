// -------------------------------------------------------------------------------------------------
// <copyright file="ElementBaseRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4CommonView;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using Moq;    
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ElementBaseRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class ElementBaseRowViewModelTestFixture
    {
        private Mock<ISession> session;

        private ElementDefinitionRowViewModel rowViewModel;
            
        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();
            
            var elementDefinition = new ElementDefinition(Guid.NewGuid(), null, null);
            this.rowViewModel = new ElementDefinitionRowViewModel(
                elementDefinition,
                this.session.Object,
                null);
        }

        [TearDown]
        public void TearDown()
        {            
        }

        [Test]
        public void VerifyThatOwnerIsSet()
        {
            var owner = new DomainOfExpertise();

            this.rowViewModel.Owner = owner;

            Assert.AreSame(owner, this.rowViewModel.Owner);
        }
    }
}
