// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NamedShortNamedThingToStringConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Converters;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="NamedShortNamedThingToStringConverter"/> class
    /// </summary>
    [TestFixture]
    public class NamedShortNamedThingToStringConverterTestFixture
    {
        private NamedShortNamedThingToStringConverter converter;

        private TextParameterType textParameterType;

        private UserPreference userPreference;

        private ExternalIdentifierMap externalIdentifierMap;

        private OrExpression orExpression;

        [SetUp]
        public void SetUp()
        {
            this.converter = new NamedShortNamedThingToStringConverter();

            this.textParameterType = new TextParameterType()
            {
                ShortName = "txt",
                Name = "text"
            };

            this.externalIdentifierMap = new ExternalIdentifierMap()
            {
                Name = "externalIdentifierMap"
            };

            this.userPreference = new UserPreference()
            {
                ShortName = "preference"
            };
            
            this.orExpression = new OrExpression();
        }

        [Test]
        public void Verify_that_Named_and_Shortnamed_Thing_can_be_converted()
        {
            Assert.AreEqual("text [txt]", this.converter.Convert(this.textParameterType, null, null, null));
        }

        [Test]
        public void Verify_that_Named_can_be_converted()
        {
            Assert.AreEqual("externalIdentifierMap", this.converter.Convert(this.externalIdentifierMap, null, null, null));
        }

        [Test]
        public void Verify_that_Shortnamed_Thing_can_be_converted()
        {
            Assert.AreEqual("preference", this.converter.Convert(this.userPreference, null, null, null));
        }

        [Test]
        public void Verify_that_Thing_can_be_converted()
        {
            Assert.AreEqual("", this.converter.Convert(this.orExpression, null, null, null));
        }

        [Test]
        public void Verify_that_ConvertBack_is_not_supported()
        {
            Assert.Throws<NotSupportedException>(() => this.converter.ConvertBack(null, null, null, null));
        }
    }
}