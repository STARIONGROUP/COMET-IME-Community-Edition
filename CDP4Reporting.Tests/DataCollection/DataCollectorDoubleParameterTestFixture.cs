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
    using System.Globalization;
    using System.Linq;

    using CDP4Common.Helpers;
    using CDP4Common.Types;

    using CDP4Reporting.DataCollection;

    using NUnit.Framework;

    [TestFixture]
    public class DataCollectorDoubleParameterTestFixture
    {
        private DataCollectorParameterTestFixture dataCollectorParameterTestFixture;

        private class Row : DataCollectorRow
        {
            [DefinedThingShortName("type1", "TypeOne")]
            public DataCollectorDoubleParameter<Row> parameter1 { get; set; }
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

            Assert.AreEqual(1, node.GetColumns<DataCollectorDoubleParameter<Row>>().Count());
            Assert.IsTrue(node.GetTable().Columns.Contains("TypeOne"));
        }

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void VerifyThatValuesAreCalculatedCorrectly(string valueArrayValue, double expectedValue1, double expectedValue2)
        {
            this.dataCollectorParameterTestFixture.ed1.Parameter.First().ValueSet.First().Manual =  new ValueArray<string>(new [] { valueArrayValue });

            var hierarchy = new CategoryDecompositionHierarchy
                    .Builder(this.dataCollectorParameterTestFixture.iteration, this.dataCollectorParameterTestFixture.cat1.ShortName)
                .Build();

            var dataSource = new DataCollectorNodesCreator<Row>();
            var nestedElementTree = new NestedElementTreeGenerator().Generate(this.dataCollectorParameterTestFixture.option).ToList();

            var node = dataSource.CreateNodes(
                hierarchy,
                nestedElementTree).First();


            var culture = new CultureInfo("en-GB")
            {
                NumberFormat =
                {
                    NumberDecimalSeparator = ",", 
                    NumberGroupSeparator = "."
                }
            };

            System.Threading.Thread.CurrentThread.CurrentCulture = culture;

            Assert.AreEqual(1, node.GetColumns<DataCollectorDoubleParameter<Row>>().Count());

            Assert.AreEqual(expectedValue1, (double)node.GetTable().Rows[0]["TypeOne"]);

            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.NumberGroupSeparator = ",";

            Assert.AreEqual(expectedValue2, (double)node.GetTable().Rows[0]["TypeOne"]);
        }

        static object[] TestCases =
        {
            new object[] { "11", 11, 11},
            new object[] { "11.11", 11.11, 11.11},
            new object[] { "11,11", 11.11, 1111},
            new object[] { "-", 0, 0},
            new object[] { "", 0, 0},
            new object[] { " ", 0, 0},
            new object[] { "aa", 0, 0 }
        };
    }
}
