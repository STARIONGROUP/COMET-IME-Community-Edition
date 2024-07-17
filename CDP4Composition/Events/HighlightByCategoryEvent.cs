// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HighlightByCategoryEvent.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Events
{
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The purpose of the <see cref="HighlightByCategoryEvent"/> is to notify an observer
    /// that it has to highlight if it is categorized by the provided <see cref="CDP4Common.SiteDirectoryData.Category"/>.
    /// </summary>
    public class HighlightByCategoryEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightByCategoryEvent"/> class.
        /// </summary>
        /// <param name="category">
        /// The payload <see cref="Category"/> to highlight <see cref="Thing"/>s by.
        /// </param>
        public HighlightByCategoryEvent(Category category)
        {
            this.Category = category;
        }
        
        /// <summary>
        /// Gets or sets the <see cref="Category"/> by which <see cref="Thing"/>s should be highlighted.
        /// </summary>
        public Category Category { get; set; }
    }
}