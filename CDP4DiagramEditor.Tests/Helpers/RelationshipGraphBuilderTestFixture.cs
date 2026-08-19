// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelationshipGraphBuilderTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4DiagramEditor.Tests.Helpers
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;

    using CDP4DiagramEditor.Helpers;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="RelationshipGraphBuilder"/> class
    /// </summary>
    [TestFixture]
    public class RelationshipGraphBuilderTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Iteration iteration;

        private Category traceCategory;
        private Category otherCategory;

        private RequirementsSpecification spec1;
        private RequirementsSpecification spec2;
        private RequirementsSpecification spec3;
        private RequirementsSpecification spec4;
        private ElementDefinition elementDefinition;

        [SetUp]
        public void Setup()
        {
            var messageBus = new CDPMessageBus();
            var assembler = new Assembler(this.uri, messageBus);
            this.cache = assembler.Cache;

            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);

            this.traceCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "trace" };
            this.otherCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "other" };

            this.spec1 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "spec1" };
            this.spec2 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "spec2" };
            this.spec3 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "spec3" };
            this.spec4 = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "spec4" };
            this.elementDefinition = new ElementDefinition(Guid.NewGuid(), this.cache, this.uri) { ShortName = "ed" };

            this.iteration.RequirementsSpecification.Add(this.spec1);
            this.iteration.RequirementsSpecification.Add(this.spec2);
            this.iteration.RequirementsSpecification.Add(this.spec3);
            this.iteration.RequirementsSpecification.Add(this.spec4);
            this.iteration.Element.Add(this.elementDefinition);
        }

        /// <summary>
        /// Adds a <see cref="BinaryRelationship"/> to the <see cref="Iteration"/> under test
        /// </summary>
        /// <param name="source">The source <see cref="Thing"/></param>
        /// <param name="target">The target <see cref="Thing"/></param>
        /// <param name="categories">The <see cref="Category"/>s of the relationship</param>
        /// <returns>The created <see cref="BinaryRelationship"/></returns>
        private BinaryRelationship AddBinaryRelationship(Thing source, Thing target, params Category[] categories)
        {
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.cache, this.uri)
            {
                Source = source,
                Target = target
            };

            relationship.Category.AddRange(categories);
            this.iteration.Relationship.Add(relationship);

            return relationship;
        }

        /// <summary>
        /// Adds a <see cref="MultiRelationship"/> to the <see cref="Iteration"/> under test
        /// </summary>
        /// <param name="relatedThings">The related <see cref="Thing"/>s</param>
        /// <returns>The created <see cref="MultiRelationship"/></returns>
        private MultiRelationship AddMultiRelationship(params Thing[] relatedThings)
        {
            var relationship = new MultiRelationship(Guid.NewGuid(), this.cache, this.uri);

            relationship.RelatedThing.AddRange(relatedThings);
            this.iteration.Relationship.Add(relationship);

            return relationship;
        }

        [Test]
        public void VerifyThatDownwardDepthIsHonoured()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);
            this.AddBinaryRelationship(this.spec3, this.spec4, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 2, DepthUp = 0 };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));
            Assert.That(graph.Edges.Count, Is.EqualTo(2));
            Assert.That(graph.Nodes.Single(x => x.Thing == this.spec1).Level, Is.EqualTo(0));
            Assert.That(graph.Nodes.Single(x => x.Thing == this.spec2).Level, Is.EqualTo(1));
            Assert.That(graph.Nodes.Single(x => x.Thing == this.spec3).Level, Is.EqualTo(2));
            Assert.That(graph.MaxNodeCountReached, Is.False);
        }

        [Test]
        public void VerifyThatUpwardDepthIsHonoured()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 0, DepthUp = 1 };

            var graph = builder.Build(new[] { this.spec3 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec2, this.spec3 }));
            Assert.That(graph.Nodes.Single(x => x.Thing == this.spec2).Level, Is.EqualTo(-1));
        }

        [Test]
        public void VerifyThatUpwardAndDownwardTraversalAreIndependent()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);
            this.AddBinaryRelationship(this.spec3, this.spec4, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 1, DepthUp = 1 };

            var graph = builder.Build(new[] { this.spec2 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));
            Assert.That(graph.Nodes.Single(x => x.Thing == this.spec1).Level, Is.EqualTo(-1));
            Assert.That(graph.Nodes.Single(x => x.Thing == this.spec3).Level, Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatBinaryRelationshipEdgeKeepsItsDirectionWhenTraversedUpward()
        {
            var relationship = this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 0, DepthUp = 1 };

            var graph = builder.Build(new[] { this.spec2 }, configuration);

            var edge = graph.Edges.Single();

            Assert.That(edge.Relationship, Is.EqualTo(relationship));
            Assert.That(edge.Source, Is.EqualTo(this.spec1));
            Assert.That(edge.Target, Is.EqualTo(this.spec2));
        }

        [Test]
        public void VerifyThatTheDefaultLevelFilterIsApplied()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec1, this.spec3, this.otherCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);

            var configuration = new RelationshipGraphConfiguration { DepthDown = 1, DepthUp = 0 };
            configuration.DefaultLevelFilter = new RelationshipGraphFilter();
            configuration.DefaultLevelFilter.Categories.Add(this.traceCategory);

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2 }));
        }

        [Test]
        public void VerifyThatALevelFilterOverrideIsApplied()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);

            var configuration = new RelationshipGraphConfiguration { DepthDown = 2, DepthUp = 0 };

            var levelTwoFilter = new RelationshipGraphFilter();
            levelTwoFilter.Categories.Add(this.otherCategory);
            configuration.LevelFilterOverrides.Add(2, levelTwoFilter);

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2 }));
        }

        [Test]
        public void VerifyThatALevelFilterMatchesASubCategory()
        {
            var subCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = "subtrace" };
            subCategory.SuperCategory.Add(this.traceCategory);

            this.AddBinaryRelationship(this.spec1, this.spec2, subCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);

            var configuration = new RelationshipGraphConfiguration { DepthDown = 1, DepthUp = 0 };
            configuration.DefaultLevelFilter = new RelationshipGraphFilter();
            configuration.DefaultLevelFilter.Categories.Add(this.traceCategory);

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2 }));
        }

        [Test]
        public void VerifyThatAMultiRelationshipExpandsToAllRelatedThingsAndCostsOneLevel()
        {
            this.AddMultiRelationship(this.spec1, this.spec2, this.spec3);
            this.AddBinaryRelationship(this.spec3, this.spec4, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 1, DepthUp = 0 };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));
            Assert.That(graph.Nodes.Single(x => x.Thing == this.spec2).Level, Is.EqualTo(1));
            Assert.That(graph.Edges.Count, Is.EqualTo(2));
        }

        [Test]
        public void VerifyThatAMultiRelationshipEdgeIsNotDuplicatedWhenBothEndpointsExpand()
        {
            this.AddMultiRelationship(this.spec1, this.spec2);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 3, DepthUp = 0 };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            // expanding spec2 finds the same relationship back to spec1; that must not add a second edge
            Assert.That(graph.Edges.Count, Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatAMultiRelationshipIsExpandedInBothDirections()
        {
            this.AddMultiRelationship(this.spec1, this.spec2);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 0, DepthUp = 1 };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2 }));
            Assert.That(graph.Nodes.Single(x => x.Thing == this.spec2).Level, Is.EqualTo(-1));
        }

        [Test]
        public void VerifyThatACycleTerminates()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);
            this.AddBinaryRelationship(this.spec3, this.spec1, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 100, DepthUp = 100 };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));
            Assert.That(graph.Edges.Count, Is.EqualTo(3));
            Assert.That(graph.MaxNodeCountReached, Is.False);
        }

        [Test]
        public void VerifyThatTheMaxNodeCountGuardTrips()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);
            this.AddBinaryRelationship(this.spec3, this.spec4, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 10, DepthUp = 0, MaxNodes = 2 };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.MaxNodeCountReached, Is.True);
            Assert.That(graph.Nodes.Count, Is.EqualTo(2));
        }

        [Test]
        public void VerifyThatALevelDirectionOverrideFollowsArrowsBackwards()
        {
            // requirement decomposition points downward, but the equipment link points up: spec4 -> spec3
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);
            this.AddBinaryRelationship(this.spec4, this.spec3, this.otherCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 3, DepthUp = 0 };

            // without the override the third hop finds nothing
            var graph = builder.Build(new[] { this.spec1 }, configuration);
            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));

            var levelThreeFilter = new RelationshipGraphFilter { Direction = RelationshipTraversalDirection.AgainstArrows };
            configuration.LevelFilterOverrides.Add(3, levelThreeFilter);

            graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3, this.spec4 }));
            Assert.That(graph.Nodes.Single(x => x.Thing == this.spec4).Level, Is.EqualTo(3));

            // the rendered edge keeps the true arrow of the model
            var equipmentEdge = graph.Edges.Single(x => x.Source == this.spec4);
            Assert.That(equipmentEdge.Target, Is.EqualTo(this.spec3));
        }

        [Test]
        public void VerifyThatTheBothDirectionFollowsArrowsBothWays()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec3, this.spec1, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);

            var configuration = new RelationshipGraphConfiguration { DepthDown = 1, DepthUp = 0 };
            configuration.DefaultLevelFilter = new RelationshipGraphFilter { Direction = RelationshipTraversalDirection.Both };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2, this.spec3 }));
        }

        [Test]
        public void VerifyThatExplicitlyExcludedThingsArePrunedWithTheirSubtree()
        {
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 5, DepthUp = 0 };
            configuration.ExcludedThings.Add(this.spec2.Iid);

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1 }));
            Assert.That(graph.Edges, Is.Empty);

            // an excluded root is not added either
            graph = builder.Build(new[] { this.spec2 }, configuration);
            Assert.That(graph.Nodes, Is.Empty);
        }

        [Test]
        public void VerifyThatADeprecatedRootIsExcluded()
        {
            this.spec1.IsDeprecated = true;

            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 5, DepthUp = 0 };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes, Is.Empty);
            Assert.That(graph.Edges, Is.Empty);
        }

        [Test]
        public void VerifyThatTraversalStopsAtTheLevelWhereTheNodeCapTrips()
        {
            // a chain that is far deeper than the cap allows
            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);
            this.AddBinaryRelationship(this.spec3, this.spec4, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 20, DepthUp = 0, MaxNodes = 2 };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.MaxNodeCountReached, Is.True);
            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1, this.spec2 }));

            // nothing beyond the capped level was reached, so no node carries a deeper level
            Assert.That(graph.Nodes.Max(x => x.Level), Is.EqualTo(1));
        }

        [Test]
        public void VerifyThatDeprecatedThingsAreExcluded()
        {
            this.spec2.IsDeprecated = true;

            this.AddBinaryRelationship(this.spec1, this.spec2, this.traceCategory);
            this.AddBinaryRelationship(this.spec2, this.spec3, this.traceCategory);

            var builder = new RelationshipGraphBuilder(this.iteration);
            var configuration = new RelationshipGraphConfiguration { DepthDown = 5, DepthUp = 0 };

            var graph = builder.Build(new[] { this.spec1 }, configuration);

            Assert.That(graph.Nodes.Select(x => x.Thing), Is.EquivalentTo(new Thing[] { this.spec1 }));
            Assert.That(graph.Edges, Is.Empty);
        }

        [Test]
        public void VerifyThatBuildThrowsOnNullArguments()
        {
            var builder = new RelationshipGraphBuilder(this.iteration);

            Assert.Throws<ArgumentNullException>(() => new RelationshipGraphBuilder(null));
            Assert.Throws<ArgumentNullException>(() => builder.Build(null, new RelationshipGraphConfiguration()));
            Assert.Throws<ArgumentNullException>(() => builder.Build(new[] { this.spec1 }, null));
        }
    }
}
