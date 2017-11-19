// -------------------------------------------------------------------------------------------------
// <copyright file="DialogCloserTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm.Behaviours
{
    using System.Windows;

    using CDP4Composition.Navigation;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="DialogCloser"/> class.
    /// </summary>
    [TestFixture]
    [RequiresSTA]
    public class DialogCloserTestFixture
    {
        private Window window;

        [SetUp]
        public void SetUp()
        {
            this.window = new Window();
        }

        [Test]
        public void VerifyThatWhenResultIsChangedEventIsHandled()
        {
            DialogCloser.SetDialogResult(this.window, null);
            Assert.IsNull(this.window.DialogResult); 
        }

        [Test]
        [ExpectedException]
        public void VerifyThatEventHandlerIsCalledWhenDialogResultIsSetToTrue()
        {
            // Setting the dialog result to anything but null throws an exception. 
            // What we are testing here is that the dialog result is set on the window
            // The fact that this happens demonstrates the event is handled

            DialogCloser.SetDialogResult(this.window, true);
        }
    }
}
