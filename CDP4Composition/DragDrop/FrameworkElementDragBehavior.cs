// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FrameworkElementDragBehavior.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interactivity;
    using NLog;

    /// <summary>
    /// The drag behavior for any <see cref="FrameworkElement"/>
    /// </summary>
    public class FrameworkElementDragBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// A NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The name of the data format used for drag-n-drop operations
        /// </summary>
        public static readonly DataFormat DataFormat = DataFormats.GetDataFormat("CDP4.DragDrop");

        /// <summary>
        /// The <see cref="IDragInfo"/> object that contains information about the drag operation.
        /// </summary>
        private IDragInfo dragInfo;

        /// <summary>
        /// A value indicating whether a drag operation is currently in progress
        /// </summary>
        private bool dragInProgress;

        /// <summary>
        /// The on attached event handler
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.MouseLeftButtonDown += new MouseButtonEventHandler(this.MouseLeftButtonDown);
            this.AssociatedObject.MouseLeftButtonUp += new MouseButtonEventHandler(this.MouseLeftButtonUp);
            this.AssociatedObject.MouseMove += new MouseEventHandler(this.MouseMove);            
        }

        /// <summary>
        /// Event handler for the <see cref="MouseLeftButtonDown"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the left mouse button is pressed while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 1 || VisualTreeExtensions.HitTestScrollBar(sender, e)
                                  || VisualTreeExtensions.HitTestGridColumnHeader(sender, e))
            {
                this.dragInfo = null;                
                return;
            }

            this.dragInfo = new DragInfo(sender, e);
        }

        /// <summary>
        /// Event handler for the <see cref="MouseLeftButtonUp"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the left mouse button is released while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.dragInfo = null;  
        }

        /// <summary>
        /// Event handler for the <see cref="MouseMove"/> event
        /// </summary>
        /// <remarks>
        /// Occurs when the mouse pointer moves while the mouse pointer is over this element.
        /// </remarks>
        /// <param name="sender">the sender of the event</param>
        /// <param name="e">the <see cref="MouseButtonEventArgs"/> associated to the event</param>
        private void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                this.dragInfo = null;
                return;
            }
            
            if (this.dragInfo != null && !this.dragInProgress)
            {
                var dragStart = this.dragInfo.DragStartPosition;
                var position = e.GetPosition(null);

                if (Math.Abs(position.X - dragStart.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(position.Y - dragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    this.dragInfo.Effects = DragDropEffects.All;
                    var dragSource = this.AssociatedObject.DataContext as IDragSource;
                    if (dragSource != null)
                    {
                        dragSource.StartDrag(this.dragInfo);

                        if (this.dragInfo.Effects != DragDropEffects.None && this.dragInfo.Payload != null)
                        {
                            var data = new DataObject(DataFormat.Name, this.dragInfo.Payload);
                            
                            this.dragInProgress = true;

                            this.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                try
                                {
                                    System.Windows.DragDrop.DoDragDrop(this.AssociatedObject, data, this.dragInfo.Effects);
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, "MouseMove caused an error");
                                }
                                finally
                                {
                                    this.dragInProgress = false;
                                }
                            }
                            ));
                        }
                    }
                }
            }
        }
    }
}