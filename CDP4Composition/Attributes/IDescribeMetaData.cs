// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDescribeMetaData.cs" company="Starion Group S.A.">
//   Copyright (c) 2017 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Attributes
{
    using System.ComponentModel.Composition;

    /// <summary>
    /// The purpose of the <see cref="IDescribeMetaData"/> interface is to define a name and a description to be
    ///   used in conjunction with a custom <see cref="ExportAttribute"/>.
    /// </summary>
    public interface IDescribeMetaData
    {
        /// <summary>
        /// Gets a human readable name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets a human readable description.
        /// </summary>
        string Description { get; }
    }
}
