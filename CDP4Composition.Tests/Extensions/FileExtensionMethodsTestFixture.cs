// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileExtensionMethodsTestFixture.cs" company="RHEA System S.A.">
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

    using CDP4CommonView;

    using CDP4Composition.Extensions;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Test suite for the <see cref="FileDialogViewModel"/> class
    /// </summary>
    [TestFixture]
    internal class FileExtensionMethodsTestFixture
    {
        private const string FileContent = "This is file content";
        private const string FileName = "FileRevisionDownloadTestFile.txt";

        private Mock<IOpenSaveFileDialogService> fileDialogService;
        internal Mock<IServiceLocator> ServiceLocator;

        internal Mock<ISession> Session;
        internal FileRevision FileRevision;

        [SetUp]
        public void Setup()
        {
            this.Session = new Mock<ISession>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.ServiceLocator = new Mock<IServiceLocator>();
            CommonServiceLocator.ServiceLocator.SetLocatorProvider(() => this.ServiceLocator.Object);
            this.ServiceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.fileDialogService.Object);

            this.fileDialogService.Setup(
                    x => x.GetSaveFileDialog(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<int>()))
                .Returns(FileName);

            this.Session.Setup(x => x.ReadFile(It.IsAny<FileRevision>())).ReturnsAsync(Encoding.ASCII.GetBytes(FileContent));

            this.FileRevision = new FileRevision(Guid.NewGuid(), null, null)
            {
                Name = "File",
                Creator = new Participant(),
                ContainingFolder = new Folder(),
                CreatedOn = DateTime.UtcNow.AddHours(-1),
                ContentHash = "DOESNOTMATTER"
            };

            this.FileRevision.FileType.Add(new FileType());
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public async Task VerifyThatDownloadFileWorks()
        {
            await this.FileRevision.DownloadFile(this.Session.Object);

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
                nameof(CDP4Common.EngineeringModelData.FileRevision.Name),
                nameof(CDP4Common.EngineeringModelData.FileRevision.ContainingFolder),
                nameof(CDP4Common.EngineeringModelData.FileRevision.ContentHash),
                nameof(CDP4Common.EngineeringModelData.FileRevision.Creator),
                nameof(CDP4Common.EngineeringModelData.FileRevision.CreatedOn),
                nameof(CDP4Common.EngineeringModelData.FileRevision.Path),
                nameof(CDP4Common.EngineeringModelData.FileRevision.FileType),
                nameof(CDP4Common.EngineeringModelData.FileRevision.ClassKind),
                nameof(CDP4Common.EngineeringModelData.FileRevision.ExcludedDomain),
                nameof(CDP4Common.EngineeringModelData.FileRevision.ExcludedPerson),
                nameof(CDP4Common.EngineeringModelData.FileRevision.Iid),
                nameof(CDP4Common.EngineeringModelData.FileRevision.ModifiedOn),
                nameof(CDP4Common.EngineeringModelData.FileRevision.ThingPreference),
                nameof(CDP4Common.EngineeringModelData.FileRevision.RevisionNumber)
            };

            CollectionAssert.AreEquivalent(realFileRevisionProperties, expectedFileRevisionProperties, 
                $@"Found unexpected, or missing properties in {nameof(CDP4Common.EngineeringModelData.FileRevision)}. 
                         Please update var {nameof(expectedFileRevisionProperties)} 
                         and make sure {nameof(this.VerifyThatCopyToNewWorks)} works as expected.");
        }

        [Test]
        public void VerifyThatCopyToNewWorks()
        {
            var participant = new Participant();
            var newFileRevision =  this.FileRevision.CopyToNew(participant);

            // Copied properties
            Assert.AreEqual(this.FileRevision.Name, newFileRevision.Name);
            Assert.AreEqual(this.FileRevision.ContainingFolder, newFileRevision.ContainingFolder);
            Assert.AreEqual(this.FileRevision.ContentHash, newFileRevision.ContentHash);
            Assert.AreEqual(this.FileRevision.Path, newFileRevision.Path);

            // new Creator
            Assert.AreEqual(newFileRevision.Creator, participant);
            
            // CreatedOn of the copy is higher than CreatedOn of the source FileRevision
            Assert.IsTrue(this.FileRevision.CreatedOn < newFileRevision.CreatedOn);

            // Same FileTypes
            CollectionAssert.AreEqual(this.FileRevision.FileType, newFileRevision.FileType);
        }
    }
}
