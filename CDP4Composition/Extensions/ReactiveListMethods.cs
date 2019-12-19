// ------------------------------------------------------------------------------------------------
// <copyright file="ReactiveListMethods.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -----------------------------------------------------------------------------------------------
namespace CDP4Composition.Extensions
{
    using System;

    using CDP4Common.CommonData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// This class contains methods for specific <see cref="ReactiveList{T}"/> related functionality 
    /// </summary>
    public static class ReactiveListMethods
    {
        /// <summary>
        /// Dispose all Disposable objects in the <see cref="ReactiveList{TReactive}"/> before clearing the list
        /// </summary>
        /// <typeparam name="TReactive">The Type of the <see cref="ReactiveList{TReactive}"/></typeparam>
        /// <param name="reactiveList">The <see cref="ReactiveList{TTReactive}"/> to perform the actions on</param>
        public static void DisposeAndClear<TReactive>(this ReactiveList<TReactive> reactiveList) where TReactive : IViewModelBase<Thing>
        {
            foreach (var item in reactiveList)
            {
                item.Dispose();
            }

            reactiveList.Clear();
        }

        /// <summary>
        /// Dispose all <see cref="IDisposable"/> objects in the <see cref="ReactiveList{T}"/> before clearing the list
        /// </summary>
        /// <typeparam name="TReactive">The Type of the <see cref="ReactiveList{TReactive}"/></typeparam>
        /// <param name="reactiveList">The <see cref="ReactiveList{TTReactive}"/> to perform the actions on</param>
        /// <param name="row">The row that needs to be removed from the <see cref="ReactiveList{T}"/></param>
        public static void DisposeAndRemove<TReactive>(this ReactiveList<TReactive> reactiveList, TReactive row) where TReactive : IViewModelBase<Thing>
        {
            reactiveList.Remove(row);
            row.Dispose();
        }
    }
}
