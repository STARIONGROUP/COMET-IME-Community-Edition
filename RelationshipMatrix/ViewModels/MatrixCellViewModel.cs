// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MatrixCellViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using ReactiveUI;

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
        /// Initializes a new instance of the <see cref="MatrixCellViewModel"/> class
        /// </summary>
        /// <param name="source1">The row that represents the <see cref="Thing"/></param>
        /// <param name="source2">The column that represents the <see cref="Thing"/></param>
        /// <param name="binaryRelationship">The current set of <see cref="BinaryRelationship"/></param>
        /// <param name="rule">The current <see cref="BinaryRelationshipRule"/></param>
        public MatrixCellViewModel(Thing source1, Thing source2, IReadOnlyList<BinaryRelationship> binaryRelationship, BinaryRelationshipRule rule)
        {
            this.Source1 = source1;
            this.Source2 = source2;
            this.Rule = rule;
            this.Relationships = binaryRelationship ?? new List<BinaryRelationship>();
            if (binaryRelationship == null || binaryRelationship.Count == 0)
            {
                this.RelationshipDirection = RelationshipDirectionKind.None;
            }
            else
            {
                this.RelationshipDirection = binaryRelationship.All(x => x.Source.Iid == source1.Iid)
                    ? RelationshipDirectionKind.RowThingToColumnThing
                    : binaryRelationship.All(x => x.Source.Iid == source2.Iid)
                        ? RelationshipDirectionKind.ColumnThingToRowThing
                        : RelationshipDirectionKind.BiDirectional;
            }
        }

        /// <summary>
        /// Gets the <see cref="Thing"/> in the row context
        /// </summary>
        public Thing Source1 { get; }

        /// <summary>
        /// Gets the <see cref="Thing"/> in the column context
        /// </summary>
        public Thing Source2 { get; }

        /// <summary>
        /// Gets the <see cref="BinaryRelationshipRule"/> in the current context
        /// </summary>
        public BinaryRelationshipRule Rule { get; }

        /// <summary>
        /// Gets the <see cref="BinaryRelationship"/>
        /// </summary>
        public IReadOnlyList<BinaryRelationship> Relationships { get; }

        /// <summary>
        /// Gets the <see cref="RelationshipDirectionKind"/>
        /// </summary>
        public RelationshipDirectionKind RelationshipDirection
        {
            get { return this.relationshipDirection; }
            private set { this.RaiseAndSetIfChanged(ref this.relationshipDirection, value); }
        }
    }
}
