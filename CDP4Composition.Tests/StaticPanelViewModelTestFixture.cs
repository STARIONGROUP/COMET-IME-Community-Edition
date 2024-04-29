// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StaticPanelViewModelTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests
{
    using NUnit.Framework;

    /// <summary>
    /// TestFixture for the <see cref="StaticPanelViewModel"/>
    /// </summary>
    [TestFixture]
    public class StaticPanelViewModelTestFixture
    {
        [Test]
        public void VerifyThatTheCaptionIsCorrect()
        {
            var targetName = "a target";

            var viewmodel = new ConcretePanelViewModel(targetName);
            Assert.AreEqual(targetName, viewmodel.TargetName);

            var newTargetName = "a new target";
            viewmodel.TargetName = newTargetName;

            Assert.AreEqual(newTargetName, viewmodel.TargetName);

        }

        /// <summary>
        /// private class to test the <see cref="StaticPanelViewModel"/>
        /// </summary>
        private class ConcretePanelViewModel : StaticPanelViewModel
        {
            public ConcretePanelViewModel(string targetName)
                : base(targetName)
            {
            }
        }
    }
}
