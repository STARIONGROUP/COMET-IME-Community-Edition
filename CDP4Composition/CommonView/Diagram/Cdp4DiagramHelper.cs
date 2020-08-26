// -------------------------------------------------------------------------------------------------
// <copyright file="Cdp4DiagramHelper.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Diagram
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using DevExpress.Diagram.Core;

    /// <summary>
    /// A Utility class containing the constants used in the CDP4 diagramming control
    /// </summary>
    public static class Cdp4DiagramHelper
    {
        /// <summary>
        /// The ClassKind to ShapeDescription map
        /// </summary>
        private static readonly Dictionary<ClassKind, ShapeDescription> ShapeMapper = new Dictionary<ClassKind, ShapeDescription>
        {
            { ClassKind.RequirementsSpecification, BasicShapes.Rectangle },
            { ClassKind.ElementDefinition, BasicShapes.Ellipse }
        };

        /// <summary>
        /// The default height
        /// </summary>
        public const float DefaultHeight = 50;

        /// <summary>
        /// The default Width
        /// </summary>
        public const float DefaultWidth = 100;

        /// <summary>
        /// The default resolution
        /// </summary>
        public const float DefaultResolution = 300;

        /// <summary>
        /// The default separation
        /// </summary>
        public static readonly float DefaultSeparation = 2 * DefaultWidth;

        /// <summary>
        /// Gets the <see cref="ShapeDescription"/> for a <see cref="ClassKind"/>
        /// </summary>
        /// <param name="classKind">The <see cref="ClassKind"/></param>
        /// <returns>The <see cref="ShapeDescription"/></returns>
        /// <remarks>Default is <see cref="BasicShapes.Rectangle"/></remarks>
        public static ShapeDescription GetShape(ClassKind classKind)
        {
            ShapeDescription shape;
            return ShapeMapper.TryGetValue(classKind, out shape) ? shape : BasicShapes.Rectangle;
        }
    }
}