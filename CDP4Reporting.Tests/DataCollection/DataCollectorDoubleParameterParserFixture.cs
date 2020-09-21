// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataCollectorDoubleParameterParserFixture.cs" company="RHEA System S.A.">
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

    using CDP4Common.Types;

    using CDP4Reporting.DataCollection;

    using NUnit.Framework;

    [TestFixture]
    public class DataCollectorDoubleParameterParserFixture
    {
        private DataCollectorParameterTestFixture dataCollectorParameterTestFixture;

        [SetUp]
        public void SetUp()
        {
            this.dataCollectorParameterTestFixture = new DataCollectorParameterTestFixture();
            this.dataCollectorParameterTestFixture.SetUp();
        }

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void VerifyThatValuesAreCalculatedCorrectly(string valueArrayValue, double expectedValue1, double expectedValue2)
        {
            var dataCollectorDoubleParameterParser = new DataCollectorDoubleParameterParser();
            this.dataCollectorParameterTestFixture.ed1.Parameter.First().ValueSet.First().Manual =  new ValueArray<string>(new [] { valueArrayValue });

            var culture = new CultureInfo("en-GB")
            {
                NumberFormat =
                {
                    NumberDecimalSeparator = ",", 
                    NumberGroupSeparator = "."
                }
            };

            System.Threading.Thread.CurrentThread.CurrentCulture = culture;

            Assert.AreEqual(expectedValue1, dataCollectorDoubleParameterParser.Parse(valueArrayValue, this.dataCollectorParameterTestFixture.ed1.Parameter.Skip(3).First()));

            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.NumberGroupSeparator = ",";

            Assert.AreEqual(expectedValue2, dataCollectorDoubleParameterParser.Parse(valueArrayValue, this.dataCollectorParameterTestFixture.ed1.Parameter.Skip(3).First()));
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
