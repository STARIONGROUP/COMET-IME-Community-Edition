// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WxsObject.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4IMEInstaller.Tests
{
    /// <summary>
    /// Contains all data needed to create a assembly entry in DevExpress.wxs 
    /// </summary>
    public class WxsObject
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assemblyName">The assembly's' name</param>
        /// <param name="guid">The assembly's guid</param>
        /// <param name="componentId">The assembly's component id</param>
        /// <param name="fileId">The assembly's file id</param>
        public WxsObject(string assemblyName, string guid, string componentId, string fileId)
        {
            this.AssemblyName = assemblyName;
            this.ComponentId = componentId;
            this.FileId = fileId;
            this.Guid = guid;
        }

        /// <summary>
        /// Gets the assembly's' name
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets the assembly's guid
        /// </summary>
        public string ComponentId { get; }

        /// <summary>
        /// Gets the assembly's component id
        /// </summary>
        public string FileId { get; }

        /// <summary>
        /// Gets the assembly's file id
        /// </summary>
        public string Guid { get; }
    }
}
