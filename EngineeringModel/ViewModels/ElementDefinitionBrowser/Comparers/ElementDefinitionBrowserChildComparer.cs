// ------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionBrowserChildComparer.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;

    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;

    /// <summary>
    /// The <see cref="IComparer{T}"/> for the child-rows of the <see cref="ElementDefinitionBrowserViewModel"/>
    /// </summary>
    public class ElementDefinitionBrowserChildComparer : IComparer<IRowViewModelBase<Thing>>
    {
        /// <summary>
        /// The comparer
        /// </summary>
        private static readonly ElementDefinitionComparer comparer = new ElementDefinitionComparer();

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
            if (!(x.Thing is ElementDefinition) || !(y.Thing is ElementDefinition))
            {
                throw new InvalidOperationException("one or both of the parameters is not an Element Definition row.");
            }

            return comparer.Compare((ElementDefinition) x.Thing, (ElementDefinition) y.Thing);
        }
    }
}