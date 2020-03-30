// -------------------------------------------------------------------------------------------------
// <copyright file="FileRowViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Tests.ViewModels.CommonFileStoreBrowser
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4EngineeringModel.ViewModels;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="FileRowViewModel"/>
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class FileRowViewModelTestFixture
    {
        private PropertyInfo revisionNumberPropertyInfo = typeof(Thing).GetProperty("RevisionNumber");

        private Mock<ISession> session;
        private Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

        private Person person;
        private Participant participant;
        private CommonFileStore store;
        
        private File file;
        private FileRevision fileRevision1;
        private FileRevision fileRevision2;
        
        [SetUp]
        public void SetUp()
        {
            this.session = new Mock<ISession>();

            this.person = new Person(Guid.NewGuid(), this.cache, this.uri)
            {
                ShortName = "person",
                GivenName = "John",
                Surname = "Doe" 
            };
            this.participant = new Participant(Guid.NewGuid(), this.cache, this.uri)
            {
                Person = this.person,
                IsActive = true,
            };

            this.store = new CommonFileStore(Guid.NewGuid(), this.cache, this.uri);

            this.store = new CommonFileStore(Guid.NewGuid(), this.cache, this.uri);            
            this.file = new File(Guid.NewGuid(), this.cache, this.uri);
            
            this.fileRevision1 = new FileRevision(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "1",
                Creator = this.participant,
                CreatedOn = new DateTime(1, 1, 1)
            };
            this.fileRevision2 = new FileRevision(Guid.NewGuid(), this.cache, this.uri)
            {
                Name = "1.1",
                Creator = this.participant,
                CreatedOn = new DateTime(1, 1, 2)
            };

        }

        [Test]
        public void VerifyThatContainerViewModelMayNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FileRowViewModel(this.file, this.session.Object, null, null));
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var commonFileStoreRowViewModel = new CommonFileStoreRowViewModel(this.store, this.session.Object, null);

            this.file.FileRevision.Add(this.fileRevision1);

            var viewModel = new FileRowViewModel(this.file, this.session.Object, commonFileStoreRowViewModel, null);
            Assert.AreEqual(new DateTime(1, 1, 1).ToString(CultureInfo.InvariantCulture), viewModel.CreatedOn);
            Assert.AreEqual("John Doe", viewModel.CreatorValue);
            Assert.IsFalse(viewModel.IsLocked);
            Assert.AreEqual(string.Empty, viewModel.Locker);
            Assert.AreEqual("1", viewModel.Name);

            this.file.LockedBy = this.person;
            revisionNumberPropertyInfo.SetValue(this.file, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(this.file, EventKind.Updated);

            Assert.IsTrue(viewModel.IsLocked);
            Assert.AreEqual("John Doe", viewModel.Locker);
        }
    }
}