// -------------------------------------------------------------------------------------------------
// <copyright file="OrderSettings.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements
{
    using System;

    /// <summary>
    /// A setting class for the requirement module
    /// </summary>
    public class OrderSettings
    {
        /// <summary>
        /// Gets or sets the identifier of the <see cref="ParameterType"/> to use to order a requirement
        /// </summary>
        public Guid ParameterType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the values associated to the order parameter-type can be edited
        /// </summary>
        public bool IsEditable { get; set; }
    }
}
