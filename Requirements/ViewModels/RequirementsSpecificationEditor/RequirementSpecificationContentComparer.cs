// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementSpecificationContentComparer.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels.RequirementsSpecificationEditor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Composition.Mvvm;
    using CDP4Requirements.Utils;

    /// <summary>
    /// The purpose of the <see cref="RequirementSpecificationContentComparer"/> is to sort the content of the contained row-view-models
    /// of the <see cref="RequirementsSpecificationEditorViewModel"/>.
    /// </summary>
    public class RequirementSpecificationContentComparer : IComparer<IRowViewModelBase<Thing>>
    {
        /// <summary>
        /// The Permissible Kind of child <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        private static readonly List<Type> PermissibleRowTypes = new List<Type>
                                                                     {
                                                                         typeof(CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementsSpecificationRowViewModel),
                                                                         typeof(CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementsGroupRowViewModel),
                                                                         typeof(CDP4Requirements.ViewModels.RequirementsSpecificationEditor.RequirementRowViewModel),
                                                                     };
        
        /// <summary>
        /// The <see cref="IComparer{IBreadCrumb}"/> used to compare rows of the <see cref="RequirementsSpecificationEditor"/>
        /// </summary>
        private static readonly BreadCrumbComparer BreadCrumbComparer = new BreadCrumbComparer();

        /// <summary>
        /// Compares two <see cref="IRowViewModelBase{Thing}"/>
        /// </summary>
        /// <param name="x">The first <see cref="IRowViewModelBase{Thing}"/> to compare</param>
        /// <param name="y">The second <see cref="IRowViewModelBase{Thing}"/> to compare</param>
        /// <returns>
        /// Less than zero : x is "lower" than y 
        /// Zero: x "equals" y. 
        /// Greater than zero: x is "greater" than y.
        /// </returns>
        public int Compare(IRowViewModelBase<Thing> x, IRowViewModelBase<Thing> y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentNullException();
            }

            var xType = x.GetType();
            var yType = y.GetType();

            if (!PermissibleRowTypes.Any(type => type.IsAssignableFrom(xType)) )
            {
                throw new ArgumentException(string.Format("argument x is of type {0} which is not supported", xType.Name));
            }

            if (!PermissibleRowTypes.Any(type => type.IsAssignableFrom(yType)))
            {
                throw new ArgumentException(string.Format("argument y is of type {0} which is not supported", yType.Name));
            }
            
            return BreadCrumbComparer.Compare((IBreadCrumb)x, (IBreadCrumb)y);
        }
    }
}
