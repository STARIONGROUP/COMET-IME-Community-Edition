// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstantRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRDL.Tests.ViewModels
{
    using System;
    using System.Reactive.Concurrency;
    using BasicRdl.ViewModels;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;

    using Moq;
    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ConstantRowViewModel"/>
    /// </summary>
    [TestFixture]
    public class ConstantRowViewModelTestFixture
    {
        /// <summary>
        /// A mock of the session.
        /// </summary>
        private Mock<ISession> session;

        /// <summary>
        /// The uri.
        /// </summary>
        private Uri uri;

        [SetUp]
        public void Setup()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://test.com");
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatTheConstructorSetsTheProperties()
        {
            var shortname = "constantshortname";
            var name = "constant name";
            

            var ratioScale = new RatioScale(Guid.NewGuid(), null, this.uri)
            {
                Name = "ratio scale",
                ShortName = "ratioscaleshortname",
            };

            var qk = new SimpleQuantityKind(Guid.NewGuid(), null, this.uri);            
            qk.DefaultScale = ratioScale;

            var constant = new Constant(Guid.NewGuid(), null, this.uri)
            {
                Name = name,
                ShortName = shortname,
                ParameterType = qk,
                Scale = ratioScale
            };

            var constantRowViewModel = new ConstantRowViewModel(constant, this.session.Object, null);

            Assert.AreEqual(shortname, constantRowViewModel.ShortName);
            Assert.AreEqual(name, constantRowViewModel.Name);
            Assert.AreEqual(string.Empty, constantRowViewModel.ContainerRdl);
            Assert.AreEqual(ratioScale, constantRowViewModel.SelectedScale);
        }
    }
}
