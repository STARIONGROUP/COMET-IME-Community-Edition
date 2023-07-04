// -------------------------------------------------------------------------------------------------
// <copyright file="FileRowViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft
//            Nathanael Smiechowski, Kamil Wojnowski
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
        private Mock<IFileStoreFileAndFolderHandler> fileStoreFileAndFolderHandler;
        private Uri uri = new Uri("http://www.rheagroup.com");
        private ConcurrentDictionary<CacheKey, Lazy<Thing>> cache = new ConcurrentDictionary<CacheKey, Lazy<Thing>>();

        private Person person;
        private Participant participant;
        private DomainFileStore store;
        
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

            this.store = new DomainFileStore(Guid.NewGuid(), this.cache, this.uri);
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

            this.fileStoreFileAndFolderHandler = new Mock<IFileStoreFileAndFolderHandler>();
        }

        [Test]
        public void VerifyThatContainerViewModelMayNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FileRowViewModel(this.file, this.session.Object, null, this.fileStoreFileAndFolderHandler.Object));
        }

        [Test]
        public void VerifyThatPropertiesAreSet()
        {
            var domainFileStoreRowViewModel = new DomainFileStoreRowViewModel(this.store, this.session.Object, null);

            this.file.FileRevision.Add(this.fileRevision1);

            var viewModel = new FileRowViewModel(this.file, this.session.Object, domainFileStoreRowViewModel, this.fileStoreFileAndFolderHandler.Object);
            Assert.AreEqual(this.fileRevision1.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture), viewModel.CreationDate);
            Assert.AreEqual(this.fileRevision1.Name, viewModel.Name);
            Assert.AreEqual(this.fileRevision1.Creator.Person.Name, viewModel.CreatorValue);
            Assert.IsFalse(viewModel.IsLocked);
            Assert.AreEqual(string.Empty, viewModel.Locker);
            Assert.AreEqual("1", viewModel.Name);
            this.fileStoreFileAndFolderHandler.Verify(x => x.UpdateFileRowPosition(this.file, It.IsAny<FileRevision>()), Times.Never);

            this.file.LockedBy = this.person;
            this.revisionNumberPropertyInfo.SetValue(this.file, 10);
            CDPMessageBus.Current.SendObjectChangeEvent(this.file, EventKind.Updated);

            Assert.IsTrue(viewModel.IsLocked);
            Assert.AreEqual("John Doe", viewModel.Locker);
            this.fileStoreFileAndFolderHandler.Verify(x => x.UpdateFileRowPosition(this.file, It.IsAny<FileRevision>()), Times.Never);

            this.file.FileRevision.Add(this.fileRevision2);
            this.revisionNumberPropertyInfo.SetValue(this.file, 11);
            CDPMessageBus.Current.SendObjectChangeEvent(this.file, EventKind.Updated);

            Assert.AreEqual(this.fileRevision2.CreatedOn.ToString("yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture), viewModel.CreationDate);
            Assert.AreEqual(this.fileRevision2.Name, viewModel.Name);
            Assert.AreEqual(this.fileRevision2.Creator.Person.Name, viewModel.CreatorValue);
            this.fileStoreFileAndFolderHandler.Verify(x => x.UpdateFileRowPosition(this.file, It.IsAny<FileRevision>()), Times.Once);
        }
    }
}