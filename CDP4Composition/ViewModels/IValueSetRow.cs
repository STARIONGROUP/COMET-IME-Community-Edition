// -------------------------------------------------------------------------------------------------
// <copyright file="IValueSetRow.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using CDP4Common.CommonData;

    /// <summary>
    /// The interface for rows that displays <see cref="ParameterBase"/> values
    /// </summary>
    public interface IValueSetRow
    {
        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="ParameterType"/> represented by this <see cref="IValueSetRow"/>
        /// </summary>
        ClassKind ParameterTypeClassKind { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ParameterType"/> of this <see cref="Parameter"/> is a <see cref="EnumerationParameterType"/> allowing multi-select
        /// </summary>
        bool IsMultiSelect { get; }
    }
}