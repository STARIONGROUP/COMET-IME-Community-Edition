// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AvalonEditExtensions.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
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

            string text = textEditor.Document.GetText(lineOffset, 1);

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