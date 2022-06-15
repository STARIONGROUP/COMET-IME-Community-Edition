// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextDataProvider.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.Views
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Data;

    using NLog;

    /// <summary>
    /// Provides a text read from a File Source
    /// </summary>
    public class TextDataProvider : DataSourceProvider
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets or sets the path of the file
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Gets the Result of the data provider
        /// </summary>
        public object Result { get; private set; }

        /// <summary>
        /// Query the text from the file
        /// </summary>
        protected override void BeginQuery()
        {
            try
            {
                Logger.Debug(this.Uri);

                var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var path = Path.Combine(assemblyPath, this.Uri);
                this.Result = File.ReadAllText(path);

                var extension = this.Uri.Substring(this.Uri.Length -4);
                if (extension == ".rtf")
                {
                    // Use the RichTextBox to convert the RTF code to plain text.
                    var rtBox = new System.Windows.Forms.RichTextBox { Rtf = (string)this.Result };
                    this.Result = rtBox.Text;
                }

                this.OnQueryFinished(this.Result, null, null, null);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                this.Result = e;
                this.OnQueryFinished(null, e, null, null);
            }
        }
    }
}