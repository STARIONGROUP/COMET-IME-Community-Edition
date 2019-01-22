// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvalonEditExtensionsTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Tests.Helpers
{
    using System.Threading;
    using CDP4Scripting.Helpers;
    using ICSharpCode.AvalonEdit;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="AvalonEditExtensions"/> class.
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class AvalonEditExtensionsTestFixture
    {
        private TextEditor textEditor;

        [SetUp]
        public void SetUp()
        {
            this.textEditor = new TextEditor();
        }

        [Test]
        public void VerifyThatGetWordsBeforeDotWorks()
        {
            this.textEditor.Text = "Command.";
            this.textEditor.CaretOffset = 8;
            var wordsBeforeDot = this.textEditor.GetWordsBeforeDot();
            Assert.AreEqual("Command", wordsBeforeDot);

            this.textEditor.Text = "object.property.";
            this.textEditor.CaretOffset = 16;
            wordsBeforeDot = this.textEditor.GetWordsBeforeDot();
            Assert.AreEqual("object.property", wordsBeforeDot);

            this.textEditor.Text = "object=property.";
            this.textEditor.CaretOffset = 16;
            wordsBeforeDot = this.textEditor.GetWordsBeforeDot();
            Assert.AreEqual("property", wordsBeforeDot);

            this.textEditor.Text = "command. property.";
            this.textEditor.CaretOffset = 18;
            wordsBeforeDot = this.textEditor.GetWordsBeforeDot();
            Assert.AreEqual("property", wordsBeforeDot);
        }
    }
}