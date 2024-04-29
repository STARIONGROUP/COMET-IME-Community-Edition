// ------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageChildRowComparer.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.Comparers
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4ProductTree.ViewModels;

    /// <summary>
    /// The <see cref="IComparer{T}"/> used to sort the child rows of the <see cref="ElementUsageRowViewModel"/>
    /// </summary>
    public class ElementUsageChildRowComparer : IComparer<IRowViewModelBase<Thing>>
    {
        /// <summary>
        /// The Permissible Kind of child <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        private static readonly List<Type> PermissibleRowTypes = new List<Type>
        {
            typeof(ParameterRowViewModel),
            typeof(ParameterOverrideRowViewModel),
            typeof(ParameterGroupRowViewModel),
            typeof(ElementUsageRowViewModel)
        };

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

            if (!PermissibleRowTypes.Contains(xType) || !PermissibleRowTypes.Contains(yType))
            {
                throw new NotSupportedException("The list contains other types of row than the specified ones.");
            }

            var isxRowPrameterRow = typeof(ParameterOrOverrideBaseRowViewModel).IsAssignableFrom(xType);
            var isyRowParameterRow = typeof(ParameterOrOverrideBaseRowViewModel).IsAssignableFrom(yType);
            if ((isxRowPrameterRow && isyRowParameterRow) || (xType == yType))
            {
                return this.CompareSameType(x, y, yType);
            }

            if (isxRowPrameterRow)
            {
                return -1;
            }

            if (xType == typeof(ElementUsageRowViewModel))
            {
                return 1;
            }

            // x is a ParameterGroupRow
            if (isyRowParameterRow)
            {
                return 1;
            }

            // x is ParameterGroupRow, y is ElementUsageRow
            return -1;
        }

        /// <summary>
        /// Compares two <see cref="IRowViewModelBase{Thing}"/> of the same type
        /// </summary>
        /// <param name="x">The First <see cref="IRowViewModelBase{Thing}"/></param>
        /// <param name="y">The second <see cref="IRowViewModelBase{Thing}"/></param>
        /// <param name="type">The actual Type</param>
        /// <returns>
        /// Less than zero : x is "lower" than y 
        /// Zero: x "equals" y. 
        /// Greater than zero: x is "greater" than y.
        /// </returns>
        private int CompareSameType(IRowViewModelBase<Thing> x, IRowViewModelBase<Thing> y, Type type)
        {
            if (typeof(ParameterOrOverrideBaseRowViewModel).IsAssignableFrom(type))
            {
                var comparer = new ParameterBaseComparer();
                return comparer.Compare((ParameterBase)x.Thing, (ParameterBase)y.Thing);
            }

            if (type == typeof(ParameterGroupRowViewModel))
            {
                var comparer = new ParameterGroupComparer();
                return comparer.Compare((ParameterGroup)x.Thing, (ParameterGroup)y.Thing);
            }

            var usageComparer = new ElementUsageComparer();
            return usageComparer.Compare((ElementUsage)x.Thing, (ElementUsage)y.Thing);
        }
    }
}