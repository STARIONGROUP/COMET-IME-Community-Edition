// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorCompletionData.cs" company="RHEA System S.A.">
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
    using System;

    using ICSharpCode.AvalonEdit.CodeCompletion;
    using ICSharpCode.AvalonEdit.Document;
    using ICSharpCode.AvalonEdit.Editing;

    /// <summary>
    /// Sets the content of the editor auto-completion window.
    /// </summary>
    public class EditorCompletionData : ICompletionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EditorCompletionData"/> class.
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="description">The description</param>
        public EditorCompletionData(string text, string description)
        {
            this.Text = text;
            this.Description = description;
        }

        /// <summary>
        /// Gets the image to display.
        /// </summary>
        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the text to display.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public object Description { get; private set; }

        /// <summary>
        /// Gets or sets the content. This can be the same as 'Text', or a WPF UIElement if you want to display rich content.
        /// </summary>
        public object Content
        {
            get { return this.Text; }
        }

        /// <summary>
        /// Gets the priority. This property is used in the selection logic.
        /// You can use it to prefer selecting those items which the user is accessing most frequently.
        /// </summary>
        public double Priority { get; }

        /// <summary>
        /// Performs the completion.
        /// </summary>
        /// <param name="textArea">The text area on which completion is performed.</param>
        /// <param name="completionSegment">The text segment used by the completion window</param>
        /// <param name="insertionRequestEventArgs">The event for the insertion request.</param>
        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
