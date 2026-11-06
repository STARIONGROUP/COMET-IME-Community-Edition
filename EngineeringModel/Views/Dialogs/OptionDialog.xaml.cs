// ------------------------------------------------------------------------------------------------
// <copyright file="OptionDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System;
    using System.Linq;
    using System.Windows;

    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for OptionDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.Option)]
    public partial class OptionDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionDialog"/> class.
        /// </summary>
        public OptionDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public OptionDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }

        /// <summary>
        /// Re-enables resizing, which the <see cref="IThingDialogNavigationService"/> disables on every dialog before showing it.
        /// The grip in the bottom right corner shows the user that the dialog is resizable.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
        }

        /// <summary>
        /// Prevents the user from shrinking the dialog below the size it opened with.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            this.MinWidth = this.ActualWidth;
            this.MinHeight = this.ActualHeight;
        }

        /// <summary>
        /// Lets every tab fill the window once the user resized it, which switches <see cref="Window.SizeToContent"/> to <see cref="SizeToContent.Manual"/>.
        /// Before that the tabs keep their size caps, as unbounded grids would size the window to all their rows.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == SizeToContentProperty && this.SizeToContent == SizeToContent.Manual)
            {
                this.ElementUsagesGrid?.ClearValue(HeightProperty);
                ClearSizeCaps(this);
            }
        }

        /// <summary>
        /// Clears the locally set <see cref="FrameworkElement.MaxWidth"/> and <see cref="FrameworkElement.MaxHeight"/> of the <paramref name="element"/> and its logical descendants.
        /// </summary>
        /// <param name="element">The root of the logical tree to clear.</param>
        private static void ClearSizeCaps(DependencyObject element)
        {
            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.ClearValue(MaxWidthProperty);
                frameworkElement.ClearValue(MaxHeightProperty);
            }

            foreach (var child in LogicalTreeHelper.GetChildren(element).OfType<DependencyObject>())
            {
                ClearSizeCaps(child);
            }
        }
    }
}
