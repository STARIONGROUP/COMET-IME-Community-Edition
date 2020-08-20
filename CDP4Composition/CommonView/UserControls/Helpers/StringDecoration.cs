// -------------------------------------------------------------------------------------------------
// <copyright file="StringDecoration.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.UserControls
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// The string decoration depencency object.
    /// </summary>
    public abstract class StringDecoration : DependencyObject
    {
        /// <summary>
        /// EDecoration Type of the decoration , Default is TextColor
        /// </summary>
        public static DependencyProperty DecorationTypeProperty = DependencyProperty.Register("DecorationType", typeof(DecorationType), typeof(StringDecoration), new PropertyMetadata(DecorationType.TextColor));

        /// <summary>
        /// Gets or sets the <see cref="DecorationType"/> of this <see cref="StringDecoration"/>
        /// </summary>
        public DecorationType DecorationType
        {
            get { return (DecorationType)GetValue(DecorationTypeProperty); }
            set { SetValue(DecorationTypeProperty, value); }
        }

        /// <summary>
        /// Brushed used for the decoration
        /// </summary>
        public static DependencyProperty BrushProperty = DependencyProperty.Register("Brush", typeof(Brush), typeof(StringDecoration),
        new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the <see cref="Brush"/> applied to this decoration.
        /// </summary>
        public Brush Brush
        {
            get { return (Brush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }

        /// <summary>
        /// The list of <see cref="StringRange"/> indicating which substrings are adorned.
        /// </summary>
        /// <param name="Text">The string to match.</param>
        /// <returns>The list of ranges to decorate.</returns>
        public abstract List<StringRange> Ranges(string Text);
    }
}
