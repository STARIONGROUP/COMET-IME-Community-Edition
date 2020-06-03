// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyLocationLoader.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Addin.Tests.Utils
{
    using System.IO;
    using System.Reflection;

    using CDP4Composition.Utilities;

    /// <summary>
    /// The helper class that make method that uses <code>Assembly.GetExecutingAssembly().Location</code>
    /// testable, in order to find plugins
    /// </summary>
    public class AssemblyLocationLoader : IAssemblyLocationLoader
    {
        /// <summary>
        /// Gets the path of the IME bin folder
        /// </summary>
        /// <returns>the path of the assembly</returns>
        public string GetLocation()
        {
            var frameworkVersion = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Name;
            var testDirectory = Path.Combine(Assembly.GetExecutingAssembly().Location, @"../../../../../");
            return Path.GetFullPath(Path.Combine(testDirectory, $@"CDP4IME\bin\Debug\{frameworkVersion}"));
        }
    }
}
