// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvalonEditExtensions.cs" company="Starion Group S.A.">
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

namespace CDP4Scripting.Helpers
{
    using System.Linq;

    using ICSharpCode.AvalonEdit;

    /// <summary>
    /// Additional features for the Avalon text editor.
    /// </summary>
    /// <remarks>
    /// source : http://blog.thomaslebrun.net/category/avalonedit/#.WW8ldYiGOM9
    /// </remarks>
    public static class AvalonEditExtensions
    {
        // The char separators that indicate where to cut the expression
        public static readonly string[] CharSeparator = { "~", "`", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "+", "=", "\"" };

        /// <summary>
        /// Gets the words before the dot.
        /// </summary>
        /// <param name="textEditor">The text Editor.</param>
        /// <returns>The string before the dot.</returns>
        public static string GetWordsBeforeDot(this TextEditor textEditor)
        {
            var wordBeforeDot = string.Empty;

            var caretPosition = textEditor.CaretOffset - 2;

            var lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(caretPosition));

            var text = textEditor.Document.GetText(lineOffset, 1);

            // Get text backward of the mouse position, until the first space or the first separator
            while (!string.IsNullOrWhiteSpace(text) && !CharSeparator.Any(text.Contains))
            {
                wordBeforeDot = text + wordBeforeDot;

                if (caretPosition == 0)
                    break;

                lineOffset = textEditor.Document.GetOffset(textEditor.Document.GetLocation(--caretPosition));

                text = textEditor.Document.GetText(lineOffset, 1);
            }

            return wordBeforeDot;
        }
    }
}
