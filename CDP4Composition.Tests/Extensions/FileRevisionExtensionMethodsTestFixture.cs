// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionExtensionMethodsTestFixture.cs" company="RHEA System S.A.">
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
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using CDP4Common;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Extensions;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Test suite for the <see cref="FileRevisionExtensionMethods"/> class
    /// </summary>
    [TestFixture]
    internal class FileRevisionExtensionMethodsTestFixture
    {
        private const string FileContent = "This is file content";
        private const string FileName = "FileRevisionDownloadTestFile.txt";

        private Mock<ISession> session;
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private Mock<IServiceLocator> serviceLocator;
        private FileRevision fileRevision;

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.fileDialogService.Object);

            this.fileDialogService.Setup(
                    x => x.GetSaveFileDialog(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<int>()))
                .Returns(FileName);

            this.session.Setup(x => x.ReadFile(It.IsAny<FileRevision>())).ReturnsAsync(Encoding.ASCII.GetBytes(FileContent));

            this.fileRevision = new FileRevision(Guid.NewGuid(), null, null)
            {
                Name = "File",
                Creator = new Participant(),
                ContainingFolder = new Folder(),
                CreatedOn = DateTime.UtcNow.AddHours(-1),
                ContentHash = "DOESNOTMATTER"
            };

            this.fileRevision.FileType.Add(new FileType());
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task VerifyThatDownloadFileWorks()
        {
            await this.fileRevision.DownloadFile(this.session.Object);

            var result = System.IO.File.ReadAllBytes(FileName);

            System.IO.File.Delete(FileName);

            Assert.AreEqual(FileContent, Encoding.ASCII.GetString(result));
        }

        [Test]
        public void VerifyThatCopyToNewStaysWorking()
        {
            var realFileRevisionProperties = typeof(FileRevision).GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(UmlInformationAttribute), true).Any())
                .Select(x => x.Name)
                .ToList();

            var expectedFileRevisionProperties = new List<string>
            {
                nameof(FileRevision.Name),
                nameof(FileRevision.ContainingFolder),
                nameof(FileRevision.ContentHash),
                nameof(FileRevision.Creator),
                nameof(FileRevision.CreatedOn),
                nameof(FileRevision.Path),
                nameof(FileRevision.FileType),
                nameof(FileRevision.ClassKind),
                nameof(FileRevision.ExcludedDomain),
                nameof(FileRevision.ExcludedPerson),
                nameof(FileRevision.Iid),
                nameof(FileRevision.ModifiedOn),
                nameof(FileRevision.ThingPreference),
                nameof(FileRevision.RevisionNumber)
            };

            CollectionAssert.AreEquivalent(realFileRevisionProperties, expectedFileRevisionProperties,
                $@"Found unexpected, or missing properties in {nameof(FileRevision)}. 
                         Please update var {nameof(expectedFileRevisionProperties)} 
                         and make sure {nameof(this.VerifyThatCopyToNewWorks)} works as expected.");
        }

        [Test]
        public void VerifyThatCopyToNewWorks()
        {
            var participant = new Participant();
            var newFileRevision = this.fileRevision.CopyToNew(participant);

            // Copied properties
            Assert.AreEqual(this.fileRevision.Name, newFileRevision.Name);
            Assert.AreEqual(this.fileRevision.ContainingFolder, newFileRevision.ContainingFolder);
            Assert.AreEqual(this.fileRevision.ContentHash, newFileRevision.ContentHash);
            Assert.AreEqual(this.fileRevision.Path, newFileRevision.Path);

            // new Creator
            Assert.AreEqual(newFileRevision.Creator, participant);

            // CreatedOn of the copy is higher than CreatedOn of the source FileRevision
            Assert.IsTrue(this.fileRevision.CreatedOn < newFileRevision.CreatedOn);

            // Same FileTypes
            CollectionAssert.AreEqual(this.fileRevision.FileType, newFileRevision.FileType);
        }
    }
}
