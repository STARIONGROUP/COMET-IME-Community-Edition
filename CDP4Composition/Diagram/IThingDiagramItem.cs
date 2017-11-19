// -------------------------------------------------------------------------------------------------
// <copyright file="IThingDiagramItem.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Diagram
{
    using CDP4Common.CommonData;

    /// <summary>
    /// Represents an interface to <see cref="DiagramItem"/> controls that also hold a <see cref="Thing"/>.
    /// </summary>
    public interface IThingDiagramItem
    {
        /// <summary>
        /// Gets or sets the <see cref="Thing"/>.
        /// </summary>
        Thing Thing { get; set; }
    }
}
