// -------------------------------------------------------------------------------------------------
// <copyright file="DialogCloser.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation
{
    using System.Windows;

    /// <summary>
    /// The <see cref="DialogCloser"/> is an Attached Behavior that allows a class that is bound to a <see cref="Window"/> to close that
    /// <see cref="Window"/> using a binding
    /// </summary>
    /// <example>
    /// <Window
    /// xmlns:xc="clr-namespace:CDP4Composition.Navigation"
    /// xc:DialogCloser.DialogResult="{Binding DialogResult}">
    /// </Window>
    /// </example>
    /// <remarks>
    /// based on the following <seealso cref="http://blog.excastle.com/2010/07/25/mvvm-and-dialogresult-with-no-code-behind/"/> 
    /// </remarks>
    public static class DialogCloser
    {
        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="SetDialogResult"/> method.
        /// </summary>
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloser),
                new PropertyMetadata(DialogResultChanged));

        /// <summary>
        /// Sets the dialog result of the target <see cref="Window"/>.
        /// </summary>
        /// <param name="target">
        /// the <see cref="Window"/> that has this behavior attached.
        /// </param>
        /// <param name="value">
        /// The dialog result.
        /// </param>
        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }

        /// <summary>
        /// Event handler for a change on the <see cref="DialogResultProperty"/>.
        /// </summary>
        /// <param name="d">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The event arguments.
        /// </param>
        private static void DialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;

            // DialogResult cannot be set twice
            if (window != null && !window.DialogResult.HasValue)
            {
                window.DialogResult = (bool?)e.NewValue;
            }
        }
    }
}
