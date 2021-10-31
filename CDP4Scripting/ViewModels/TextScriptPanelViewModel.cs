// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextScriptPanelViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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