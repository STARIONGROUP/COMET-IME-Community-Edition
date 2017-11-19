// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionMenuGroupViewModelTestFxiture.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm.MenuItems
{
    using System;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="SessionMenuGroupViewModel"/> class.
    /// </summary>
    [TestFixture]
    public class SessionMenuGroupViewModelTestFxiture
    {
        private string dataSourceUri;
        private Mock<ISession> session;
        private SiteDirectory siteDirectory;
        private SessionMenuGroupViewModel sessionMenuGroupViewModel;


        [SetUp]
        public void SetUp()
        {
            this.dataSourceUri = "http://www.rheagroup.com";
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.DataSourceUri).Returns(this.dataSourceUri);            
            this.session.Setup(x => x.Name).Returns(this.dataSourceUri + " John Doe");

            this.siteDirectory = new SiteDirectory();

            this.sessionMenuGroupViewModel = new SessionMenuGroupViewModel(this.siteDirectory, this.session.Object);
        }

        [Test]
        public void VerifyThatDeriveNameReturnsExpectedResult()
        {
            Assert.AreEqual(this.dataSourceUri + " John Doe", this.sessionMenuGroupViewModel.Name);
        }

        [Test]
        public void VerifyThatEngineeringModelsIsEmpty()
        {
            CollectionAssert.IsEmpty(this.sessionMenuGroupViewModel.EngineeringModels);
        }
    }
}
