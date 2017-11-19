// -------------------------------------------------------------------------------------------------
// <copyright file="GenericConfirmationDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.ViewModels
{
    using CDP4Composition.ViewModels;
    using NUnit.Framework;
    
    [TestFixture]
    public class GenericConfirmationDialogViewModelTestFixture
    {
        private GenericConfirmationDialogViewModel viewModel;

        [Test]
        public void VerityThatPropertiesAreSetByConstructor()
        {
            this.viewModel = new GenericConfirmationDialogViewModel("the title", "the message");

            Assert.AreEqual("the title", viewModel.Title);
            Assert.AreEqual("the message", viewModel.Message);
        }

        [Test]
        public void VerifyThatYesCommandWorksAsExpected()
        {
            this.viewModel = new GenericConfirmationDialogViewModel("", "");
            this.viewModel.YesCommand.CanExecute(null);

            this.viewModel.YesCommand.Execute(null);

            Assert.IsTrue((bool)this.viewModel.DialogResult.Result.Value);
        }

        [Test]
        public void VerifyThatNOCommandWorksAsExpected()
        {
            this.viewModel = new GenericConfirmationDialogViewModel("", "");
            this.viewModel.NoCommand.CanExecute(null);

            this.viewModel.NoCommand.Execute(null);

            Assert.IsFalse((bool)this.viewModel.DialogResult.Result.Value);
        }
    }
}
