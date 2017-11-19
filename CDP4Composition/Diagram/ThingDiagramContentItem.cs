// -------------------------------------------------------------------------------------------------
// <copyright file="ThingDiagramContentItem.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using CDP4Common.CommonData;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Represents a diagram content control class that can store a <see cref="Thing"/>.
    /// </summary>
    public abstract class ThingDiagramContentItem : DiagramContentItem, IThingDiagramItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramContentItem"/> class.
        /// </summary>
        /// <param name="thing">
        /// The thing represented.
        /// </param>
        protected ThingDiagramContentItem(Thing thing)
        {
            this.Thing = thing;
            this.Content = thing;
        }

        /// <summary>
        /// Gets or sets the <see cref="IThingDiagramItem.Thing"/>.
        /// </summary>
        public Thing Thing { get; set; }
    }
}
