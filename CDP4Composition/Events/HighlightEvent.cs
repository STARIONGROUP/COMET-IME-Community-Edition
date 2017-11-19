// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HighlightEvent.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Events
{
    using CDP4Common.CommonData;

    /// <summary>
    /// The purpose of the <see cref="HighlightEvent"/> is to notify an observer
    /// that the referenced <see cref="Thing"/> has to be highlighted.
    /// </summary>
    public class HighlightEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightEvent"/> class.
        /// </summary>
        /// <param name="thing">
        /// The payload <see cref="Thing"/> to highlight.
        /// </param>
        public HighlightEvent(Thing thing)
        {
            this.HighlightedThing = thing;
        }
        
        /// <summary>
        /// Gets or sets the highlighted <see cref="Thing"/>
        /// </summary>
        public Thing HighlightedThing { get; set; }
    }
}