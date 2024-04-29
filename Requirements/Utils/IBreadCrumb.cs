// ------------------------------------------------------------------------------------------------
// <copyright file="IBreadCrumb.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Utils
{
    /// <summary>
    /// Definition of the <see cref="IBreadCrumb"/> interface
    /// </summary>
    public interface IBreadCrumb
    {
        /// <summary>
        /// Gets the bread crumb a.k.a path
        /// </summary>
        string BreadCrumb { get;  }
    }
}
