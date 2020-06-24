// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="RHEA System S.A.">
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

namespace CDP4PluginPackager
{
    using System;
    using System.IO;
    using System.Linq;

    internal class Program
    {
        /// <summary>
        /// Main entry for the PluginPackager, handles exception and notify the user in the output build window of visual studio
        /// </summary>
        /// <param name="args">Array containing all command line arguments</param>
        static void Main(string[] args)
        {
            try
            {
                string path = null;

                if (args == null || !args.Any(Directory.Exists))
                {
                    path = Directory.GetCurrentDirectory();
                }
                else
                {
                    path = args.FirstOrDefault(Directory.Exists);
                }

                var shouldPluginGetPacked = args?.Any(a => a.ToLower() == "pack") == true;

                var buildConfiguration = GetBuildConfigurationFromArgs(args);

                var targetFramework = GetTargetFrameworkFromArgs(args);

                var buildPlatform = GetBuildPlatformFromArgs(args);

                var plugingPacker = new SdkPluginPackager(path, shouldPluginGetPacked, buildConfiguration, targetFramework, buildPlatform);

                plugingPacker.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
                throw;
            }
        }

        /// <summary>
        /// Gets the Build Configuration (Debug/Release) from the command line arguments
        /// </summary>
        /// <param name="args">The array of command line arguments</param>
        /// <returns>Build Configuration when found in arguments, otherwise null</returns>
        private static string GetBuildConfigurationFromArgs(string[] args)
        {
            var configParameterPosition = Array.FindIndex(args, x => x.StartsWith("config:"));
            
            return 
                configParameterPosition >= 0 
                    ? args[configParameterPosition].Split(new[] { ':' }, StringSplitOptions.None)[1] 
                    : null;
        }

        /// <summary>
        /// Gets the Target Framework (net452, net48, netcore31, enz...) from the command line arguments
        /// </summary>
        /// <param name="args">The array of command line arguments</param>
        /// <returns>Target Framework when found in arguments, otherwise null</returns>
        private static string GetTargetFrameworkFromArgs(string[] args)
        {
            var targetFrameworkParameterPosition = Array.FindIndex(args, x => x.StartsWith("framework:"));

            return 
                targetFrameworkParameterPosition >= 0 
                    ? args[targetFrameworkParameterPosition].Split(new[] { ':' }, StringSplitOptions.None)[1] 
                    : null;
        }

        /// <summary>
        /// Gets the Build Platform (AnyCpu, x86, x64, enz...) from the command line arguments
        /// </summary>
        /// <param name="args">The array of command line arguments</param>
        /// <returns>Build Platform when found in arguments, otherwise null</returns>
        private static string GetBuildPlatformFromArgs(string[] args)
        {
            var buildPlatformParameterPosition = Array.FindIndex(args, x => x.StartsWith("platform:"));

            return
                buildPlatformParameterPosition >= 0
                    ? args[buildPlatformParameterPosition].Split(new[] { ':' }, StringSplitOptions.None)[1]
                    : null;
        }
    }
}
