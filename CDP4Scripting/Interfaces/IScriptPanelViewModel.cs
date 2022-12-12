// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IScriptPanelViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Interfaces
{
    using System.Windows.Controls;

    using CDP4Composition;
    using CDP4Dal;
    using Helpers;
    using ICSharpCode.AvalonEdit;
    using ViewModels;

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
