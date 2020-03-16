// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClassKindConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 ClassKindConverterTestFixture System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using CDP4Common.CommonData;

namespace CDP4DiagramEditor.Tests.Helpers
{
    using CDP4DiagramEditor.Helpers;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="ClassKindConverter"/> class.
    /// </summary>
    [TestFixture]
    public class ClassKindConverterTestFixture
    {
        private ClassKindConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new ClassKindConverter();
        }

        [Test]
        public void VerifyThatConverReturnsTheExpectedResult()
        {
            var result = this.converter.Convert(ClassKind.ElementDefinition, null, null, null);
            Assert.AreEqual("<<Element Definition>>", result);
        }

        [Test]
        public void VerifyThatConvertBackThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}
