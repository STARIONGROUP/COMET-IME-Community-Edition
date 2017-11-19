// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IDescribeMetaData.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
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
