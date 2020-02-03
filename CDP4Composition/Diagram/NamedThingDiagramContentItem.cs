// -------------------------------------------------------------------------------------------------
// <copyright file="NamedThingDiagramContentItem.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using System;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Represents a <see cref="ThingDiagramContentItem"/> with a name.
    /// </summary>
    public class NamedThingDiagramContentItem : ThingDiagramContentItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing.
        /// </param>
        public NamedThingDiagramContentItem(Thing thing) 
            : base(thing)
        {
            if (thing is INamedThing namedThing)
            {
                this.FullName = namedThing.Name;
            }

            if (thing is IShortNamedThing shortNamedThing)
            {
                this.ShortName = shortNamedThing.ShortName;
            }

            // special cases
            if (thing is ParameterBase parameterBaseThing)
            {
                this.FullName = parameterBaseThing.UserFriendlyName;
                this.ShortName = parameterBaseThing.UserFriendlyShortName;
            }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the shortname of the <see cref="NamedThingDiagramContentItem"/>
        /// </summary>
        public string ShortName { get; set; } = string.Empty;
    }
}
