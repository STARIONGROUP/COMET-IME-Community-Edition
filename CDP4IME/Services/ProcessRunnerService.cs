﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandRunnerService.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.Services
{
    using System.ComponentModel.Composition;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;

    /// <summary>
    /// The <see cref="ProcessRunnerService"/> provides methods that allows invoking commands on conhost
    /// </summary>
    [Export(typeof(IProcessRunnerService))]
    public class ProcessRunnerService : IProcessRunnerService
    {
        /// <summary>
        /// Runs the provided <see cref="executable"/> with elevated rights
        /// </summary>
        /// <param name="executable">The executable command path</param>
        public void RunAsAdmin(string executable)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = "msiexec",
                    WorkingDirectory = Path.GetTempPath(),
                    Arguments = $" /i \"{executable}\" ALLUSERS=1",
                    Verb = "runas"
                }
            };

            process.Start();
        }
        
        /// <summary>
        /// Gracefully restart the IME
        /// </summary>
        public void Restart()
        {
            Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}
