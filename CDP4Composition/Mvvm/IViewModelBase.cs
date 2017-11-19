// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using CDP4Common.CommonData;
    using CDP4Dal;

    /// <summary>
    /// A Covariant interface that allows the <see cref="Thing"/> to be of a more generic type
    /// </summary>
    /// <typeparam name="T">
    /// The type parameter
    /// </typeparam>
    public interface IViewModelBase<out T> where T : Thing
    {
        /// <summary>
        /// Gets the <see cref="Thing"/> that is represented by the current <see cref="IViewModelBase{T}"/>
        /// </summary>
        T Thing { get; }

        /// <summary>
        /// Dispose of the <see cref="IDisposable"/> objects
        /// </summary>
        void Dispose();
    }
}
