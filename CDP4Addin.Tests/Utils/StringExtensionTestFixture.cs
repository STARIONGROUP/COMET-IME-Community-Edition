﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtensionTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4AddinCE.Tests.Utils
{
    using CDP4AddinCE.Utils;
    using CDP4Composition;
    using NetOffice.OfficeApi.Enums;

    using NUnit.Framework;

    /// <summary>
    /// suite of tests for the <see cref="StringExtension"/> class
    /// </summary>
    [TestFixture]
    public class StringExtensionTestFixture
    {
        [Test]
        public void VerifyThatToDockPositionReturnsExpectedResults()
        {
            var left = LayoutGroupNames.LeftGroup;
            Assert.AreEqual(MsoCTPDockPosition.msoCTPDockPositionLeft, StringExtension.ToDockPosition(left));

            var right = LayoutGroupNames.RightGroup;
            Assert.AreEqual(MsoCTPDockPosition.msoCTPDockPositionRight, StringExtension.ToDockPosition(right));

            var anystring = "somestring";
            Assert.AreEqual(MsoCTPDockPosition.msoCTPDockPositionLeft, StringExtension.ToDockPosition(anystring));
        }
    }
}
