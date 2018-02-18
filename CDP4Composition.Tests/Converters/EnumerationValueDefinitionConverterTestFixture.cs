// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerationValueDefinitionConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Collections.Generic;
    using CDP4Common;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="EnumerationValueDefinitionConverter"/> class
    /// </summary>
    [TestFixture]
    public class EnumerationValueDefinitionConverterTestFixture
    {
        private EnumerationValueDefinitionConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new EnumerationValueDefinitionConverter();
        }

        [Test]
        public void VerifyThatConvertBackThrowsNotSupporedException()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }

        [Test]
        public void VerifyThatAnyOtherClassThanEnumerationValueDefinitionGetsConvertedToHyphen()
        {
            var notThing = new NotThing("nothing");

            Assert.AreEqual("-", this.converter.Convert(notThing, null, null, null));
        }

        [Test]
        public void VerifyThatAListOfEnumerationValueDefinitionsIsConverted()
        {
            var enumeration = new EnumerationParameterType { Name = "Technology Readiness Level", ShortName = "TRL" };

            var level1 = new EnumerationValueDefinition { Name = "1", ShortName = "1" };
            var level2 = new EnumerationValueDefinition { Name = "2", ShortName = "2" };

            enumeration.ValueDefinition.Add(level1);
            enumeration.ValueDefinition.Add(level2);

            var result = this.converter.Convert(enumeration.ValueDefinition, null, null, null);

            Assert.AreEqual("1 | 2", result);
        }

        [Test]
        public void VerfifyThatEmptyListOfValueDefinitionsReturnsHyphen()
        {
            var emptyList = new List<EnumerationValueDefinition>();
            var result = this.converter.Convert(emptyList, null, null, null);

            Assert.AreEqual("-", result);
        }
    }
}

