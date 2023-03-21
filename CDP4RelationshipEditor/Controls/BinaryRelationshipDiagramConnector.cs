// -------------------------------------------------------------------------------------------------
// <copyright file="BinaryRelationshipDiagramConnector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
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
        public BinaryRelationshipDiagramConnector(BinaryRelationship thing, DiagramConnector baseConnector) : base(thing, baseConnector)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Update the properties.
        /// </summary>
        private void UpdateProperties()
        {
            if (this.Thing is BinaryRelationship relationship)
            {
                this.Content = string.Join(", ", relationship.QueryAppliedBinaryRelationshipRules().Select(b => b.Name)).PadLeft(1).PadRight(1);
            }
        }
    }
}
