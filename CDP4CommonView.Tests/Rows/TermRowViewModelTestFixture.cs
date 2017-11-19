// -------------------------------------------------------------------------------------------------
// <copyright file="TermRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Tests.Rows
{
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="TermRowViewModel"/> class
    /// </summary>
    public class TermRowViewModelTestFixture
    {
        private Term term;
        private TermRowViewModel termRowViewModel;
        private Mock<ISession> session;

        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();

            this.term = new Term();
            this.termRowViewModel = new TermRowViewModel(term, this.session.Object, null);
        }

        [Test]
        public void VerifyThatTheDefinitionValueCanBeSetAndGet()
        {
            this.termRowViewModel.DefinitionValue = "test";

            Assert.AreEqual("test", this.termRowViewModel.DefinitionValue);
        }
    }
}
