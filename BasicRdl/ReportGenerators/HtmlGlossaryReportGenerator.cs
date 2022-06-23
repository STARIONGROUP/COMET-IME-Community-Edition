// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlGlossaryReportGenerator.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ReportGenerators
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using DotLiquid;
    using DotLiquid.NamingConventions;

    /// <summary>
    /// The html glossary report generator.
    /// </summary>
    public class HtmlGlossaryReportGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGlossaryReportGenerator"/> class.
        /// </summary>
        public HtmlGlossaryReportGenerator()
        {
            Template.NamingConvention = new CSharpNamingConvention();
            this.RegisterTypes();
        }

        /// <summary>
        /// Creates a <see cref="List{Thing}"/> that contains <see cref="Glossary"/> and its containing 
        /// <see cref="Term"/>.
        /// </summary>
        /// <param name="glossary">
        /// The <see cref="Glossary"/> to generate content from.
        /// </param>
        /// <returns>
        /// A <see cref="List{Thing}"/> ordered by shortname.
        /// </returns>
        private IEnumerable<Thing> CreateSortedContent(Glossary glossary)
        {
            var things = new List<Thing>();

            things.AddRange(glossary.Term.OrderBy(t=>t.ShortName));

            return things;
        }

        /// <summary>
        /// Renders a <see cref="Glossary"/> as HTML report
        /// </summary>
        /// <param name="glossary">
        /// The glossary.
        /// </param>
        /// <returns>
        /// an HTML string that represents the <see cref="Glossary"/> in HTML
        /// </returns>
        public string Render(Glossary glossary)
        {
            if (glossary == null)
            {
                throw new ArgumentNullException("glossary", "The Glossary may not be null");
            }

            var htmlTemplate = this.ReadEmbeddedTemplate();
            var template = Template.Parse(htmlTemplate);

            var sortedContent = this.CreateSortedContent(glossary);

            var htmlReport = template.Render(Hash.FromAnonymousObject(new { content = sortedContent }));

            return htmlReport;
        }

        /// <summary>
        /// Writes the report to disk
        /// </summary>
        /// <param name="report">
        /// The HTML report as a string
        /// </param>
        /// <param name="path">
        /// the path where the HTML report needs to be written to
        /// </param>
        public void Write(string report, string path)
        {
            File.WriteAllText(path, report, Encoding.UTF8);
        }

        /// <summary>
        /// Registeres the required types with the <see cref="Template"/> as safe types
        /// </summary>
        private void RegisterTypes()
        {
            Template.RegisterSafeType(typeof(Term), new[] {"ShortName", "Name", "Definition", "IsDeprecated" });
            Template.RegisterSafeType(typeof(Definition), new[] { "Content", "LanguageCode", "Container", "ClassKind" });
        }

        /// <summary>
        /// Reads the embedded HTML template from a resource
        /// </summary>
        /// <returns>
        /// A string with the contents of the html template.
        /// </returns>
        private string ReadEmbeddedTemplate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "BasicRdl.Resources.DotLiquidTemplate.GlossaryReportTemplate.liquid";

            var stream = assembly.GetManifestResourceStream(resourceName);

            if(stream == null)
            {
                throw new NullReferenceException(string.Format("Could not load the {0} generator template into a stream", resourceName));
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                var result = reader.ReadToEnd();
                return result;
            }
        }
    }
}