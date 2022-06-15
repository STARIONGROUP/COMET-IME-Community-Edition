// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DownloadFileServiceTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Services;
    using CDP4Composition.Tests.Extensions;

    using CDP4Dal;

    using CommonServiceLocator;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="DownloadFileService"/> class
    /// </summary>
    [TestFixture]
    public class DownloadFileServiceTestFixture
    {
        private Mock<IDownloadFileViewModel> downloadFileViewModel;
        private Mock<ISession> session;
        private FileRevision fileRevision;
        private File file;
        private FileExtensionMethodsTestFixture fileExtensionMethodsTestFixture;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IMessageBoxService> messageBoxService;
        private DownloadFileService service;

        [SetUp]
        public void SetUp()
        {
            this.fileExtensionMethodsTestFixture = new FileExtensionMethodsTestFixture();
            this.fileExtensionMethodsTestFixture.Setup();

            this.serviceLocator = this.fileExtensionMethodsTestFixture.ServiceLocator;
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);

            this.messageBoxService = new Mock<IMessageBoxService>();
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            this.session = this.fileExtensionMethodsTestFixture.Session;
            this.fileRevision = this.fileExtensionMethodsTestFixture.FileRevision;

            this.file = new File();
            this.file.FileRevision.Add(this.fileRevision);

            this.downloadFileViewModel = new Mock<IDownloadFileViewModel>();
            this.downloadFileViewModel.Setup(x => x.Session).Returns(this.session.Object);

            this.session.Setup(x => x.ReadFile(this.fileRevision))
                .Callback(() =>
                {
                    this.downloadFileViewModel.VerifySet(x => x.IsBusy = true, Times.Once);
                    this.downloadFileViewModel.VerifySet(x => x.IsBusy = false, Times.Never);
                    this.downloadFileViewModel.VerifySet(x => x.LoadingMessage = It.IsAny<string>(), Times.Once);
                    System.Threading.Thread.Sleep(500);
                    this.session.Verify(x => x.CanCancel(), Times.AtLeastOnce);
                })
                .ReturnsAsync(new byte[1]);

            this.service = new DownloadFileService();
        }

        [Test]
        public async Task VerifyThatDownloadFileRevisionWorks()
        {
            await this.service.ExecuteDownloadFile(this.downloadFileViewModel.Object, this.fileRevision);

            this.VerifyProperties();
        }

        [Test]
        public async Task VerifyThatDownloadFileWorks()
        {
            await this.service.ExecuteDownloadFile(this.downloadFileViewModel.Object, this.file);

            this.VerifyProperties();
        }

        [Test]
        public async Task VerifyThatCancelDownloadWorks1()
        {
            this.session.Setup(x => x.ReadFile(this.fileRevision)).Throws<OperationCanceledException>();

            await this.service.ExecuteDownloadFile(this.downloadFileViewModel.Object, this.fileRevision);

            this.VerifyMessageBox(MessageBoxImage.Exclamation);
            this.VerifyProperties();
        }

        [Test]
        public async Task VerifyThatCancelDownloadWorks2()
        {
            this.session.Setup(x => x.ReadFile(this.fileRevision)).Throws(new Exception("", new TaskCanceledException()));

            await this.service.ExecuteDownloadFile(this.downloadFileViewModel.Object, this.fileRevision);

            this.VerifyMessageBox(MessageBoxImage.Exclamation);
            this.VerifyProperties();
        }

        [Test]
        public async Task VerifyThatCancelDownloadWorks3()
        {
            this.session.Setup(x => x.ReadFile(this.fileRevision)).Throws(new Exception());

            await this.service.ExecuteDownloadFile(this.downloadFileViewModel.Object, this.fileRevision);

            this.VerifyMessageBox(MessageBoxImage.Error);
            this.VerifyProperties();
        }

        private void VerifyMessageBox(MessageBoxImage icon)
        {
            this.messageBoxService.Verify(
                x => x.Show(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<MessageBoxButton>(),
                    icon
                    )
                );
        }

        private void VerifyProperties()
        {
            this.session.Verify(x => x.ReadFile(this.fileRevision), Times.Once);
            this.downloadFileViewModel.VerifySet(x => x.IsBusy = false, Times.Once);
            this.downloadFileViewModel.VerifySet(x => x.LoadingMessage = "", Times.Once);
            this.downloadFileViewModel.VerifySet(x => x.LoadingMessage = It.IsAny<string>(), Times.Exactly(2));
        }
    }
}
