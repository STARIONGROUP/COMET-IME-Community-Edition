// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixCellViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Services;
    using ReactiveUI;
    using Settings;

    /// <summary>
    /// Assertion on the kind of relationships that exist in the current cell
    /// </summary>
    public enum RelationshipDirectionKind
    {
        /// <summary>
        /// Asserts that there is a relationship going from the <see cref="Thing"/> represented by the row to the <see cref="Thing"/> represented by the column
        /// </summary>
        RowThingToColumnThing,

        /// <summary>
        /// Asserts that there is a relationship going from the <see cref="Thing"/> represented by the column to the <see cref="Thing"/> represented by the row
        /// </summary>
        ColumnThingToRowThing,

        /// <summary>
        /// Asserts that there is a bi-directional relationship
        /// </summary>
        BiDirectional,

        /// <summary>
        /// Asserts that no relationship exist
        /// </summary>
        None
    }

    /// <summary>
    /// A view-model that contains detailed information of a <see cref="BinaryRelationship"/>
    /// </summary>
    public class MatrixCellViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="RelationshipDirection"/>
        /// </summary>
        private RelationshipDirectionKind relationshipDirection;

        /// <summary>
        /// Backing field for <see cref="Tooltip"/>
        /// </summary>
        private string tooltip;

        /// <summary>
        /// Backing field for <see cref="IsHighlighted"/>
        /// </summary>
        private bool isHighlighted;

        /// <summary>
        /// Initializes a new instance of the <see cref="MatrixCellViewModel"/> class
        /// </summary>
        /// <param name="sourceY">The row that represents the <see cref="Thing"/></param>
        /// <param name="sourceX">The column that represents the <see cref="Thing"/></param>
        /// <param name="binaryRelationship">The current set of <see cref="BinaryRelationship"/></param>
        /// <param name="rule">The current <see cref="BinaryRelationshipRule"/></param>
        /// <param name="displayKind">The <see cref="DisplayKind"/> of the item.</param>
        public MatrixCellViewModel(Thing sourceY, Thing sourceX, IReadOnlyList<BinaryRelationship> binaryRelationship, BinaryRelationshipRule rule, DisplayKind? displayKind = null)
        {
            this.SourceY = sourceY;
            this.SourceX = sourceX;
            this.Rule = rule;
            this.Relationships = binaryRelationship ?? new List<BinaryRelationship>();
            this.DisplayKind = displayKind;
            this.IsHighlighted = false;

            if (binaryRelationship == null || binaryRelationship.Count == 0)
            {
                this.RelationshipDirection = RelationshipDirectionKind.None;
            }
            else
            {
                this.RelationshipDirection = binaryRelationship.All(x => x.Source.Iid == sourceY.Iid)
                    ? RelationshipDirectionKind.RowThingToColumnThing
                    : binaryRelationship.All(x => x.Source.Iid == sourceX.Iid)
                        ? RelationshipDirectionKind.ColumnThingToRowThing
                        : RelationshipDirectionKind.BiDirectional;
            }

            this.BuildToolTip();
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> in the row context
        /// </summary>
        public Thing SourceY { get; }

        /// <summary>
        /// Gets the <see cref="Thing"/> in the column context
        /// </summary>
        public Thing SourceX { get; }

        /// <summary>
        /// Gets the <see cref="BinaryRelationshipRule"/> in the current context
        /// </summary>
        public BinaryRelationshipRule Rule { get; }

        /// <summary>
        /// Gets the <see cref="BinaryRelationship"/>
        /// </summary>
        public IReadOnlyList<BinaryRelationship> Relationships { get; }

        /// <summary>
        /// Gets the <see cref="DisplayKind"/> of the cell.
        /// </summary>
        public DisplayKind? DisplayKind { get; }

        /// <summary>
        /// Gets or sets the highlighted state.
        /// </summary>
        public bool IsHighlighted
        {
            get { return this.isHighlighted; }
            private set { this.RaiseAndSetIfChanged(ref this.isHighlighted, value); }
        }

        /// <summary>
        /// Gets or sets the tooltip of the cell.
        /// </summary>
        public string Tooltip
        {
            get { return this.tooltip; }
            private set { this.RaiseAndSetIfChanged(ref this.tooltip, value); }
        }

        /// <summary>
        /// Gets the <see cref="RelationshipDirectionKind"/>
        /// </summary>
        public RelationshipDirectionKind RelationshipDirection
        {
            get { return this.relationshipDirection; }
            private set { this.RaiseAndSetIfChanged(ref this.relationshipDirection, value); }
        }

        /// <summary>
        /// Constructs the tooltip.
        /// </summary>
        private void BuildToolTip()
        {
            if (this.SourceY == null)
            {
                // no content
                this.Tooltip = string.Empty;
                return;
            }

            if (this.SourceX == null)
            {
                // row header
                this.Tooltip = this.SourceY.Tooltip();
                return;
            }

            if (this.RelationshipDirection == RelationshipDirectionKind.None)
            {
                // no relationship
                this.Tooltip = String.Empty;
            }

            this.Tooltip = this.RelationshipDirection == RelationshipDirectionKind.RowThingToColumnThing
                ? $"{this.SourceY.UserFriendlyName} --({this.Rule.ForwardRelationshipName})--> {this.SourceX.UserFriendlyName}"
                : this.RelationshipDirection == RelationshipDirectionKind.ColumnThingToRowThing
                    ? $"{this.SourceY.UserFriendlyName} <--({this.Rule.ForwardRelationshipName})-- {this.SourceX.UserFriendlyName}"
                    : $"{this.SourceY.UserFriendlyName} <--({this.Rule.ForwardRelationshipName})--> {this.SourceX.UserFriendlyName}";
        }
    }
}
