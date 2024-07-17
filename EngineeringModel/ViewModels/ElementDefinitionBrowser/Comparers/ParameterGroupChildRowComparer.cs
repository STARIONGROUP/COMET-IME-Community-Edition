﻿// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupChildRowComparer.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Comparers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.Comparers;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4EngineeringModel.ViewModels;

    /// <summary>
    /// The <see cref="IComparer{T}"/> used to sort the child rows of the <see cref="ParameterGroupRowViewModel"/>
    /// </summary>
    public class ParameterGroupChildRowComparer : IComparer<IRowViewModelBase<Thing>>
    {
        /// <summary>
        /// The Permissible Kind of child <see cref="IRowViewModelBase{T}"/>
        /// </summary>
        private static readonly List<Type> PermissibleRowTypes = new List<Type>
        {
            typeof(ParameterOrOverrideBaseRowViewModel),
            typeof(ParameterSubscriptionRowViewModel),
            typeof(ParameterGroupRowViewModel)
        };

        /// <summary>
        /// Compares two <see cref="IRowViewModelBase{Thing}"/> of the same type
        /// </summary>
        /// <param name="x">The First <see cref="IRowViewModelBase{Thing}"/></param>
        /// <param name="y">The second <see cref="IRowViewModelBase{Thing}"/></param>
        /// <returns>
        /// Less than zero : x is "lower" than y 
        /// Zero: x "equals" y. 
        /// Greater than zero: x is "greater" than y.
        /// </returns>
        public int Compare(IRowViewModelBase<Thing> x, IRowViewModelBase<Thing> y)
        {
            var xType = x.GetType();
            var yType = y.GetType();

            if (!PermissibleRowTypes.Any(type => type.IsAssignableFrom(xType)) || !PermissibleRowTypes.Any(type => type.IsAssignableFrom(yType)))
            {
                throw new NotSupportedException("The list contains other types of row than the specified ones.");
            }

            if (xType == yType)
            {
                return this.CompareSameType(x, y, yType);
            }

            if ((typeof(ParameterOrOverrideBaseRowViewModel).IsAssignableFrom(xType) || typeof(ParameterSubscriptionRowViewModel).IsAssignableFrom(xType)) &&
                (typeof(ParameterOrOverrideBaseRowViewModel).IsAssignableFrom(yType) || typeof(ParameterSubscriptionRowViewModel).IsAssignableFrom(yType)))
            {
                return this.CompareSameType(x, y, yType);
            }

            if (typeof(ParameterOrOverrideBaseRowViewModel).IsAssignableFrom(xType) ||
                typeof(ParameterSubscriptionRowViewModel).IsAssignableFrom(xType))
            {
                return -1;
            }

            // xtype is ParameterGroupRow, y parameterOrOverrideBase
            return 1;
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
            if (typeof(ParameterOrOverrideBaseRowViewModel).IsAssignableFrom(type) || typeof(ParameterSubscriptionRowViewModel).IsAssignableFrom(type))
            {
                var comparer = new ParameterBaseComparer();
                return comparer.Compare((ParameterBase)x.Thing, (ParameterBase)y.Thing);
            }

            var groupComparer = new ParameterGroupComparer();
            return groupComparer.Compare((ParameterGroup)x.Thing, (ParameterGroup)y.Thing);
        }
    }
}