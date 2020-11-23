// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileTypeRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.Tests.ViewModels.Rows
{
    using System;
    using System.Collections.Concurrent;
    using System.Reactive.Concurrency;
    using System.Reflection;

    using BasicRdl.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;
    
    /// <summary>
    /// Suite of tests for the <see cref="FileTypeRowViewModel"/> class
    /// </summary>
    [TestFixture]
    public class FileTypeRowViewModelTestFixture
    {
        private Uri uri;
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache;
        private Mock<ISession> session;
        private PropertyInfo revInfo = typeof(Thing).GetProperty("RevisionNumber");
        private SiteReferenceDataLibrary siteReferenceDataLibrary;
        private FileType fileType;

        [SetUp]
        public void SetUp()
        {
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.session = new Mock<ISession>();
            this.uri = new Uri("http://www.rheagroup.com");            
            this.cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

            this.siteReferenceDataLibrary = new SiteReferenceDataLibrary(Guid.NewGuid(), this.cache, this.uri)
                                                {
                                                    ShortName = "rdl",
                                                    Name = "reference data library"
                                                };

            this.fileType = new FileType(Guid.NewGuid(), this.cache, this.uri) { ShortName = "cdp" };
            this.siteReferenceDataLibrary.FileType.Add(this.fileType);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatPropertiesAreSetByConstructor()
        {
            var row = new FileTypeRowViewModel(this.fileType, this.session.Object, null);
            Assert.AreEqual("cdp", row.ShortName);
            Assert.AreEqual("rdl", row.ContainerRdl);
        }

        [Test]
        public void VerifyThatChangeMessageIsProcessed()
        {
            var row = new FileTypeRowViewModel(this.fileType, this.session.Object, null);
            this.fileType.ShortName = "cdp4";

            this.revInfo.SetValue(this.fileType, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(this.fileType, EventKind.Updated);

            Assert.AreEqual("cdp4", row.ShortName);
        }
    }
}
