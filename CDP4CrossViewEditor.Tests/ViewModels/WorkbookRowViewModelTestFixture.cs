// -------------------------------------------------------------------------------------------------
// <copyright file="WorkbookSelectionViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A. All rights reserved
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.Tests.ViewModels
{
    using CDP4CrossViewEditor.ViewModels;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="WorkbookRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class WorkbookRowViewModelTestFixtureTestFixture
    {
        private WorkbookRowViewModel viewModel;

        [SetUp]
        public void SetUp()
        {
            this.viewModel = null;
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void CheckIfModelIsInitialized()
        {
            this.viewModel = new WorkbookRowViewModel(null);

            Assert.IsNull(this.viewModel.Workbook);
            Assert.IsNull(this.viewModel.Name);
            Assert.IsNull(this.viewModel.Path);
        }
    }
}
