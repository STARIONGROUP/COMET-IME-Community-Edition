// -------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipDiagramConnector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipEditor.Controls
{
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Diagram;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Represents a <see cref="ThingDiagramConnector"/> for a <see cref="BinaryRelationship"/>.
    /// </summary>
    public class BinaryRelationshipDiagramConnector : ThingDiagramConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramConnector"/> class.
        /// </summary>
        public BinaryRelationshipDiagramConnector(BinaryRelationship thing, NamedThingDiagramContentItem source, NamedThingDiagramContentItem target, DiagramConnector baseConnector) : base(thing, baseConnector)
        {
            this.Source = source;
            this.Target = target;

            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the source of this <see cref="DiagramConnector"/>
        /// </summary>
        public NamedThingDiagramContentItem Source { get; set; }

        /// <summary>
        /// Gets or sets the target of this <see cref="DiagramConnector"/>
        /// </summary>
        public NamedThingDiagramContentItem Target { get; set; }

        /// <summary>
        /// Update the properties.
        /// </summary>
        private void UpdateProperties()
        {
            this.BeginItem = this.Source;
            this.EndItem = this.Target;

            var relationship = this.Thing as BinaryRelationship;

            if (relationship != null)
            {
                this.Text = string.Join(", ", relationship.AppliedBinaryRelationshipRules.Select(b => b.Name)).PadLeft(1).PadRight(1);
            }
        }

    }
}
