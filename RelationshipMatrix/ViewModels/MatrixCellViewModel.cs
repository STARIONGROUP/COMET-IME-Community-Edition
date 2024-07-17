// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipDirectionKind.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
        /// Gets or sets the Deprecated state.
        /// </summary>
        public bool IsDeprecated => this.CalculateDeprecated();

        /// <summary>
        /// Calculate if this cell should show deprecated
        /// </summary>
        /// <returns>true if deprecated, otherwise false</returns>
        private bool CalculateDeprecated()
        {
            var deprecatableThingX = this.SourceX as IDeprecatableThing;
            var deprecatableThingY = this.SourceY as IDeprecatableThing;

            return (deprecatableThingX?.IsDeprecated ?? false) || (deprecatableThingY?.IsDeprecated ?? false);
        }

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
