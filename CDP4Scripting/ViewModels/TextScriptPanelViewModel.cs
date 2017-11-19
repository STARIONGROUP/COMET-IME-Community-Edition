// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextScriptPanelViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.ViewModels
{
    using CDP4Dal;
    using Interfaces;
    using ReactiveUI;
    using Views;

    /// <summary>
    /// The view-model for the <see cref="ScriptPanel"/> for the text files.
    /// </summary>
    public class TextScriptPanelViewModel : ScriptPanelViewModel
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="TextScriptPanelViewModel"/> class.
        /// </summary>
        /// <param name="panelTitle">The title of the panel associated to this view model.</param> 
        /// <param name="scriptingProxy">A <see cref="IScriptingProxy"/> object to perform the script commands associated to CDP4.</param>
        /// <param name="openSessions">The list of the open <see cref="ISession"/>.</param>
        public TextScriptPanelViewModel(string panelTitle, IScriptingProxy scriptingProxy, ReactiveList<ISession> openSessions) : base(panelTitle, scriptingProxy, "*.txt", openSessions, false)
        {
            this.IsRunButtonVisible = false;
            this.IsSelectSessionVisible = false;
            this.IsClearOutputButtonVisible = false;
            this.IsStopScriptButtonVisible = false;
            this.IsScriptVariablesPanelVisible = false;
        }
    }
}