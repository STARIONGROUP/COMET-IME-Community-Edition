// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReactiveListClassKindConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.Converters
{
    using System.Collections;
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using NUnit.Framework;
    using ReactiveUI;
    using CDP4Composition.Converters;

    /// <summary>
    /// Suite of tests for the <see cref="ReactiveClassKindToObjectListConverter"/>
    /// </summary>
    [TestFixture]
    internal class ReactiveListClassKindConverterTestFixture
    {
        [Test]
        public void VerifyThatConvertWorksWithValue()
        {
            var converter = new ReactiveClassKindToObjectListConverter();
            var value = converter.Convert(new ReactiveList<ClassKind>{ ClassKind.ActualFiniteState }, null, null, null) as IList;

            Assert.IsNotNull(value);
            Assert.AreEqual(1, value.Count);
        }

        [Test]
        public void VerifyThatConvertWorksWithNullValue()
        {
            var converter = new ReactiveClassKindToObjectListConverter();
            var value = converter.Convert(null, null, null, null) as IList;

            Assert.IsNotNull(value);
            Assert.AreEqual(0, value.Count);
        }

        [Test]
        public void VerifyThatConvertBackWorksWithValue()
        {
            var converter = new ReactiveClassKindToObjectListConverter();
            var value = converter.ConvertBack(new List<ClassKind> {ClassKind.Person}, null, null, null) as ReactiveList<ClassKind>;

            Assert.AreEqual(1, value.Count);
        }

        [Test]
        public void VerifyThatConvertBackWorksWithNullValue()
        {
            var converter = new ReactiveClassKindToObjectListConverter();
            var value = converter.ConvertBack(null, null, null, null) as ReactiveList<ClassKind>;

            Assert.AreEqual(0, value.Count);
        }
    }
}