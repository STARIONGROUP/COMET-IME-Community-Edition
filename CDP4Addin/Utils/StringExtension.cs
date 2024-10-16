// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringExtension.cs" company="Starion Group S.A.">
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

namespace CDP4AddinCE.Utils
{
    using System.Runtime.InteropServices;

    using CDP4Composition;

    using NetOffice.OfficeApi.Enums;

    /// <summary>
    /// Static extension class to convert a string to a <see cref="MsoCTPDockPosition"/>
    /// </summary>
    [ComVisible(false)]
    public static class StringExtension
    {
        /// <summary>
        /// Convert a <see cref="string"/> to a <see cref="MsoCTPDockPosition"/>
        /// </summary>
        /// <param name="str">
        /// The string to convert
        /// </param>
        /// <returns>
        /// a <see cref="MsoCTPDockPosition"/> value, the default is <see cref="MsoCTPDockPosition.msoCTPDockPositionLeft"/>
        /// </returns>
        public static MsoCTPDockPosition ToDockPosition(this string str)
        {
            MsoCTPDockPosition dockPosition;

            switch (str)
            {
                case LayoutGroupNames.LeftGroup:
                    dockPosition = MsoCTPDockPosition.msoCTPDockPositionLeft;
                    break;
                case LayoutGroupNames.RightGroup:
                    dockPosition = MsoCTPDockPosition.msoCTPDockPositionRight;
                    break;
                default:
                    dockPosition = MsoCTPDockPosition.msoCTPDockPositionLeft;
                    break;
            }

            return dockPosition;
        }
    }
}
