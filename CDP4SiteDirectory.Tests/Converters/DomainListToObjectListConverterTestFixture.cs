// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DomainListToObjectListConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Tests.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4SiteDirectory.Converters;
    using NUnit.Framework;
    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="DomainListToObjectListConverter"/> class.
    /// </summary>
    [TestFixture]
    public class DomainListToObjectListConverterTestFixture
    {
        /// <summary>
        /// The <see cref="DomainListToObjectListConverter"/> under test.
        /// </summary>
        private DomainListToObjectListConverter converter;

        [SetUp]
        public void SetUp()
        {
            this.converter = new DomainListToObjectListConverter();
        }

        [Test]
        public void VerifyThatConvertReturnsAListOfObject()
        {
            var list = new ReactiveList<DomainOfExpertise>();
            var domain1 = new DomainOfExpertise(Guid.NewGuid(), null, null);
            var domain2 = new DomainOfExpertise(Guid.NewGuid(), null, null);

            list.Add(domain1);
            list.Add(domain2);

            var result = (IEnumerable<object>)this.converter.Convert(list, null, null, null);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public void VerifyThatNullValueConvertReturnsAListOfObject()
        {
            var result = this.converter.Convert(null, null, null, null);
            Assert.IsInstanceOf<List<object>>(result);

            var listofobject = result as List<object>;

            CollectionAssert.IsEmpty(listofobject);
        }

        [Test]
        public void VerifyThatConvertBackReturnsAReactiveListOfDomainOfExpertise()
        {
            var objectlist = new List<object>();
            var domain1 = new DomainOfExpertise(Guid.NewGuid(), null, null);
            var domain2 = new DomainOfExpertise(Guid.NewGuid(), null, null);
            objectlist.Add(domain1);
            objectlist.Add(domain2);

            var result = (ReactiveList<DomainOfExpertise>)this.converter.ConvertBack(objectlist, null, null, null);

            Assert.AreEqual(2, result.Count);
        }
    }
}
