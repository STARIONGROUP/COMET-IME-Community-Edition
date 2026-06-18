// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ColumnChooserBehavior.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary,
//              Rowan de Voogt
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm.Behaviours
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// A <see cref="Behavior{T}"/> that turns a <see cref="Button"/> into a "Column Chooser" button for a bound
    /// <see cref="DataViewBase"/>: clicking it shows the grid's column chooser (the same dialog reachable from the grid's
    /// right-click header menu) and the button is given the same glyph that the menu item uses. Keeping this logic in a
    /// behavior avoids placing code in the view's code-behind.
    /// </summary>
    public class ColumnChooserBehavior : Behavior<Button>
    {
        /// <summary>
        /// The name of the embedded resource in the DevExpress grid assembly that holds the column-chooser glyph used by
        /// the grid's right-click header menu.
        /// </summary>
        private const string ColumnChooserImageResourceName = "DevExpress.Xpf.Grid.Images.ItemColumnChooser.png";

        /// <summary>
        /// The <see cref="DependencyProperty"/> for the <see cref="View"/>.
        /// </summary>
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register(
                nameof(View),
                typeof(DataViewBase),
                typeof(ColumnChooserBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the <see cref="DataViewBase"/> whose column chooser is shown when the <see cref="Button"/> is clicked.
        /// </summary>
        public DataViewBase View
        {
            get => (DataViewBase)this.GetValue(ViewProperty);
            set => this.SetValue(ViewProperty, value);
        }

        /// <summary>
        /// Executes when this behavior is attached to its <see cref="Button"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            this.SetColumnChooserGlyph();
            this.AssociatedObject.Click += this.OnClick;
        }

        /// <summary>
        /// Executes when this behavior is detached from its <see cref="Button"/>.
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.Click -= this.OnClick;

            base.OnDetaching();
        }

        /// <summary>
        /// Handles the click of the <see cref="Button"/> by showing the column chooser of the bound <see cref="View"/>.
        /// </summary>
        /// <param name="sender">The <see cref="Button"/>.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/>.</param>
        private void OnClick(object sender, RoutedEventArgs e)
        {
            this.View?.ShowColumnChooser();
        }

        /// <summary>
        /// Replaces the textual content of the <see cref="Button"/> with the same glyph that the grid's right-click header
        /// menu uses. The textual content is kept as a fallback if the glyph cannot be loaded.
        /// </summary>
        private void SetColumnChooserGlyph()
        {
            try
            {
                using (var stream = typeof(GridControl).Assembly.GetManifestResourceStream(ColumnChooserImageResourceName))
                {
                    if (stream == null)
                    {
                        return;
                    }

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    this.AssociatedObject.Content = new Image { Source = bitmap, Width = 16, Height = 16 };
                }
            }
            catch
            {
                // keep the textual fallback content if the glyph cannot be loaded
            }
        }
    }
}
