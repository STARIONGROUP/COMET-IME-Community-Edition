// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstantRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
        private Mock<ISession> session;
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
