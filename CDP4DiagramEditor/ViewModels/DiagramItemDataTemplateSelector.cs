// -------------------------------------------------------------------------------------------------
// <copyright file="BudgetParameterConfigDataTemplateSelector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels
{
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Composition.Diagram;

    /// <summary>
    /// The <see cref="DataTemplateSelector"/> to select a <see cref="DataTemplate"/> depending on the kind of budget to compute
    /// </summary>
    public class DiagramItemDataTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Selects the template for a <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        /// <param name="item">The <see cref="NamedThingDiagramContentItem"/></param>
        /// <param name="container">the dependency-object</param>
        /// <returns>The <see cref="DataTemplate"/> to use</returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (!(item is NamedThingDiagramContentItem))
            {
                return base.SelectTemplate(item, container);
            }
            
            return item is DiagramPortViewModel ? this.DiagramPortTemplate : this.GenericDiagramItemDataTemplate;

            //switch (vm.ClassKind)
            //{
            //    case ClassKind.ElementDefinition:
            //        return this.DiagramPortTemplate;
            //    default:
            //        return this.DiagramPortTemplate;
            //}
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a mass-budget template
        /// </summary>
        public DataTemplate GenericDiagramItemDataTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for a generic-budget
        /// </summary>
        public DataTemplate DiagramPortTemplate { get; set; }
    }
}
