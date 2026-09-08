// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVActivityQueryTestFixture.cs" company="Starion Group S.A.">
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

namespace CDP4Requirements.Tests.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using CDP4Requirements.Rdl;
    using CDP4Requirements.Services;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="VandVActivityQuery"/> class.
    /// </summary>
    [TestFixture]
    public class VandVActivityQueryTestFixture
    {
        private readonly Uri uri = new Uri("http://test.com");

        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private EngineeringModel model;
        private Iteration iteration;
        private RequirementsSpecification vandVSpecification;
        private Category activityCategory;
        private Category performedByCategory;
        private Category vnvItemCategory;
        private Category vnvReportCategory;

        [SetUp]
        public void SetUp()
        {
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.model = new EngineeringModel(Guid.NewGuid(), this.cache, this.uri);
            this.iteration = new Iteration(Guid.NewGuid(), this.cache, this.uri);
            this.model.Iteration.Add(this.iteration);

            this.vandVSpecification = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "VNV", Name = "V&V" };
            this.iteration.RequirementsSpecification.Add(this.vandVSpecification);

            this.activityCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = VandVCategory.VnVActivity, Name = "VnV Activity" };
            this.performedByCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = VandVCategory.PerformedBy, Name = "performed by" };
            this.vnvItemCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = VandVCategory.VnVItem, Name = "VnV Item" };
            this.vnvReportCategory = new Category(Guid.NewGuid(), this.cache, this.uri) { ShortName = VandVCategory.VnVReport, Name = "VnV Report" };
        }

        [Test]
        public void VerifyThatIsActivityRecognisesTheCategory()
        {
            var activity = this.AddActivity("ACT_1");
            var requirement = new Requirement(Guid.NewGuid(), this.cache, this.uri) { ShortName = "REQ-1" };

            Assert.Multiple(() =>
            {
                Assert.That(VandVActivityQuery.IsActivity(activity), Is.True);
                Assert.That(VandVActivityQuery.IsActivity(requirement), Is.False);
            });
        }

        [Test]
        public void VerifyThatQueryActivitiesReturnsNonDeprecatedActivitiesOrdered()
        {
            var second = this.AddActivity("ACT_2");
            var first = this.AddActivity("ACT_1");
            var deprecated = this.AddActivity("ACT_0");
            deprecated.IsDeprecated = true;

            var activities = VandVActivityQuery.QueryActivities(this.iteration);

            Assert.That(activities, Is.EqualTo(new[] { first, second }));
        }

        [Test]
        public void VerifyThatQueryActivityFollowsThePerformedByLink()
        {
            var activity = this.AddActivity("ACT_1");
            var item = this.AddVnVItem("VNV-1");
            var standalone = this.AddVnVItem("VNV-2");

            this.AddPerformedByRelationship(item, activity);

            Assert.Multiple(() =>
            {
                Assert.That(VandVActivityQuery.QueryActivity(this.iteration, item), Is.EqualTo(activity));
                Assert.That(VandVActivityQuery.QueryActivity(this.iteration, standalone), Is.Null);
                Assert.That(VandVActivityQuery.QueryPerformedItems(this.iteration, activity), Is.EqualTo(new[] { item }));
            });
        }

        [Test]
        public void VerifyThatEffectiveAttributeFallsBackToTheActivity()
        {
            var activity = this.AddActivity("ACT_1");
            this.SetAttribute(activity, VandVParameter.Stage, "FAT");
            this.SetAttribute(activity, VandVParameter.Method, "Analysis");

            var item = this.AddVnVItem("VNV-1");
            this.SetAttribute(item, VandVParameter.Method, "Test");
            this.AddPerformedByRelationship(item, activity);

            Assert.Multiple(() =>
            {
                Assert.That(VandVActivityQuery.EffectiveAttribute(item, VandVParameter.Method), Is.EqualTo("Test"), "the item's own value wins");
                Assert.That(VandVActivityQuery.EffectiveAttribute(item, VandVParameter.Stage), Is.EqualTo("FAT"), "an empty field inherits from the activity");
                Assert.That(VandVActivityQuery.EffectiveAttribute(item, VandVParameter.Facility), Is.Null, "absent on both stays absent");
            });
        }

        [Test]
        public void VerifyThatTheActivityMapsResolveEveryLinkInOnePass()
        {
            var activity = this.AddActivity("ACT_42");
            var first = this.AddVnVItem("VNV-1");
            var second = this.AddVnVItem("VNV-2");
            var standalone = this.AddVnVItem("VNV-3");

            this.AddPerformedByRelationship(first, activity);
            this.AddPerformedByRelationship(second, activity);

            var activityByItem = VandVActivityQuery.QueryActivityMap(this.iteration);

            Assert.Multiple(() =>
            {
                Assert.That(activityByItem[first.Iid], Is.EqualTo(activity));
                Assert.That(activityByItem[second.Iid], Is.EqualTo(activity));
                Assert.That(activityByItem.ContainsKey(standalone.Iid), Is.False, "an item performed by nothing is absent");
            });

            var itemsByActivity = VandVActivityQuery.QueryPerformedItemsMap(this.iteration);

            Assert.That(itemsByActivity[activity.Iid], Is.EqualTo(new[] { first, second }), "the inverse map is ordered by short-name");
        }

        [Test]
        public void VerifyThatQueryReportsReturnsTheReportSpecifications()
        {
            var report = new RequirementsSpecification(Guid.NewGuid(), this.cache, this.uri) { ShortName = "FAT_REPORT", Name = "FAT Report" };
            report.Category.Add(this.vnvReportCategory);
            this.iteration.RequirementsSpecification.Add(report);

            Assert.That(VandVActivityQuery.QueryReports(this.iteration), Is.EqualTo(new[] { report }), "the plain and V&V specifications are not reports");

            var activity = new Requirement(Guid.NewGuid(), this.cache, this.uri) { ShortName = "ACT_1" };
            activity.Category.Add(this.activityCategory);
            report.Requirement.Add(activity);

            Assert.Multiple(() =>
            {
                Assert.That(VandVActivityQuery.QueryReport(activity), Is.EqualTo(report), "an activity's report is its containing specification");
                Assert.That(VandVActivityQuery.QueryReport(this.AddActivity("ACT_2")), Is.Null, "an activity in the V&V specification belongs to no report");
            });
        }

        [Test]
        public void VerifyThatQuerySuggestionsReturnsTheDistinctValuesInUse()
        {
            var first = this.AddVnVItem("VNV-1");
            this.SetAttribute(first, VandVParameter.EvidenceReference, "TR-101");

            var second = this.AddVnVItem("VNV-2");
            this.SetAttribute(second, VandVParameter.EvidenceReference, "tr-101");

            var third = this.AddVnVItem("VNV-3");
            this.SetAttribute(third, VandVParameter.EvidenceReference, "TR-100");

            var suggestions = VandVActivityQuery.QuerySuggestions(this.iteration, VandVParameter.EvidenceReference);

            Assert.That(suggestions, Is.EqualTo(new[] { "TR-100", "TR-101" }), "case-insensitively distinct and ordered");
        }

        [Test]
        public void VerifyThatTheRollUpDerivesTheStatusFromTheActivity()
        {
            var activity = this.AddActivity("ACT_1");
            this.SetAttribute(activity, VandVParameter.Status, "Passed");

            var item = this.AddVnVItem("VNV-1");
            this.AddPerformedByRelationship(item, activity);

            var rollUp = VandVCoverageQuery.RollUp(new[] { item });

            Assert.Multiple(() =>
            {
                Assert.That(rollUp.Passed, Is.EqualTo(1), "the activity passing moves the item it performs");
                Assert.That(rollUp.Open, Is.EqualTo(0));
            });
        }

        private Requirement AddActivity(string shortName)
        {
            var activity = new Requirement(Guid.NewGuid(), this.cache, this.uri) { ShortName = shortName, Name = shortName };
            activity.Category.Add(this.activityCategory);
            this.vandVSpecification.Requirement.Add(activity);

            return activity;
        }

        private Requirement AddVnVItem(string shortName)
        {
            var item = new Requirement(Guid.NewGuid(), this.cache, this.uri) { ShortName = shortName, Name = shortName };
            item.Category.Add(this.vnvItemCategory);
            this.vandVSpecification.Requirement.Add(item);

            return item;
        }

        private void AddPerformedByRelationship(Requirement item, Requirement activity)
        {
            var relationship = new BinaryRelationship(Guid.NewGuid(), this.cache, this.uri) { Source = item, Target = activity };
            relationship.Category.Add(this.performedByCategory);
            this.iteration.Relationship.Add(relationship);
        }

        private void SetAttribute(Requirement requirement, string parameterTypeShortName, string value)
        {
            requirement.SetVandVAttribute(parameterTypeShortName, value, this.cache, this.uri);
        }
    }
}
