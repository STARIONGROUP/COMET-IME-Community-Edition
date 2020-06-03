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
        /// <param name="args"></param>
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

                string buildConfiguration = null;

                var configParameterPosition = Array.FindIndex(args, x => x.StartsWith("config:"));

                if (configParameterPosition >= 0)
                {
                    buildConfiguration = args[configParameterPosition].Split(new [] {':'}, StringSplitOptions.None)[1];
                }

                string targetFramework = null;

                var targetFrameworkParameterPosition = Array.FindIndex(args, x => x.StartsWith("framework:"));

                if (targetFrameworkParameterPosition >= 0)
                {
                    targetFramework = args[targetFrameworkParameterPosition].Split(new[] { ':' }, StringSplitOptions.None)[1];
                }


                var projectFile = Directory.EnumerateFiles(path).FirstOrDefault(f => f.EndsWith(".csproj"));

                if (File.ReadAllText(projectFile).Contains("<Project Sdk=\"Microsoft.NET.Sdk\">"))
                {
                    new SdkPluginPackager(path, shouldPluginGetPacked, buildConfiguration, targetFramework).Start();
                }
                else
                {
                    new OldSchoolPluginPackager(path, shouldPluginGetPacked).Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
                throw;
            }
        }
    }
}
