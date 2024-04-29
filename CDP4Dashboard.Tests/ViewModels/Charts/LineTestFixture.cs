// -------------------------------------------------------------------------------------------------
// <copyright file="LineTestFixture.cs" company="Starion Group S.A.">
//   Copyright (c) 2020 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.ViewModels.Rows
{
    using System.Linq;

    using CDP4Dashboard.ViewModels.Charts;

    using NUnit.Framework;

    /// <summary>
    /// Tests for <see cref="Line"/> instances
    /// </summary>
    [TestFixture]
    internal class LineTestFixture
    {
        internal Line line1;
        internal Line line2;

        [SetUp]
        public void Setup()
        {
            this.line1 = new Line { Value = 123456, RevisionNumber = 1 };
            this.line2 = new Line { Value = "123456", RevisionNumber = 1 };
        }

        [Test]
        public void verify_that_export_headers_are_created_as_expected()
        {
            var header1 = this.line1.GetExportHeaders().ToList();
            var header2 = this.line2.GetExportHeaders().ToList();

            Assert.AreEqual(2, header1.Count);
            CollectionAssert.Contains(header1, nameof(Line.RevisionNumber));
            CollectionAssert.Contains(header1, nameof(Line.Value));

            Assert.AreEqual(2, header2.Count);
            CollectionAssert.Contains(header2, nameof(Line.RevisionNumber));
            CollectionAssert.Contains(header2, nameof(Line.Value));
        }

        [Test]
        public void verify_that_export_data_is_created_as_expected()
        {
            var data1 = this.line1.GetExportData().ToList();
            var data2 = this.line2.GetExportData().ToList();

            Assert.AreEqual(1, data1.Count);
            Assert.AreEqual(2, data1[0].Count());
            CollectionAssert.Contains(data1[0], this.line1.RevisionNumber);
            CollectionAssert.Contains(data1[0], this.line1.Value);

            Assert.AreEqual(1, data2.Count);
            Assert.AreEqual(2, data2[0].Count());
            CollectionAssert.Contains(data2[0], this.line2.RevisionNumber);
            CollectionAssert.Contains(data2[0], this.line2.Value);
        }
    }
}
