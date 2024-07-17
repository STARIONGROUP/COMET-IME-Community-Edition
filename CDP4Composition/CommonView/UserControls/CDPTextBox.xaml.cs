// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDPTextBox.xaml.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.UserControls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using CDP4Common.CommonData;
    using CDP4Composition;
    using CommonServiceLocator;
    using NLog;

    /// <summary>
    /// Interaction logic for CDPTextBox xaml
    /// </summary>
    public partial class CDPTextBox
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The basic foreground dependency property to provide coloring of text.
        /// </summary>
        public static DependencyProperty BaseForegroundProperty = DependencyProperty.Register("BaseForeground", typeof(Brush), typeof(CDPTextBox), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Black), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Indicates whether the scrolling event is enabled.
        /// </summary>
        private bool scrollingEventEnabled;

        /// <summary>
        /// The list of <see cref="StringDecoration"/>s that are applicable to this text box.
        /// </summary>
        private List<StringDecoration> decorations = new List<StringDecoration>();

        /// <summary>
        /// The special terms service.
        /// </summary>
        private ISpecialTermsService termsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CDPTextBox"/> class.
        /// </summary>
        public CDPTextBox()
        {
            this.TextChanged += this.textBox_TextChanged;
            this.Foreground = new SolidColorBrush(Colors.Transparent);
            this.Background = new SolidColorBrush(Colors.Transparent);
            
            InitializeComponent();
            this.InitializeDecorators();
        }

        /// <summary>
        /// Gets or sets the list of the Decorative attributes assigned to the text
        /// </summary>
        public List<StringDecoration> Decorations
        {
            get { return this.decorations; }
            set { this.decorations = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="BaseForegroundProperty"/> dependency property.
        /// </summary>
        public Brush BaseForeground
        {
            get { return (Brush)GetValue(BaseForegroundProperty); }
            set { SetValue(BaseForegroundProperty, value); }
        }

        /// <summary>
        /// Overrides the draw call for this control
        /// </summary>
        /// <param name="drawingContext">The drawing context</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                return;
            }

            this.EnsureScrolling();

            // Text that matches the textbox's
            var formattedText = new FormattedText(this.Text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(this.FontFamily.Source), this.FontSize, this.BaseForeground);  

            var leftMargin = 2.0 + this.BorderThickness.Left;
            var topMargin = this.BorderThickness.Top;

            // space for scrollbar
            formattedText.MaxTextWidth = this.ViewportWidth;

            // Adjust for scrolling
            formattedText.MaxTextHeight = Math.Max(this.ActualHeight + this.VerticalOffset, 0);

            // Restrict text to textbox
            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight))); 

            // TextColor
            foreach (var decoration in this.decorations)
            {
                if (decoration.DecorationType == DecorationType.TextColor)
                {
                    var ranges = decoration.Ranges(this.Text);

                    foreach (var range in ranges)
                    {
                        formattedText.SetForegroundBrush(decoration.Brush, range.Start, range.Length);
                    }
                }
            }

            var point = new Point(leftMargin, topMargin - this.VerticalOffset);
            drawingContext.DrawText(formattedText, point);
        }

        /// <summary>
        /// Initializes the text decorators.
        /// </summary>
        private void InitializeDecorators()
        {
            try
            {
                this.termsService = ServiceLocator.Current.GetInstance<ISpecialTermsService>();

                var termDecorator = new MultiRegexWordDecoration();
                termDecorator.Brush = new SolidColorBrush(Colors.Blue);
                termDecorator.Words.AddRange(this.termsService.TermsList);
                this.Decorations.Add(termDecorator);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "The ServiceLocator instance is null");
            }
        }

        /// <summary>
        /// Eventhandler to handle invalidation of the whole control. Triggerred on every key stroke.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The arguments.</param>
        void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        /// <summary>
        /// Ensures that the scrolling functions.
        /// </summary>
        private void EnsureScrolling()
        {
            if (this.scrollingEventEnabled)
            {
                return;
            }

            var child = VisualTreeHelper.GetChild(this, 0);
            var scrollViewer = VisualTreeHelper.GetChild(child, 0) as ScrollViewer;             
            scrollViewer.ScrollChanged += this.ScrollChanged;              
            this.scrollingEventEnabled = true;
        }

        /// <summary>
        /// Event handler handling the scrolling
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The arguments.</param>
        private void ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        /// <summary>
        /// Update the tooltip on mouse over a term.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CDPTextBox_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!this.termsService.TermsList.Any())
            {
                return;
            }

            var mousePoint = Mouse.GetPosition(this);
            var charPosition = this.GetCharacterIndexFromPoint(mousePoint, true);
            if (charPosition <= 0)
            {
                return;
            }

            var index = 0;
            var i = 0;
            var strings = this.Text.Split(' ');
            while (index + strings[i].Length < charPosition && i < strings.Length)
            {
                index += strings[i++].Length + 1;
            }

            Definition definition;
            var rgx = new Regex("[^a-zA-Z0-9-_]");
            var word = rgx.Replace(strings[i], string.Empty);
            this.termsService.TermDefinitionDictionary.TryGetValue(word, out definition);
            this.ToolTip = definition != null ? definition.Content : string.Empty;
        }
    }
}
