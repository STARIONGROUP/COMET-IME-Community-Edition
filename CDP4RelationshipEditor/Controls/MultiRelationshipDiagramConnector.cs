// -------------------------------------------------------------------------------------------------
// <copyright file="MultiRelationshipDiagramConnector.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipEditor.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Diagram;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// Represents a <see cref="ThingDiagramConnector"/> for a <see cref="MultiRelationship"/>.
    /// </summary>
    public class MultiRelationshipDiagramConnector : ThingDiagramConnector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThingDiagramConnector"/> class.
        /// </summary>
        public MultiRelationshipDiagramConnector(MultiRelationship thing, NamedThingDiagramContentItem source, NamedThingDiagramContentItem target)
            : base(thing)
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
            
            this.Text = string.Join(", ", this.GetAppliedMultiRelationshipRules().Select(b => b.Name));
        }

        /// <summary>
        /// Gets the applied <see cref="MultiRelationshipRule"/>s that relate to the <see cref="MultiRelationship"/>.
        /// </summary>
        /// <remarks>
        /// TODO: This needs to be moved to the hand coded part of <see cref="MultiRelationship"/> similarly to <see cref="BinaryRelationship"/>
        /// </remarks>
        /// <returns>The list of applied <see cref="MultiRelationshipRule"/></returns>
        private IEnumerable<MultiRelationshipRule> GetAppliedMultiRelationshipRules()
        {
            var relationship = this.Thing as MultiRelationship;

            if (relationship == null || relationship.Category == null)
            {
                return new List<MultiRelationshipRule>();
            }

            var model = relationship.GetContainerOfType<EngineeringModel>();
            if (model == null)
            {
                throw new InvalidOperationException("The Engineering Model container is null.");
            }

            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var appliedRules =
                new List<MultiRelationshipRule>(
                    mrdl.Rule.OfType<MultiRelationshipRule>()
                        .Where(c => relationship.Category.Contains(c.RelationshipCategory)));
            appliedRules.AddRange(mrdl.GetRequiredRdls()
                .SelectMany(rdl => rdl.Rule.OfType<MultiRelationshipRule>())
                .Where(c => relationship.Category.Contains(c.RelationshipCategory)));

            return appliedRules;
        }

    }
}
