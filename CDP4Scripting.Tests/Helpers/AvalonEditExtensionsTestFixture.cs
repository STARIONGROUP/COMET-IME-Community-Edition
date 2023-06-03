// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvalonEditExtensionsTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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