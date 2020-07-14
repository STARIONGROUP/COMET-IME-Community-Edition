// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GrapherSaveFileDialog.cs" company="RHEA System S.A.">
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

namespace CDP4Grapher.Utilities
{
    using System;
    using System.IO;

    using DevExpress.Diagram.Core;

    using Microsoft.Win32;

    /// <summary>
    /// Represent a Save File dialog that will save as the outputs of the Grapher tool
    /// </summary>
    public class GrapherSaveFileDialog : IGrapherSaveFileDialog
    {
        /// <summary>
        /// Holds the <see cref="SaveFileDialog"/>
        /// </summary>
        private readonly SaveFileDialog dialog;

        /// <summary>
        /// Gets or Sets the output file extension
        /// </summary>
        public string Extension { get; private set; }

        /// <summary>
        /// Gets or Sets the output file format
        /// </summary>
        public DiagramExportFormat Format { get; private set; }

        /// <summary>
        /// Instanciate a new <see cref="GrapherSaveFileDialog"/>
        /// </summary>
        /// <param name="format"></param>
        public GrapherSaveFileDialog(DiagramExportFormat format)
        {
            this.Format = format;
            this.Extension = format.ToString().ToLower();
            this.dialog = new SaveFileDialog() { FileName = $"CDP4Graph -{ DateTime.Now:yyyy-MM-dd_HH-mm}", OverwritePrompt = true, Filter = $"{ format } file(*.{ this.Extension }) | *.{ this.Extension }; ", AddExtension = true, DefaultExt = this.Extension, ValidateNames = true };
        }

        /// <summary>
        /// Shows up the save dialog
        /// </summary>
        /// <returns>return a assert whether the dialog is showing</returns>
        public bool ShowDialog()
        {
            return this.dialog.ShowDialog() == true;
        }

        /// <summary>
        /// Gets the <see cref="IDisposable"/> <see cref="Stream"/> to write the output in
        /// </summary>
        /// <returns>A <see cref="Stream"/> to write into</returns>
        public Stream OpenFile()
        {
            return this.dialog.OpenFile();
        }
    }
}
