// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorCompletionDataTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

    using NUnit.Framework;

    /// <summary>
    /// Suite of test for the <see cref="EditorCompletionData"/> class.
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
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