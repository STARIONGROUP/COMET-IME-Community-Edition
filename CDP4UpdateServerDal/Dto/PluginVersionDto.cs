﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginVersionDto.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Kamil Wojnowski, Nathanael Smiechowski.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4UpdateServerDal.Dto
{
    using System;

    /// <summary>
    /// The Data Transfer Object representation of the <see cref="PluginVersionDto"/> class.
    /// </summary>
    public class PluginVersionDto
    {
        /// <summary>
        /// Gets or sets the PreRelease value.
        /// </summary>
        public bool IsPreRelease { get; set; }

        /// <summary>
        /// Gets or sets the MinIMEVersion.
        /// </summary>
        public string MinIMEVersion { get; set; }

        /// <summary>
        /// Gets or sets the Release DateTime value.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// Gets or sets the VersionNumber.
        /// </summary>
        public string VersionNumber { get; set; }

    }
}