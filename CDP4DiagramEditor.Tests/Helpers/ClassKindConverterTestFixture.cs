// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassKindConverterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski.
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


using System;
using CDP4Common.CommonData;

namespace CDP4DiagramEditor.Tests.Helpers
{
    using CDP4DiagramEditor.Helpers;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ClassKindConverter"/> class.
    /// </summary>
    [TestFixture]
    public class ClassKindConverterTestFixture
    {
        private ClassKindConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new ClassKindConverter();
        }

        [Test]
        public void VerifyThatConverReturnsTheExpectedResult()
        {
            var result = this.converter.Convert(ClassKind.ElementDefinition, null, null, null);
            Assert.AreEqual("<<Element Definition>>", result);
        }

        [Test]
        public void VerifyThatConvertBackThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}
