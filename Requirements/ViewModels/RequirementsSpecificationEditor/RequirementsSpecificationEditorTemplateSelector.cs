// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationEditorTemplateSelector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.RequirementsSpecificationEditor
{
    using System.Windows;
    using System.Windows.Controls;
    
    /// <summary>
    /// The purpose of the <see cref="RequirementsSpecificationEditorTemplateSelector"/> is to select the appropriate
    /// <see cref="DataTemplate"/> for a RequirementsDocument
    /// </summary>
    public class RequirementsSpecificationEditorTemplateSelector : DataTemplateSelector
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
            if (item is CDP4CommonView.RequirementsSpecificationRowViewModel)
            {
                return this.RequirementsSpecificationTemplate;
            }
            
            if (item is CDP4CommonView.RequirementRowViewModel)
            {
                return this.RequirementTemplate;
            }

            if (item is CDP4CommonView.RequirementsGroupRowViewModel)
            {
                return this.RequirementsGroupTemplate;
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> that is applicable to a <see cref="RequirementsSpecificationRowViewModel"/>
        /// </summary>
        public DataTemplate RequirementsSpecificationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> that is applicable to a <see cref="RequirementsGroupRowViewModel"/>
        /// </summary>
        public DataTemplate RequirementsGroupTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> that is applicable to a <see cref="RequirementRowViewModel"/>
        /// </summary>
        public DataTemplate RequirementTemplate { get; set; }
    }
}
