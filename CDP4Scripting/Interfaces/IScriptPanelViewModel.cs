// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IScriptPanelViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2023 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Interfaces
{
    using System.Windows.Controls;

    using CDP4Composition;

    using CDP4Dal;

    using CDP4Scripting.Helpers;
    using CDP4Scripting.ViewModels;

    using ICSharpCode.AvalonEdit;

    /// <summary>
    /// Represents the interface of the <see cref="ScriptPanelViewModel"/>
    /// </summary>
    public interface IScriptPanelViewModel : IPanelViewModel
    {
        /// <summary>
        /// Gets and sets the AvalonEditor.
        /// </summary>
        TextEditor AvalonEditor { get; set; }

        /// <summary>
        /// Gets or sets the File extension.
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Gets or sets the visibility of the run button.
        /// </summary>
        bool IsRunButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the combobox which allow a user to select a <see cref="ISession"/>.
        /// </summary>
        bool IsSelectSessionVisible { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the clear output button.
        /// </summary>
        bool IsClearOutputButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the stop button.
        /// </summary>
        bool IsStopScriptButtonVisible { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the terminal panels.
        /// </summary>
        bool AreTerminalsVisible { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the stop button.
        /// </summary>
        bool IsScriptVariablesPanelVisible { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ISession"/> selected by the user to execute the script.
        /// </summary>
        ISession SelectedSession { get; set; }

        /// <summary>
        /// Gets or sets the output terminal.
        /// </summary>
        OutputTerminal OutputTerminal { get; }

        /// <summary>
        /// Gets or sets the caption of the panel.
        /// </summary>
        new string Caption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content of the Panel is dirty or not.
        /// </summary>
        new bool IsDirty { get; set; }

        /// <summary>
        /// Clears the variables of the scope of the script.
        /// </summary>
        void ClearScopeVariables();

        /// <summary>
        /// Executes the supplied script
        /// </summary>
        /// <param name="script">The string of the script that is to be executed.</param>
        void Execute(string script);

        /// <summary>
        /// Loads a style sheet to enable the syntax highlighting in the script.
        /// </summary>
        /// <param name="sheetPath">The path of the embedded resource which contains the symbols to highlight.</param>
        void LoadHighlightingSheet(string sheetPath);
    }
}
