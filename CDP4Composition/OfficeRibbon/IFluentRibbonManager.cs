// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFluentRibbonManager.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using CDP4Composition.OfficeRibbon;

    /// <summary>
    /// Interface that defines the methods to populate the Fluent Office Ribbon and interact with the <see cref="RibbonPart"/>s
    /// </summary>
    public interface IFluentRibbonManager : IFluentRibbonCallback
    {
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IFluentRibbonManager"/> is active or not
        /// </summary>
        /// <remarks>
        /// The <see cref="RibbonPart"/>s shall only be active if the orchestrating <see cref="IFluentRibbonManager"/> is active.
        /// </remarks>
        bool IsActive { get; set; }

        /// <summary>
        /// Registers the <see cref="RibbonPart"/> that contains fluent Ribbon XML with the <see cref="FluentRibbonManager"/>
        /// </summary>
        /// <param name="ribbonPart">
        /// The <see cref="RibbonPart"/> that is to be registered
        /// </param>
        void RegisterRibbonPart(RibbonPart ribbonPart);

        /// <summary>
        /// Gets the Fluent Ribbon XML that is used to customize the Office Ribbon
        /// </summary>
        /// <returns>
        /// a string of valid Fluent Ribbon XML
        /// </returns>
        string GetFluentXml();
    }
}
