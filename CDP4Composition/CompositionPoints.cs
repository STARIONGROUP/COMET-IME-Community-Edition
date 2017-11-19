// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompositionPoints.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    /// <summary>
    /// The static <see cref="CompositionPoints"/> provides Contract names for MEF Export and Import attributes.
    /// </summary>
    public static class CompositionPoints
    {
        /// <summary>
        /// The composition point for DataTemplate of View Models to bind Views
        /// </summary>
        public const string ViewInstantiation = "CompositionPoints.ViewInstantiation";

        /// <summary>
        /// The composition point for Themes defined in XAML
        /// </summary>
        public const string Themes = "CompositionPoints.Themes";

        /// <summary>
        /// The composition point for Styles defined in XAML
        /// </summary>
        public const string Styles = "CompositionPoints.Styles";

        /// <summary>
        /// The composition point for Views defined in XAML
        /// </summary>
        public const string Views = "CompositionPoints.Views";
    }
}
