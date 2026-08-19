// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipGraphBuilder.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Walks the <see cref="Relationship"/>s of an <see cref="Iteration"/> outward from a set of roots and returns the
    /// reached <see cref="Thing"/>s as an ephemeral <see cref="RelationshipGraph"/>. This class is free of any
    /// dependency on the UI and it never mutates the model.
    /// </summary>
    public class RelationshipGraphBuilder
    {
        /// <summary>
        /// The <see cref="BinaryRelationship"/>s of the <see cref="Iteration"/>, indexed by the <see cref="Thing.Iid"/>
        /// of their <see cref="BinaryRelationship.Source"/>
        /// </summary>
        private readonly ILookup<Guid, BinaryRelationship> binaryRelationshipsBySource;

        /// <summary>
        /// The <see cref="BinaryRelationship"/>s of the <see cref="Iteration"/>, indexed by the <see cref="Thing.Iid"/>
        /// of their <see cref="BinaryRelationship.Target"/>
        /// </summary>
        private readonly ILookup<Guid, BinaryRelationship> binaryRelationshipsByTarget;

        /// <summary>
        /// The <see cref="MultiRelationship"/>s of the <see cref="Iteration"/>, indexed by the <see cref="Thing.Iid"/>
        /// of each of their <see cref="MultiRelationship.RelatedThing"/>
        /// </summary>
        private readonly ILookup<Guid, MultiRelationship> multiRelationshipsByRelatedThing;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipGraphBuilder"/> class
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> whose <see cref="Iteration.Relationship"/>s are traversed. The relationships are
        /// indexed once, so a builder is to be recreated when they change.
        /// </param>
        public RelationshipGraphBuilder(Iteration iteration)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            var binaryRelationships = iteration.Relationship
                .OfType<BinaryRelationship>()
                .Where(x => x.Source != null && x.Target != null)
                .ToList();

            this.binaryRelationshipsBySource = binaryRelationships.ToLookup(x => x.Source.Iid);
            this.binaryRelationshipsByTarget = binaryRelationships.ToLookup(x => x.Target.Iid);

            this.multiRelationshipsByRelatedThing = iteration.Relationship
                .OfType<MultiRelationship>()
                .SelectMany(
                    multiRelationship => multiRelationship.RelatedThing
                        .Where(relatedThing => relatedThing != null)
                        .Select(relatedThing => new { relatedThing.Iid, MultiRelationship = multiRelationship }))
                .ToLookup(x => x.Iid, x => x.MultiRelationship);
        }

        /// <summary>
        /// Builds the <see cref="RelationshipGraph"/> that is reachable from the supplied roots
        /// </summary>
        /// <param name="roots">
        /// The <see cref="Thing"/>s the traversal starts from. They are part of the resulting graph unless they are
        /// excluded or deprecated.
        /// </param>
        /// <param name="configuration">
        /// The <see cref="RelationshipGraphConfiguration"/> that constrains the traversal
        /// </param>
        /// <returns>
        /// The resulting <see cref="RelationshipGraph"/>
        /// </returns>
        public RelationshipGraph Build(IEnumerable<Thing> roots, RelationshipGraphConfiguration configuration)
        {
            if (roots == null)
            {
                throw new ArgumentNullException(nameof(roots));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var nodes = new Dictionary<Guid, RelationshipGraphNode>();
            var edges = new List<RelationshipGraphEdge>();
            var edgeKeys = new HashSet<Tuple<Guid, Guid, Guid>>();
            var rootThings = new List<Thing>();
            var maxNodeCountReached = false;

            foreach (var root in roots.Where(x => x != null))
            {
                // a root is not subject to the node filter, but it is subject to exclusion and deprecation, so that a
                // deprecated thing never shows up as a root while every deprecated thing reached by traversal is hidden
                if (nodes.ContainsKey(root.Iid) || configuration.ExcludedThings.Contains(root.Iid) || IsDeprecated(root))
                {
                    continue;
                }

                if (nodes.Count >= configuration.MaxNodes)
                {
                    maxNodeCountReached = true;
                    break;
                }

                nodes.Add(root.Iid, new RelationshipGraphNode(root, 0));
                rootThings.Add(root);
            }

            if (this.Expand(rootThings, configuration, 1, nodes, edges, edgeKeys))
            {
                maxNodeCountReached = true;
            }

            if (this.Expand(rootThings, configuration, -1, nodes, edges, edgeKeys))
            {
                maxNodeCountReached = true;
            }

            return new RelationshipGraph(nodes.Values.ToList(), edges, maxNodeCountReached);
        }

        /// <summary>
        /// Performs a breadth-first expansion in a single direction, adding the reached <see cref="Thing"/>s and the
        /// traversed <see cref="Relationship"/>s to the graph that is under construction
        /// </summary>
        /// <param name="roots">
        /// The <see cref="Thing"/>s the expansion starts from
        /// </param>
        /// <param name="configuration">
        /// The <see cref="RelationshipGraphConfiguration"/> that constrains the traversal
        /// </param>
        /// <param name="direction">
        /// 1 to expand downward, -1 to expand upward
        /// </param>
        /// <param name="nodes">
        /// The nodes of the graph under construction, keyed by <see cref="Thing.Iid"/>
        /// </param>
        /// <param name="edges">
        /// The edges of the graph under construction
        /// </param>
        /// <param name="edgeKeys">
        /// The keys of the edges that were already added, used to keep the downward and upward expansion from adding
        /// the same edge twice
        /// </param>
        /// <returns>
        /// true when the expansion was cut short by <see cref="RelationshipGraphConfiguration.MaxNodes"/>
        /// </returns>
        private bool Expand(IReadOnlyList<Thing> roots, RelationshipGraphConfiguration configuration, int direction, IDictionary<Guid, RelationshipGraphNode> nodes, ICollection<RelationshipGraphEdge> edges, ISet<Tuple<Guid, Guid, Guid>> edgeKeys)
        {
            var depth = direction > 0 ? configuration.DepthDown : configuration.DepthUp;

            if (depth <= 0)
            {
                return false;
            }

            var maxNodeCountReached = false;

            // a visited set per direction, so that a thing that was reached downward can still be reached upward
            var visited = new HashSet<Guid>(roots.Select(x => x.Iid));
            var currentLevelThings = roots.ToList();

            for (var level = 1; level <= depth && currentLevelThings.Any(); level++)
            {
                var levelFilter = configuration.QueryLevelFilter(level);
                var traversalDirection = levelFilter?.Direction ?? (direction > 0 ? RelationshipTraversalDirection.AlongArrows : RelationshipTraversalDirection.AgainstArrows);
                var nextLevelThings = new List<Thing>();

                foreach (var thing in currentLevelThings)
                {
                    foreach (var traversal in this.QueryTraversals(thing, traversalDirection))
                    {
                        if (levelFilter != null && !levelFilter.IsMatch(traversal.Relationship))
                        {
                            continue;
                        }

                        if (!IsNodeAllowed(traversal.RelatedThing, configuration))
                        {
                            continue;
                        }

                        if (!nodes.ContainsKey(traversal.RelatedThing.Iid))
                        {
                            if (nodes.Count >= configuration.MaxNodes)
                            {
                                // keep scanning: edges between things that are already on the diagram are still added
                                maxNodeCountReached = true;
                                continue;
                            }

                            nodes.Add(traversal.RelatedThing.Iid, new RelationshipGraphNode(traversal.RelatedThing, direction * level));
                        }

                        if (edgeKeys.Add(BuildEdgeKey(traversal.Relationship, traversal.EdgeSource, traversal.EdgeTarget)))
                        {
                            edges.Add(new RelationshipGraphEdge(traversal.EdgeSource, traversal.EdgeTarget, traversal.Relationship));
                        }

                        if (visited.Add(traversal.RelatedThing.Iid))
                        {
                            nextLevelThings.Add(traversal.RelatedThing);
                        }
                    }
                }

                currentLevelThings = nextLevelThings;

                if (maxNodeCountReached)
                {
                    // no further node can be added, so any deeper level would only discard its results
                    break;
                }
            }

            return maxNodeCountReached;
        }

        /// <summary>
        /// Builds the deduplication key of an edge. A <see cref="MultiRelationship"/> edge is keyed on its unordered
        /// endpoints, because entering the relationship through either endpoint yields the same edge and must not
        /// produce a duplicate.
        /// </summary>
        /// <param name="relationship">The traversed <see cref="Relationship"/></param>
        /// <param name="edgeSource">The <see cref="Thing"/> the edge originates from</param>
        /// <param name="edgeTarget">The <see cref="Thing"/> the edge points to</param>
        /// <returns>The key that identifies the edge</returns>
        private static Tuple<Guid, Guid, Guid> BuildEdgeKey(Relationship relationship, Thing edgeSource, Thing edgeTarget)
        {
            if (relationship is MultiRelationship && edgeTarget.Iid.CompareTo(edgeSource.Iid) < 0)
            {
                return new Tuple<Guid, Guid, Guid>(relationship.Iid, edgeTarget.Iid, edgeSource.Iid);
            }

            return new Tuple<Guid, Guid, Guid>(relationship.Iid, edgeSource.Iid, edgeTarget.Iid);
        }

        /// <summary>
        /// Queries the <see cref="Relationship"/>s that leave the supplied <see cref="Thing"/> in the supplied
        /// traversal direction
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> that is being expanded
        /// </param>
        /// <param name="traversalDirection">
        /// The <see cref="RelationshipTraversalDirection"/> in which <see cref="BinaryRelationship"/> arrows are
        /// followed at this hop
        /// </param>
        /// <returns>
        /// For each traversable <see cref="Relationship"/> the <see cref="Thing"/> that is reached through it and the
        /// two <see cref="Thing"/>s the resulting edge is to be drawn between
        /// </returns>
        /// <remarks>
        /// A <see cref="MultiRelationship"/> is undirected and is therefore expanded in both directions: entering it
        /// through any of its <see cref="MultiRelationship.RelatedThing"/>s reaches all the others, at the cost of a
        /// single level.
        /// </remarks>
        private IEnumerable<(Relationship Relationship, Thing RelatedThing, Thing EdgeSource, Thing EdgeTarget)> QueryTraversals(Thing thing, RelationshipTraversalDirection traversalDirection)
        {
            if (traversalDirection != RelationshipTraversalDirection.AgainstArrows)
            {
                foreach (var binaryRelationship in this.binaryRelationshipsBySource[thing.Iid])
                {
                    yield return (binaryRelationship, binaryRelationship.Target, binaryRelationship.Source, binaryRelationship.Target);
                }
            }

            if (traversalDirection != RelationshipTraversalDirection.AlongArrows)
            {
                foreach (var binaryRelationship in this.binaryRelationshipsByTarget[thing.Iid])
                {
                    yield return (binaryRelationship, binaryRelationship.Source, binaryRelationship.Source, binaryRelationship.Target);
                }
            }

            foreach (var multiRelationship in this.multiRelationshipsByRelatedThing[thing.Iid])
            {
                foreach (var relatedThing in multiRelationship.RelatedThing)
                {
                    if (relatedThing == null || relatedThing.Iid == thing.Iid)
                    {
                        continue;
                    }

                    yield return (multiRelationship, relatedThing, thing, relatedThing);
                }
            }
        }

        /// <summary>
        /// Asserts whether a reached <see cref="Thing"/> may be added to the graph
        /// </summary>
        /// <param name="thing">
        /// The reached <see cref="Thing"/>
        /// </param>
        /// <param name="configuration">
        /// The <see cref="RelationshipGraphConfiguration"/> holding the excluded things
        /// </param>
        /// <returns>
        /// true when the <see cref="Thing"/> may be added
        /// </returns>
        private static bool IsNodeAllowed(Thing thing, RelationshipGraphConfiguration configuration)
        {
            return !configuration.ExcludedThings.Contains(thing.Iid) && !IsDeprecated(thing);
        }

        /// <summary>
        /// Asserts whether a <see cref="Thing"/> is deprecated
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> that is to be checked
        /// </param>
        /// <returns>
        /// true when the <see cref="Thing"/> is an <see cref="IDeprecatableThing"/> that is deprecated
        /// </returns>
        private static bool IsDeprecated(Thing thing)
        {
            return thing is IDeprecatableThing deprecatableThing && deprecatableThing.IsDeprecated;
        }
    }
}
