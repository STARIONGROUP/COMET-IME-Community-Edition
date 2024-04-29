﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsolidatePluginCommand.cs" company="Starion Group S.A.">
//   Copyright (c) 2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Kamil Wojnowski, Nathanael Smiechowski.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4UpdateServerDal.Command
{
    using System;

    using CDP4UpdateServerDal.Dto;
    using System.Collections.Generic;

    /// <summary>
    /// The plugin command 
    /// </summary>
    public class ConsolidatePluginCommand : IConsolidatePluginCommand
    {
        /// <summary>
        /// Gets or sets the plugin IME version. 
        /// </summary>
        public string IMEVersion { get; set; }

        /// <summary>
        /// Gets or sets the list of client plugins
        /// </summary>
        public IList<ClientPluginDto> ClientPlugins { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsolidatePluginCommand"/> class.
        /// </summary>
        public ConsolidatePluginCommand()
        {
            this.ClientPlugins = new List<ClientPluginDto>();
        }
    }
}