// -------------------------------------------------------------------------------------------------
// <copyright file="LineSeriesTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.ViewModels.Rows
{
    using System.Linq;

    using CDP4Dashboard.ViewModels.Charts;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="LineSeries"/> instances
    /// </summary>
    [TestFixture]
    internal class LineSeriesTestFixture
    {
        internal LineSeries lineSeries;
        private LineTestFixture lineTestFixture;

        [SetUp]
        public void Setup()
        {
            this.lineTestFixture = new LineTestFixture();
            this.lineTestFixture.Setup();

            this.lineSeries = new LineSeries
            {
                OptionName = "Option",
                ParameterName = "Parameter",
                StateName = "State",
                Lines = new[]
                {
                    this.lineTestFixture.line1,
                    this.lineTestFixture.line2
                }
            };
        }

        [Test]
        public void verify_that_export_headers_are_created_as_expected()
        {
            var header = this.lineSeries.GetExportHeaders().ToList();

            Assert.AreEqual(5, header.Count);
            CollectionAssert.Contains(header, nameof(Line.RevisionNumber));
            CollectionAssert.Contains(header, nameof(Line.Value));
            CollectionAssert.Contains(header, nameof(LineSeries.OptionName));
            CollectionAssert.Contains(header, nameof(LineSeries.ParameterName));
            CollectionAssert.Contains(header, nameof(LineSeries.StateName));
        }

        [Test]
        public void verify_that_export_data_is_created_as_expected()
        {
            var data = this.lineSeries.GetExportData().ToList();

            Assert.AreEqual(2, data.Count);

            Assert.AreEqual(5, data[0].Count());
            CollectionAssert.Contains(data[0], this.lineSeries.Lines.First().RevisionNumber);
            CollectionAssert.Contains(data[0], this.lineSeries.Lines.First().Value);
            CollectionAssert.Contains(data[0], this.lineSeries.ParameterName);
            CollectionAssert.Contains(data[0], this.lineSeries.OptionName);
            CollectionAssert.Contains(data[0], this.lineSeries.StateName);

            Assert.AreEqual(5, data[1].Count());
            CollectionAssert.Contains(data[1], this.lineSeries.Lines.Last().RevisionNumber);
            CollectionAssert.Contains(data[1], this.lineSeries.Lines.Last().Value);
            CollectionAssert.Contains(data[1], this.lineSeries.ParameterName);
            CollectionAssert.Contains(data[1], this.lineSeries.OptionName);
            CollectionAssert.Contains(data[1], this.lineSeries.StateName);
        }
    }
}
