// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ToggleDeprecatedThingEvent.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Events
{
    using CDP4Common.CommonData;

    /// <summary>
    /// The purpose of the <see cref="ToggleDeprecatedThingEvent"/> is to notify an observer
    /// whether <see cref="IDeprecatableThing"/> shall be displayed
    /// </summary>
    public class ToggleDeprecatedThingEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref=""/> class
        /// </summary>
        /// <param name="shouldShow">a value indicating whether the <see cref="IDeprecatableThing"/> should be displayed</param>
        public ToggleDeprecatedThingEvent(bool shouldShow)
        {
            this.ShouldShow = shouldShow;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IDeprecatableThing"/> should be displayed
        /// </summary>
        public bool ShouldShow { get; private set; }
    }
}