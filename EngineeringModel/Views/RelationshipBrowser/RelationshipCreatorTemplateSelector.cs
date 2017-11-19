// ------------------------------------------------------------------------------------------------
// <copyright file="RelationshipCreatorTemplateSelector.cs" company="ESA">
//      Copyright (c) 2010-2017 European Space Agency. 
//      All rights reserved. See COPYRIGHT.txt for details.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using ViewModels;

    /// <summary>
    /// The <see cref="DataTemplateSelector"/> to create a new <see cref="Relationship"/>
    /// </summary>
    public class RelationshipCreatorTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Select the <see cref="DataTemplate"/> to use based on the value of <paramref name="item"/>
        /// </summary>
        /// <param name="item">The selector parameter</param>
        /// <param name="container">The <see cref="DependencyObject"/></param>
        /// <returns>The <see cref="DataTemplate"/></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var relationshipCreator = item as IRelationshipCreatorViewModel;
            if (relationshipCreator != null)
            {
                if (relationshipCreator is BinaryRelationshipCreatorViewModel)
                {
                    return this.BinaryRelationshipTemplate;
                }
                else if (relationshipCreator is MultiRelationshipCreatorViewModel)
                {
                    return this.MultipleRelationshipTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to create a <see cref="BinaryRelationship"/>
        /// </summary>
        public DataTemplate BinaryRelationshipTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to create a <see cref="MultiRelationship"/>
        /// </summary>
        public DataTemplate MultipleRelationshipTemplate { get; set; }
    }
}