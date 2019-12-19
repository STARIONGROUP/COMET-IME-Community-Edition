// ------------------------------------------------------------------------------------------------
// <copyright file="ReactiveListExtension.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using ReactiveUI;

    /// <summary>
    /// An Extension for the ReactiveList to order the contained rows
    /// </summary>
    public static class ReactiveListExtension
    {
        /// <summary>
        /// Insert a <see cref="IRowViewModelBase{Thing}"/> into the list given a <see cref="IComparer{T}"/>
        /// </summary>
        /// <param name="list">The <see cref="ReactiveList{T}"/></param>
        /// <param name="row">The <see cref="IRowViewModelBase{Thing}"/> to add</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> used to perform the sorting</param>
        public static void SortedInsert<T>(this ReactiveList<T> list, T row,
            IComparer<T> comparer)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row), $"The {nameof(row)} may not be null");
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer), $"The {nameof(comparer)} may not be null");
            }

            // item is found using the comparer : returns the index of the item found
            // item not found : returns a negative number that is the bitwise complement 
            // of the index of the next element that is larger or count if none
            var index = list.BinarySearch(row, comparer);

            if (index < 0)
            {
                list.Insert(~index, row);
            }
            else
            {
                list.Insert(index, row);
            }
        }
    }
}