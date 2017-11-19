// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DragInfo.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.DragDrop
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// The purpose of the <see cref="DragInfo"/> class is to carry information about a drag operation
    /// </summary>
    public class DragInfo : IDragInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DragInfo"/> class.
        /// </summary>
        /// <param name="sender">
        /// The sender of the mouse event that initiated the drag.
        /// </param>
        /// <param name="e">
        /// The mouse event that initiated the drag.
        /// </param>
        public DragInfo(object sender, MouseButtonEventArgs e)
        {
            this.DragStartPosition = e.GetPosition(null);
            this.Effects = DragDropEffects.None;
            this.MouseButton = e.ChangedButton;
            this.VisualSource = sender as UIElement;

            var control = sender as Control;            
            if (control != null)
            {
                if (SelectionHelper.IsMultiSelectEnabled(control))
                {
                    var selectedItems = SelectionHelper.GetSelectedItems(control);
                    if (selectedItems.Count > 1)
                    {
                        this.IsMultiSelect = true;
                        this.Payload = selectedItems;
                    }
                    else
                    {
                        this.IsMultiSelect = false;
                        foreach (var selectedItem in selectedItems)
                        {
                            this.Payload = selectedItem;
                        }
                    }
                }
                else
                {
                    this.IsMultiSelect = false;
                    this.Payload = SelectionHelper.GetSelectedItem(control);
                }                
            }
        }

        /// <summary>
        /// Gets a value indicating whether the Payload is a the result of a Multi-Select or a Single-Select enabled control
        /// </summary>
        public bool IsMultiSelect { get; private set; }

        /// <summary>
        /// Gets or sets the data that is being dragged
        /// </summary>
        public object Payload { get; set; }

        /// <summary>
        /// Gets or sets the allowed effects for the drag.
        /// </summary>
        /// <remarks>
        /// The <see cref="Effects"/> property must be set to a value other than <see cref="DragDropEffects.None"/> by a drag handler in order 
        /// for a drag to start.
        /// </remarks>
        public DragDropEffects Effects { get; set; }

        /// <summary>
        /// Gets the mouse button that initiated the drag.
        /// </summary>
        public MouseButton MouseButton { get; private set; }

        /// <summary>
        /// Gets the position of the mouse-click that initiated the drag, relative to <see cref="VisualSource"/>.
        /// </summary>
        public Point DragStartPosition { get; private set; }

        /// <summary>
        /// Gets the control that initiated the drag.
        /// </summary>
        public UIElement VisualSource { get; private set; }
    }
}
