// ------------------------------------------------------------------------------------------------
// <copyright file="IThingDialogView.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Navigation.Interfaces
{
    using Microsoft.Practices.Prism.Mvvm;

    public interface IThingDialogView : IView
    {
        /// <summary>
        /// Opens a window and returns only when the newly opened window is closed.
        /// </summary>
        /// <returns>
        /// A System.Nullable{T} value of type System.Boolean that specifies whether the activity was accepted (true) or canceled (false).
        /// The return value is the value of the System.Windows.Window.DialogResult property before a window closes.
        /// </returns>
        bool? ShowDialog();
    }
}
