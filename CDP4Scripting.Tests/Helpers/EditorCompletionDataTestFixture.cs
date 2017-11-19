// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorCompletionDataTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Tests.Helpers
{
    using CDP4Scripting.Helpers;
    using NUnit.Framework;

    /// <summary>
    /// Suite of test for the <see cref="EditorCompletionData"/> class.
    /// </summary>
    [TestFixture, RequiresSTA]
    public class EditorCompletionDataTestFixture
    {
        [Test]
        public void VerifyThatInitializationWorks()
        {
            var editorCompletionData = new EditorCompletionData("name", "description");
            Assert.AreEqual("name", editorCompletionData.Text);
            Assert.AreEqual("description", editorCompletionData.Description);
        }
    }
}