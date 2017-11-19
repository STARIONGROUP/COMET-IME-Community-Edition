// -------------------------------------------------------------------------------------------------
// <copyright file="IPublishableRow.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels.PublicationBrowser
{
    /// <summary>
    /// Exposes properties to rows that can be published.
    /// </summary>
    public interface IPublishableRow
    {
        /// <summary>
        /// Gets or sets a value indicating whether the row is to be published.
        /// </summary>
        bool ToBePublished { get; set; }
    }
}
