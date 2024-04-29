// -------------------------------------------------------------------------------------------------
// <copyright file="IMappingRowViewModelBase.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using ReqIFSharp;

    /// <summary>
    /// The interface for mapping rows
    /// </summary>
    /// <typeparam name="T">A type of <see cref="Identifiable"/></typeparam>
    public interface IMappingRowViewModelBase<out T> where T : Identifiable
    {
        /// <summary>
        /// Gets the <see cref="Identifiable"/> to map
        /// </summary>
        T Identifiable { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMappingRowViewModelBase{T}"/> is mapped
        /// </summary>
        bool IsMapped { get; }
    }
}