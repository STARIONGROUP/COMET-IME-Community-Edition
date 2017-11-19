// -------------------------------------------------------------------------------------------------
// <copyright file="CopyConfirmationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


namespace CDP4CommonView.Tests.ViewModels
{
    using CDP4CommonView.ViewModels;
    using NUnit.Framework;
    
    /// <summary>
    /// Suite of tests for the <see cref="OkDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    public class OkDialogViewModelTestFixture
    {
        private OkDialogViewModel viewModel;

        [Test]
        public void VerityThatPropertiesAreSetByConstructor()
        {
            this.viewModel = new OkDialogViewModel("the title", "the message");

            Assert.AreEqual("the title", viewModel.Title);
            Assert.AreEqual("the message", viewModel.Message);
        }

        [Test]
        public void VerifyThatYesCommandWorksAsExpected()
        {
            this.viewModel = new OkDialogViewModel("", "");
            this.viewModel.OkCommand.CanExecute(null);

            this.viewModel.OkCommand.Execute(null);

            Assert.IsTrue((bool)this.viewModel.DialogResult.Result.Value);
        }
    }
}
