// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorDoubleParameterTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Tests.DataCollection
{
    using System;
    using System.Linq;

    using CDP4Common.Helpers;

    using CDP4Reporting.DataCollection;

    using NUnit.Framework;

    [TestFixture]
    public class DataCollectorStateDependentRowDoubleParameterTestFixture
    {
        private DataCollectorParameterTestFixture dataCollectorParameterTestFixture;

        private class Row : DataCollectorRow
        {
            [DefinedThingShortName("type4", "TypeFour")]
            public DataCollectorStateDependentPerRowDoubleParameter<Row> parameter4 { get; set; }
        }

        private class NoActualStateRow : DataCollectorRow
        {
            [DefinedThingShortName("type1", "typeOne")]
            public DataCollectorStateDependentPerRowDoubleParameter<NoActualStateRow> parameter1 { get; set; }
        }

        private class ErrorRow : DataCollectorRow
        {
            [DefinedThingShortName("type1", "typeOne")]
            public DataCollectorStateDependentPerRowDoubleParameter<ErrorRow> parameter1 { get; set; }

            [DefinedThingShortName("type4", "TypeFour")]
            public DataCollectorStateDependentPerRowStringParameter<ErrorRow> parameter4 { get; set; }
        }

        private class CorrectRow : DataCollectorRow
        {
            [DefinedThingShortName("type1", "typeOne")]
            public DataCollectorStateDependentPerRowStringParameter<CorrectRow> parameter1 { get; set; }

            [DefinedThingShortName("type4", "TypeFour")]
            public DataCollectorStateDependentPerRowStringParameter<CorrectRow> parameter4 { get; set; }
        }

        [SetUp]
        public void SetUp()
        {
            this.dataCollectorParameterTestFixture = new DataCollectorParameterTestFixture();
            this.dataCollectorParameterTestFixture.SetUp();
        }

        [Test]
        public void VerifyThatCorrectColumnNameIsUsed()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.dataCollectorParameterTestFixture.iteration, this.dataCollectorParameterTestFixture.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.dataCollectorParameterTestFixture.option).ToList();

            var node = dataSource.CreateNodes(
                hierarchy,
                nestedElementTree).First();

            Assert.AreEqual(1, node.GetColumns<DataCollectorStateDependentPerRowDoubleParameter<Row>>().Count());
            Assert.IsTrue(node.GetTable().Columns.Contains("ParameterName"));
            Assert.IsTrue(node.GetTable().Columns.Contains("ParameterState"));
            Assert.IsTrue(node.GetTable().Columns.Contains("ParameterValue"));

            Assert.AreEqual(2, node.GetTable().Rows.Count);
        }

        [Test]
        public void VerifyThatParametersWithoutStateDependenciesWork()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.dataCollectorParameterTestFixture.iteration, this.dataCollectorParameterTestFixture.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<NoActualStateRow>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.dataCollectorParameterTestFixture.option).ToList();

            var node = dataSource.CreateNodes(
                hierarchy,
                nestedElementTree).First();

            Assert.AreEqual(1, node.GetColumns<DataCollectorStateDependentPerRowDoubleParameter<NoActualStateRow>>().Count());

            Assert.IsTrue(node.GetTable().Columns.Contains("ParameterName"));
            Assert.IsTrue(node.GetTable().Columns.Contains("ParameterState"));
            Assert.IsTrue(node.GetTable().Columns.Contains("ParameterValue"));

            Assert.AreEqual(1, node.GetTable().Rows.Count);

            Assert.AreEqual("typeOne", node.GetTable().Rows[0]["ParameterName"]);
            Assert.AreEqual(DBNull.Value, node.GetTable().Rows[0]["ParameterState"]);
            Assert.AreEqual(11D, node.GetTable().Rows[0]["ParameterValue"]);
        }

        [Test]
        public void VerifyThatMultipleParametersOfSameTypeAreAllowed()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.dataCollectorParameterTestFixture.iteration, this.dataCollectorParameterTestFixture.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<CorrectRow>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.dataCollectorParameterTestFixture.option).ToList();

            Assert.DoesNotThrow(() => dataSource.CreateNodes(hierarchy, nestedElementTree));
        }

        [Test]
        public void VerifyThatMultipleParametersOfDifferentTypesAreNotAllowed()
        {
            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.dataCollectorParameterTestFixture.iteration, this.dataCollectorParameterTestFixture.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<ErrorRow>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.dataCollectorParameterTestFixture.option).ToList();

            Assert.Throws<InvalidOperationException>(() => dataSource.CreateNodes(hierarchy, nestedElementTree));
        }
    }
}