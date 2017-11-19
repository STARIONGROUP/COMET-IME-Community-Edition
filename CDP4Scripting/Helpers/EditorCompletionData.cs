// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EditorCompletionData.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
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