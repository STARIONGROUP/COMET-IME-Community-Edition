// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeDomCodeCompiler.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood.
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

namespace CDP4Reporting.ViewModels
{
    using CDP4Reporting.ReportScript;

    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using CDP4Reporting.Utilities;

    using NLog;

    using CompilerResults = CDP4Reporting.ReportScript.CompilerResults;

    public class CodeDomCodeCompiler : CodeCompilerBase
    {
        /// <summary>
        /// The logger for the current class
        /// </summary>
        private readonly Logger logger = LogManager.GetLogger("ForceLogger");

        /// <summary>
        /// Holds a reference to the assemblies cached from the current AppDomain. 
        /// This is used to avoid the overhead of retrieving the assembly locations multiple times during compilation.
        /// Also a bugfix for facade assemblies loaded in the first compile run, which are not loaded in subsequent runs, 
        /// causing the assembly retrieval to fail and the compiler to throw an exception.
        /// </summary>
        private static string[] cachedAssemblyLocations;

        /// <summary>
        /// Holds a lock object for synchronizing access to the cached assembly locations. This ensures that only one thread can retrieve and cache the assembly locations at a time, preventing
        /// </summary>
        private static readonly object compilerLock = new object();

        /// <summary>
        /// Sets a value indicating that only WhiteListed Assemblies are allowed for compilation
        /// </summary>
        public static List<string> ExtraWhiteListAssemblies = [];

        /// <summary>
        /// Creates a new instance of the <see cref="CodeDomCodeCompiler"/> class
        /// </summary>
        /// <param name="onOutput">An <see cref="Action{T}"/> of type <see cref="string"/> that is invoked when user output is needed during compilation or data retrieval</param>
        public CodeDomCodeCompiler(Action<string> onOutput) : base(onOutput)
        {
        }

        /// <summary>
        /// Compiles source code and returns the <see cref="CDP4Reporting.ReportScript.CompilerResults"/>
        /// </summary>
        /// <param name="source">The source code</param>
        /// <param name="assemblies"><see cref="IEnumerable{T}"/> of type <see cref="string"/> that holds locations of the referenced assemblies</param>
        /// <returns>The <see cref="CDP4Reporting.ReportScript.CompilerResults"/></returns>
        public override CompilerResults Compile(string source, IEnumerable<string> assemblies)
        {
            var compiler = new Microsoft.CSharp.CSharpCodeProvider();

            var parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                WarningLevel = 0
            };

            var logAssemblies = false;
            var useWhiteListAssemblies = false;
            var extraWhiteListAssemblies = new List<string>();

            lock (compilerLock)
            {
                if (source.ToUpper().Contains("[LogAssemblies]".ToUpper()))
                {
                    logAssemblies = true;
                    source = Regex.Replace(source, Regex.Escape("[LogAssemblies]"), "", RegexOptions.IgnoreCase);
                }

                if (source.ToUpper().Contains("[UseWhiteListAssemblies]".ToUpper()))
                {
                    useWhiteListAssemblies = true;
                    source = Regex.Replace(source, Regex.Escape("[UseWhiteListAssemblies]"), "", RegexOptions.IgnoreCase);
                }

                var pattern = """\[WhiteListAssembly\("([^"]+)"\)\]""";

                extraWhiteListAssemblies = Regex.Matches(source, pattern)
                    .OfType<Match>()
                    .Select(m => m.Groups[1].Value)
                    .ToList();

                source = Regex.Replace(source, pattern, "");
            }

            if (useWhiteListAssemblies)
            {
                DebugUtilities.AddOutput("Using WhiteList Assemblies...");

                parameters.ReferencedAssemblies.Add("mscorlib.dll");
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.Linq.dll");
                parameters.ReferencedAssemblies.Add("System.Linq.dll");
                parameters.ReferencedAssemblies.Add("System.Data.dll");
                parameters.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
                parameters.ReferencedAssemblies.Add("System.Core.dll");
                parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
                parameters.ReferencedAssemblies.Add("CDP4Common.dll");
                parameters.ReferencedAssemblies.Add("CDP4Composition.dll");
                parameters.ReferencedAssemblies.Add("CDP4Dal.dll");
                parameters.ReferencedAssemblies.Add("CDP4JsonSerializer.dll");
                parameters.ReferencedAssemblies.Add("./plugins/CDP4Reporting/refs/netstandard.dll");
                parameters.ReferencedAssemblies.Add("./plugins/CDP4Reporting/refs/System.Runtime.dll");
                parameters.ReferencedAssemblies.Add("./plugins/CDP4Reporting/CDP4Reporting.dll");
                parameters.ReferencedAssemblies.Add("./plugins/CDP4Reporting/CDP4ReportingPlugin.dll");
                parameters.ReferencedAssemblies.Add("CDP4RequirementsVerification.dll");
                parameters.ReferencedAssemblies.Add("Newtonsoft.Json.dll");
                parameters.ReferencedAssemblies.Add("System.Text.Json.dll");

                if (extraWhiteListAssemblies.Any())
                {
                    foreach (var assembly in extraWhiteListAssemblies)
                    {
                        DebugUtilities.AddOutput($"Using extra assembly: {assembly}");
                        parameters.ReferencedAssemblies.Add(assembly);
                    }
                }
            }
            else
            {
                if (cachedAssemblyLocations == null)
                {
                    lock (compilerLock)
                    {
                        DebugUtilities.AddOutput("Setting Cached Assemblies...");

                        cachedAssemblyLocations ??= AppDomain.CurrentDomain.GetAssemblies()
                            .Where(x => !x.IsDynamic)
                            .Where(x => !string.IsNullOrEmpty(x.Location))
                            .Select(x => x.Location)
                            .ToArray();
                    }
                }

                parameters.ReferencedAssemblies.AddRange(cachedAssemblyLocations);
            }

            if (logAssemblies)
            {
                var loadedAssemblies= AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !x.IsDynamic)
                    .Where(x => !string.IsNullOrEmpty(x.Location))
                    .Select(x => (x.FullName, x))
                    .OrderBy(x => x.FullName)
                    .ToArray();

                this.logger?.Info($"Loaded Assemblies: \n{string.Join("\n", loadedAssemblies.Select(x => x.FullName + " - " + x.x.Location))}\n");
                this.logger?.Info($"To Be Loaded Assemblies: \n{string.Join("\n", parameters.ReferencedAssemblies.Cast<string>().ToArray())}\n");
            }

            var result = compiler.CompileAssemblyFromSource(parameters, source);

            var errors = new List<string>();

            foreach (var error in result.Errors)
            {
                errors.Add(error.ToString());
            }

            if (errors.Any())
            {
                return new CompilerResults(null, errors);
            }

            return new CompilerResults(result.CompiledAssembly, errors);
        }
    }
}
