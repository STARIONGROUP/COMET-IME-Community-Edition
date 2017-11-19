// -------------------------------------------------------------------------------------------------
// <copyright file="NamedThingDiagramContentItem.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using System;
    using CDP4Common.CommonData;

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
            var namedThing = thing as INamedThing;

            if (namedThing == null)
            {
                throw new ArgumentException(string.Format("The provided Thing with IId {0} is not a NamedThing.", thing.Iid), "thing");
            }
        }
    }
}
