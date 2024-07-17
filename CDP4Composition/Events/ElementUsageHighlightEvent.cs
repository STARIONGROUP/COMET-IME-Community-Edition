// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ElementUsageHighlightEvent.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Events
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// The purpose of the <see cref="ElementUsageHighlightEvent"/> is to notify an observer
    /// that it has to highlight the <see cref="ElementUsage"/>.
    /// </summary>
    public class ElementUsageHighlightEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementUsageHighlightEvent"/> class.
        /// </summary>
        /// <param name="element">
        /// The payload <see cref="ElementDefinition"/> to highlight <see cref="ElementUsage"/>s by.
        /// </param>
        public ElementUsageHighlightEvent(ElementDefinition element)
        {
            this.ElementDefinition = element;
        }

        /// <summary>
        /// Gets or sets the <see cref="ElementDefinition"/> by which <see cref="ElementUsage"/>s should be highlighted.
        /// </summary>
        public ElementDefinition ElementDefinition { get; set; }
    }
}