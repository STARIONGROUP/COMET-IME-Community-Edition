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
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Reporting.ReportScript;

    using CompilerResults = CDP4Reporting.ReportScript.CompilerResults;

    public class CodeDomCodeCompiler : CodeCompilerBase
    {
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

            var currentAssemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => !x.IsDynamic)
                    .Select(x => x.Location)
                    .ToArray();

            parameters.ReferencedAssemblies.AddRange(currentAssemblies);

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
