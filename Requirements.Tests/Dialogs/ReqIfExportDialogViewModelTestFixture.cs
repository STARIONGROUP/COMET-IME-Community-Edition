// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using CDP4Dal;
    using CDP4Requirements.ViewModels;
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
        private EngineeringModel model;
        private Iteration iteration;
        private SiteDirectory sitedir;
        private EngineeringModelSetup modelsetup;
        private IterationSetup iterationSetup;

        private readonly Uri uri = new Uri("http://test.com");

        [SetUp]
        public void Setup()
        {
            this.session = new Mock<ISession>();
            this.assembler = new Assembler(this.uri);
            this.serializer = new Mock<IReqIFSerializer>();
            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString());
            this.session.Setup(x => x.Assembler).Returns(this.assembler);

            this.sitedir = new SiteDirectory(Guid.NewGuid(), this.assembler.Cache, this.uri);
            this.modelsetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri){Name = "model"};
            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.sitedir.Model.Add(this.modelsetup);
            this.modelsetup.IterationSetup.Add(this.iterationSetup);

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri){EngineeringModelSetup = this.modelsetup};
            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri){IterationSetup = this.iterationSetup};
            this.model.Iteration.Add(this.iteration);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyThatExceptionRaises1()
        {
            var vm = new ReqIfExportDialogViewModel(null, new List<Iteration>(), this.fileDialogService.Object, this.serializer.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyThatExceptionRaises2()
        {
            var vm = new ReqIfExportDialogViewModel(new List<ISession>(), null, this.fileDialogService.Object, this.serializer.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyThatExceptionRaises3()
        {
            var vm = new ReqIfExportDialogViewModel(new List<ISession>(), new List<Iteration>(), null, this.serializer.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyThatExceptionRaises4()
        {
            var vm = new ReqIfExportDialogViewModel(new List<ISession>(), new List<Iteration>(), this.fileDialogService.Object, null);
        }

        [Test]
        public void VeriyThatOkCommandWorks()
        {
            var vm = new ReqIfExportDialogViewModel(new List<ISession> { this.session.Object }, new List<Iteration> { this.iteration }, this.fileDialogService.Object, this.serializer.Object);

            this.fileDialogService.Setup(
                x => x.GetSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 1))
                .Returns("test");

            vm.SelectedIteration = vm.Iterations.First();
            Assert.IsNotNullOrEmpty(vm.SelectedIteration.IterationNumber);
            Assert.IsNotNullOrEmpty(vm.SelectedIteration.Model);
            Assert.IsNotNullOrEmpty(vm.SelectedIteration.DataSourceUri);
            Assert.IsNotNull(vm.SelectedIteration.Iteration);

            Assert.IsFalse(vm.OkCommand.CanExecute(null));
            Assert.IsTrue(vm.CancelCommand.CanExecute(null));
            Assert.IsTrue(vm.BrowseCommand.CanExecute(null));

            vm.BrowseCommand.Execute(null);
            Assert.IsNotNullOrEmpty(vm.Path);

            Assert.IsTrue(vm.OkCommand.CanExecute(null));
            vm.OkCommand.Execute(null);

            Assert.IsNotNull(vm.DialogResult);
        }
    }
}