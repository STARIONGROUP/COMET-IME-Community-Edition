// -------------------------------------------------------------------------------------------------
// <copyright file="ContextMenuBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using CDP4Composition.Mvvm;
    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Grid;
    using NLog;

    /// <summary>
    /// This behavior creates the context menu when the user right mouse clicks on a row in a <see cref="IBrowserViewModelBase"/>.
    /// Also Initializes the commands of the browser when the user left mouse clicks on a row in a <see cref="IBrowserViewModelBase"/>.
    /// </summary>
    public class ContextMenuBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// The current logger
        /// </summary>
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.MouseRightButtonUp += this.PopulateContextMenu;
        }

        /// <summary>
        /// Remove the subscription on detaching
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.MouseRightButtonUp -= this.PopulateContextMenu;
        }

        /// <summary>
        /// Event handler for the <see cref="MouseRightButtonUp"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the right mouse button is released while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void PopulateContextMenu(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var control = sender as GridDataControlBase;
                if (control == null)
                {
                    return;
                }

                var browser = control.DataContext as IBrowserViewModelBase<Thing>;
                if (browser == null)
                {
                    return;
                }

                browser.ComputePermission();
                browser.PopulateContextMenu();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "A problem occurend when populating the ContextMenu");
            }
        }
    }
}