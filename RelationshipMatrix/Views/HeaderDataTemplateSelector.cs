// -------------------------------------------------------------------------------------------------
// <copyright file="HeaderDataTemplateSelector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix
{
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    /// The purpose of this class is to select the right template based on the column to display
    /// </summary>
    public class HeaderDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Returns a <see cref="DataTemplate"/> based on custom logic.
        /// </summary>
        /// <param name="item">
        /// The data object for which to select the template.
        /// </param>
        /// <param name="container">
        /// The data-bound object.
        /// </param>
        /// <returns>
        /// Returns a <see cref="DataTemplate"/> or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var header = item?.ToString();

            if (string.IsNullOrWhiteSpace(header))
            {
                return base.SelectTemplate(item, container);
            }

            return header == MatrixViewModel.CDP4_NAME_HEADER ? this.NameColumnTemplate : this.ThingColumnTemplate;
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to show that there is a relationship between a source 1 to a source 2
        /// </summary>
        public DataTemplate ThingColumnTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> used for the name of the column.
        /// </summary>
        public DataTemplate NameColumnTemplate { get; set; }
    }
}
