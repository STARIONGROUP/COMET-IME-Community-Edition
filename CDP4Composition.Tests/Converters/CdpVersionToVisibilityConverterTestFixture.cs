// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CdpVersionToVisibilityConverterTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    using CDP4Common;
    using CDP4Common.Helpers;
    using CDP4Dal;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="CdpVersionToVisibilityConverter"/> class.
    /// </summary>
    [TestFixture]
    public class CdpVersionToVisibilityConverterTestFixture
    {
        private CdpVersionToVisibilityConverter cdpVersionToVisibilityConverter;
        
        private Mock<ISession> sessionOneOneZero;

        private Mock<ISession> sessionOneTwoZero;

        [SetUp]
        public void SetUp()
        {
            this.sessionOneOneZero = new Mock<ISession>();
            this.sessionOneOneZero.Setup(s => s.DalVersion).Returns(new Version("1.1.0"));

            this.sessionOneTwoZero = new Mock<ISession>();
            this.sessionOneTwoZero.Setup(s => s.DalVersion).Returns(new Version("1.2.0"));

            this.cdpVersionToVisibilityConverter = new CdpVersionToVisibilityConverter();
        }

        [Test]
        public void VerifyThatVisibleIsReturnedForVersionProperties()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToVisibilityConverter.Convert(testViewModel, null, "Versioned", null);

            var visibility = (Visibility)result;

            Assert.AreEqual(Visibility.Visible, visibility);
        }

        [Test]
        public void VerifyThatVisibleIsReturnedForNonVersionProperties()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToVisibilityConverter.Convert(testViewModel, null, "NonVersioned", null);
            var visibility = (Visibility)result;

            Assert.AreEqual(Visibility.Visible, visibility);
        }
        
        [Test]
        public void VerifyThatIfNotIISessioniIsPassedDoNothingIsReturned()
        {
            var result = this.cdpVersionToVisibilityConverter.Convert(new object(), null, "NonVersioned", null);
            Assert.AreEqual(Binding.DoNothing, result);
        }

        [Test]
        public void VerfiyThatVisibleIsReturnedForUknownProperty()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);
            var result = this.cdpVersionToVisibilityConverter.Convert(testViewModel, null, "unkownproperty", null);
            Assert.AreEqual(Binding.DoNothing, result);
        }

        [Test]
        public void VerifyThatIfParameterIsNotStringDoNothingIsReturned()
        {
            var testViewModel = new TestViewModel(this.sessionOneOneZero.Object);

            var result = this.cdpVersionToVisibilityConverter.Convert(testViewModel, null, new object(), null);
            Assert.AreEqual(Binding.DoNothing, result);
        }

        [Test]
        public void VerifyThatConvertBackThrowsException()
        {
            Assert.Throws<NotSupportedException>(() => this.cdpVersionToVisibilityConverter.ConvertBack(null, null, null, null));
        }
    }

    /// <summary>
    /// Test view-model
    /// </summary>
    internal class TestViewModel : IISession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestViewModel"/> class
        /// </summary>
        /// <param name="session"></param>
        public TestViewModel(ISession session)
        {
            this.Session = session;
        }

        /// <summary>
        /// Gets the <see cref="ISession"/>
        /// </summary>
        public ISession Session { get; private set; }

        [CDPVersion("1.1.0")]
        public string Versioned { get; set; }
        
        public string NonVersioned { get; set; }
    }
}
