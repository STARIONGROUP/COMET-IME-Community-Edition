// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Tests.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Navigation;
    using CDP4Composition.Services;

    using CDP4Dal;

    using CDP4Requirements.ViewModels;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReqIFSharp;

    [TestFixture]
    internal class ReqIfExportDialogViewModelTestFixture
    {
        private Mock<ISession> session;
        private Assembler assembler;
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private Mock<IReqIFSerializer> serializer;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IMessageBoxService> messageBoxService;
        private EngineeringModel model;
        private Iteration iteration;
        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;

        private readonly Uri uri = new Uri("http://test.com");

        [SetUp]
        public void Setup()
        {
            this.serviceLocator = new Mock<IServiceLocator>();
            this.messageBoxService = new Mock<IMessageBoxService>();

            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IMessageBoxService>()).Returns(this.messageBoxService.Object);

            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.serializer = new Mock<IReqIFSerializer>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri) { Name = "model" };
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationSetup);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri) { EngineeringModelSetup = this.modelsetup };
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri) { IterationSetup = this.iterationSetup };
            this.model.Iteration.Add(this.iteration);
        }

        [Test]
        public void VerifyThatExceptionRaises1()
        {
            Assert.Throws<ArgumentNullException>(() => new ReqIfExportDialogViewModel(null, new List<Iteration>(), this.fileDialogService.Object, this.serializer.Object));
        }

        [Test]
        public void VerifyThatExceptionRaises2()
        {
            Assert.Throws<ArgumentNullException>(() => new ReqIfExportDialogViewModel(new List<ISession>(), null, this.fileDialogService.Object, this.serializer.Object));
        }

        [Test]
        public void VerifyThatExceptionRaises3()
        {
            Assert.Throws<ArgumentNullException>(() => new ReqIfExportDialogViewModel(new List<ISession>(), new List<Iteration>(), null, this.serializer.Object));
        }

        [Test]
        public void VerifyThatExceptionRaises4()
        {
            Assert.Throws<ArgumentNullException>(() => new ReqIfExportDialogViewModel(new List<ISession>(), new List<Iteration>(), this.fileDialogService.Object, null));
        }

        [Test]
        public void VeriyThatOkCommandWorks()
        {
            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            this.fileDialogService.Setup(
                    x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1))
                .Returns("test");

            vm.SelectedIteration = vm.Iterations.First();
            Assert.That(vm.SelectedIteration.IterationNumber, Is.Not.Null.Or.Empty);
            Assert.That(vm.SelectedIteration.Model, Is.Not.Null.Or.Empty);
            Assert.That(vm.SelectedIteration.DataSourceUri, Is.Not.Null.Or.Empty);
            Assert.IsNotNull(vm.SelectedIteration.Iteration);

            Assert.IsFalse(vm.OkCommand.CanExecute(null));
            Assert.IsTrue(vm.CancelCommand.CanExecute(null));
            Assert.IsTrue(vm.BrowseCommand.CanExecute(null));

            vm.BrowseCommand.Execute(null);
            Assert.That(vm.Path, Is.Not.Null.Or.Empty);

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
            vm.OkCommand.Execute(null);

            Assert.IsNotNull(vm.DialogResult);
        }
    }
}
