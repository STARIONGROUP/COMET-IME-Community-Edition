// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputTerminal.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
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

namespace CDP4Scripting.Helpers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Interfaces;

    /// <summary>
    /// The input terminal of a script panel.
    /// </summary>
    public class InputTerminal : TextBox
    {
        /// <summary>
        /// The enumeration that defines the possible directions to browse the list of the previous commands.
        /// </summary>
        private enum CommandHistoryDirection { Backward, Forward }

        /// <summary>
        /// The symbol that defines a prompt.
        /// </summary>
        private const string Prompt = "> ";

        /// <summary>
        /// The panel view model associated to this input terminal.
        /// </summary>
        private readonly IScriptPanelViewModel panelViewModel;

        /// <summary>
        /// The index in the le list of the previous commands.
        /// </summary>
        private int indexInPreviousCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputTerminal"/> class.
        /// </summary>
        /// <param name="panelViewModel">The <see cref="IScriptPanelViewModel"/> that contains this terminal.</param>
        public InputTerminal(IScriptPanelViewModel panelViewModel)
        {
            this.panelViewModel = panelViewModel;
            this.PreviousCommands = new List<string>();

            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            this.IsUndoEnabled = false;
            this.AcceptsReturn = false;
            this.AcceptsTab = false;
            this.BorderThickness = new Thickness(0);

            this.LastPromptIndex = -1;

            this.AppendText("Results are shown in the output window");
            this.InsertNewPrompt();

            this.TextChanged += (s, e) => this.ScrollToEnd();
        }

        /// <summary>
        /// The list of the previous commands that the user entered.
        /// </summary>
        public List<string> PreviousCommands { get; private set; }

        /// <summary>
        /// The index of the last prompt.
        /// </summary>
        public int LastPromptIndex { get; private set; }

        /// <summary>
        /// Reacts when a key is pressed.
        /// </summary>
        /// <param name="e">The event.</param>
        protected override async void OnPreviewKeyDown(KeyEventArgs e)
        {
            // Test the caret position.

            // If located before the last prompt index
            //    ==> Set the caret at the end of input text, add text, discard the input
            //        if user tries to erase text, else process it.
             if (this.CaretIndex <= this.LastPromptIndex)
            {
                this.CaretIndex = this.LastPromptIndex;
                if (e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left)
                {
                    e.Handled = true;
                }
            }

            // If located after(>=) the last prompt index and user presses the UP key
            //    ==> Launch command history backward, discard the input.
            if (this.CaretIndex >= this.LastPromptIndex && e.Key == Key.Up)
            {
                this.HandleCommandHistoryRequest(CommandHistoryDirection.Backward);
                e.Handled = true;
            }
            // If located after (>=) the last prompt index and user presses the UP key
            //    ==> Launch command history forward, discard the input.
            else if (this.CaretIndex >= this.LastPromptIndex && e.Key == Key.Down)
            {
                this.HandleCommandHistoryRequest(CommandHistoryDirection.Forward);
                e.Handled = true;
            }

            if (!e.Handled && e.Key == Key.Enter)
            {
                await this.HandleEnterKey();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);
        }

        /// <summary>
        /// Inserts a new prompt on a new line.
        /// </summary>
        private void InsertNewPrompt()
        {
            if (this.Text.Length > 0)
            {
                this.Text += this.Text.EndsWith("\n") ? "" : "\n";
            }
                
            this.AppendText(Prompt);
            this.CaretIndex = this.Text.Length;
            this.LastPromptIndex = this.Text.Length;
        }

        /// <summary>
        /// Called when a user requests a command entered in the past.
        /// </summary>
        /// <param name="direction">The direction to browse the list of the previous commands.</param>
        private void HandleCommandHistoryRequest(CommandHistoryDirection direction)
        {
            switch (direction)
            {
                // if user wants the previous command
                case CommandHistoryDirection.Backward:
                    if (this.indexInPreviousCommand > 0)
                        this.indexInPreviousCommand--;
                    if (this.PreviousCommands.Count > 0)
                    {
                        this.Text = this.AddSuffixToLastPrompt(this.PreviousCommands[this.indexInPreviousCommand]);
                        this.CaretIndex = this.Text.Length;
                    }
                    break;
                
                // if user wants the next command
                case CommandHistoryDirection.Forward:
                    if (this.indexInPreviousCommand < this.PreviousCommands.Count - 1)
                        this.indexInPreviousCommand++;
                    if (this.PreviousCommands.Count > 0)
                    {
                        this.Text = this.AddSuffixToLastPrompt(this.PreviousCommands[this.indexInPreviousCommand]);
                        this.CaretIndex = this.Text.Length;
                    }
                    break;
            }
        }

        /// <summary>
        /// Called when the enter key is pushed to execute the command of the user.
        /// </summary>
        private async Task HandleEnterKey()
        {
            var line = this.Text.Substring(this.LastPromptIndex);
            this.AppendText("\n");
            this.LastPromptIndex = int.MaxValue;

            if (line != string.Empty)
            {
                this.PreviousCommands.Add(line);
                await Task.Run(() => this.panelViewModel.Execute(line));
            }
            
            this.indexInPreviousCommand = this.PreviousCommands.Count - 1;
            this.InsertNewPrompt();
        }

        /// <summary>
        /// Adds the string paramter after the last prompt.
        /// </summary>
        /// <param name="suffix">The string to add after the last prompt.</param>
        /// <returns></returns>
        private string AddSuffixToLastPrompt(string suffix)
        {
            return this.Text.Substring(0, this.LastPromptIndex) + suffix;
        }
    }
}
