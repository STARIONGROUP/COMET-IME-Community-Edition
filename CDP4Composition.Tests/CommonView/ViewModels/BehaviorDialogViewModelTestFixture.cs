// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BehaviorDialogViewModelTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.CommonView.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.MetaInfo;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.CommonView.ViewModels;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.DAL;
    using CDP4Dal.Operations;
    using CDP4Dal.Permission;

    using Microsoft.Practices.ServiceLocation;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    [TestFixture]
    public class BehaviorDialogViewModelTestFixture
    {
        private Mock<IPermissionService> permissionService;
        private Behavior behavior;
        private ThingTransaction transaction;
        private Mock<ISession> session;
        private Mock<IServiceLocator> serviceLocator;
        private Mock<IThingDialogNavigationService> navigation;
        private SiteDirectory siteDirectory;
        private Mock<IDal> dal;
        private ElementDefinition container;
        private Mock<IThingSelectorDialogService> thingSelectorDialogService;
        private Mock<IOpenSaveFileDialogService> fileDialogService;

        public EngineeringModelSetup engineeringModelSetup { get; private set; }

        private EngineeringModel model;
        private IterationSetup iterationSetup;
        private Iteration iteration;
        private Assembler assembler;
        private Uri uri = new Uri("http://test.com");
        private string localPath;
        private FileType fileType1;
        private FileType fileType2;
        private SiteReferenceDataLibrary srdl;
        private ModelReferenceDataLibrary mrdl;

        [SetUp]
        public void Setup()
        {
            this.dal = new Mock<IDal>();
            this.dal.Setup(x => x.MetaDataProvider).Returns(new MetaDataProvider());

            RxApp.MainThreadScheduler = Scheduler.CurrentThread;
            this.serviceLocator = new Mock<IServiceLocator>();
            this.navigation = new Mock<IThingDialogNavigationService>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingDialogNavigationService>()).Returns(this.navigation.Object);

            this.permissionService = new Mock<IPermissionService>();
            this.behavior = new Behavior(Guid.NewGuid(), null, null); 
            this.siteDirectory = new SiteDirectory(Guid.NewGuid(), null, null);

            this.container = new ElementDefinition(Guid.NewGuid(), null, null);

            var transactionContext = TransactionContextResolver.ResolveContext(this.siteDirectory);
            this.transaction = new ThingTransaction(transactionContext, this.container);

            this.session = new Mock<ISession>();
            this.session.Setup(x => x.DalVersion).Returns(new Version(1, 1, 0));
            this.session.Setup(x => x.Dal).Returns(this.dal.Object);

            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.thingSelectorDialogService = new Mock<IThingSelectorDialogService>();

            this.serviceLocator = new Mock<IServiceLocator>();
            ServiceLocator.SetLocatorProvider(() => this.serviceLocator.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IOpenSaveFileDialogService>()).Returns(this.fileDialogService.Object);
            this.serviceLocator.Setup(x => x.GetInstance<IThingSelectorDialogService>()).Returns(this.thingSelectorDialogService.Object);

            this.assembler = new Assembler(this.uri);
            this.session.Setup(x => x.Assembler).Returns(this.assembler);
            this.session.Setup(x => x.DataSourceUri).Returns(this.uri.ToString);
            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.fileType1 = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "jpg" };
            this.fileType2 = new FileType(Guid.NewGuid(), this.assembler.Cache, this.uri) { Extension = "zip" };

            this.thingSelectorDialogService.Setup(x => x.SelectThing(It.IsAny<IEnumerable<FileType>>(), It.IsAny<IEnumerable<string>>())).Returns(this.fileType1);

            this.srdl = new SiteReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.mrdl = new ModelReferenceDataLibrary(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                RequiredRdl = this.srdl,
                FileType = { this.fileType1, this.fileType2 }
            };

            this.engineeringModelSetup = new EngineeringModelSetup(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                Name = "model"
            };

            this.model = new EngineeringModel(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                EngineeringModelSetup = this.engineeringModelSetup
            };

            this.engineeringModelSetup.RequiredRdl.Add(this.mrdl);

            this.iterationSetup = new IterationSetup(Guid.NewGuid(), this.assembler.Cache, this.uri);

            this.iteration = new Iteration(Guid.NewGuid(), this.assembler.Cache, this.uri)
            {
                IterationSetup = this.iterationSetup,
                Container = this.engineeringModelSetup
            };

            this.model.Iteration.Add(this.iteration);

            this.localPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "attachment");
            System.IO.File.Create(this.localPath).Close();
        }

        [TearDown]
        public void TearDown()
        {
            System.IO.File.Delete(this.localPath);
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [TestCase(BehavioralModelKind.File, ExpectedResult = typeof(AttachmentViewModel))]
        [TestCase(BehavioralModelKind.CSharp, ExpectedResult = typeof(ScriptKindViewModel))]
        [TestCase(BehavioralModelKind.Lua, ExpectedResult = typeof(ScriptKindViewModel))]
        [TestCase(BehavioralModelKind.Python, ExpectedResult = typeof(ScriptKindViewModel))]
        [TestCase(BehavioralModelKind.Other, ExpectedResult = typeof(ScriptKindViewModel))]
        public Type VerifySelectingModelKindCreatesTheCorrectViewModelType(BehavioralModelKind behavioralModelKind)
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container,  this.model.Iteration);

            vm.SelectedBehavioralModelKind = behavioralModelKind;

            return vm.SelectedBehavioralModelKindViewModel.GetType();
        }

        [Test]
        public void VerifyBehaviorDialogViewModelContainsExpectedModelKinds()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            Assert.That(vm.BehavioralModelKinds, Is.EquivalentTo(new[] { BehavioralModelKind.File, BehavioralModelKind.CSharp, BehavioralModelKind.Lua, BehavioralModelKind.Other, BehavioralModelKind.Python }));
        }

        [Test]
        public void VerifyWhenBehaviorWithScriptHasAllDetailsCanExecuteIsTrue()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.CSharp;
            var scriptViewModel = (ScriptKindViewModel)vm.SelectedBehavioralModelKindViewModel;
            scriptViewModel.Script = "A script blah blah";

            Assert.That(vm.OkCanExecute, Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("       ")]
        public void VerifyWhenNoScriptCanNotExecuteOkCommand(string noScript)
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.CSharp;
            var scriptViewModel = (ScriptKindViewModel)vm.SelectedBehavioralModelKindViewModel;
            scriptViewModel.Script = noScript;

            Assert.That(vm.OkCanExecute, Is.False);
        }

        [Test]
        public void VErifyNewParameterIsAddedCorrectly()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.CSharp;
            var scriptViewModel = (ScriptKindViewModel)vm.SelectedBehavioralModelKindViewModel;
            scriptViewModel.Script = "script";

            scriptViewModel.AddParameterCommand.Execute(null);

            var addedParameter = scriptViewModel.BehaviorParameter.Single();

            Assert.That(addedParameter.VariableName, Is.Null);
            Assert.That(addedParameter.Parameter, Is.Null);
            Assert.That(addedParameter.ParameterKind, Is.EqualTo(BehavioralParameterKind.Input));
            Assert.That(vm.OkCanExecute, Is.False);
        }

        [Test]
        public void VerifyWhenBehaviorWithScriptAndParameterHasAllDetailsCanExecuteIsTrue()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.CSharp;
            var scriptViewModel = (ScriptKindViewModel)vm.SelectedBehavioralModelKindViewModel;
            scriptViewModel.Script = "script";

            scriptViewModel.AddParameterCommand.Execute(null);

            var addedParameter = scriptViewModel.BehaviorParameter.Single();
            addedParameter.VariableName = "Var";
            addedParameter.Parameter = new Parameter(Guid.NewGuid(), null, null);

            Assert.That(vm.OkCanExecute, Is.True);
        }

        [Test]
        public void VerifyParameterWithoutVariableCanExecuteIsFalse()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.CSharp;
            var scriptViewModel = (ScriptKindViewModel)vm.SelectedBehavioralModelKindViewModel;
            scriptViewModel.Script = "script";

            scriptViewModel.AddParameterCommand.Execute(null);

            var addedParameter = scriptViewModel.BehaviorParameter.Single();
            addedParameter.VariableName = string.Empty;
            addedParameter.Parameter = new Parameter(Guid.NewGuid(), null, null);

            Assert.That(vm.OkCanExecute, Is.False);
        }

        [Test]
        public void VerifyBehavioralParameterWithoutParameterCanExecuteIsFalse()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.CSharp;
            var scriptViewModel = (ScriptKindViewModel)vm.SelectedBehavioralModelKindViewModel;
            scriptViewModel.Script = "script";

            scriptViewModel.AddParameterCommand.Execute(null);

            var addedParameter = scriptViewModel.BehaviorParameter.Single();
            addedParameter.VariableName = "Var";
            addedParameter.Parameter = null;

            Assert.That(vm.OkCanExecute, Is.False);
        }

        [Test]
        public void VerifyWhenBehaviorWithAttachmentHasAllDetailsCanExecuteIsTrue()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.File;
            var attachmentViewModel = (AttachmentViewModel)vm.SelectedBehavioralModelKindViewModel;

            attachmentViewModel.FileName = "SomePath";
            attachmentViewModel.ContentHash = "$$HASH$$";
            attachmentViewModel.LocalPath = this.localPath;
            attachmentViewModel.FileType.Add(new FileType(Guid.NewGuid(), null, null));

            Assert.That(vm.OkCanExecute, Is.True);
        }

        [Test]
        public void VerifyAttachmentViewModelHasCorrectFileTypes()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.File;
            var attachmentViewModel = (AttachmentViewModel)vm.SelectedBehavioralModelKindViewModel;

            attachmentViewModel.FileName = "SomePath";
            attachmentViewModel.AddFileTypeCommand.Execute(null);

            Assert.That(attachmentViewModel.FileType, Is.EquivalentTo(new[] { this.fileType1 }));
            Assert.That(attachmentViewModel.Path, Is.EqualTo("SomePath.jpg"));
        }

        [Test]
        public void VerifyAddFileCommand()
        {
            this.fileDialogService.Setup(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(new string[] { this.localPath });

            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.File;
            var attachmentViewModel = (AttachmentViewModel)vm.SelectedBehavioralModelKindViewModel;

            attachmentViewModel.AddFileCommand.Execute(null);

            Assert.That(attachmentViewModel.FileName, Is.EqualTo("attachment"));
            Assert.That(attachmentViewModel.LocalPath, Is.EqualTo(this.localPath));
        }

        [Test]
        public void VerifyAttachmentViewModelHasCorrectPossibleFileTypes()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);

            vm.Name = "AnyName";
            vm.ShortName = "Short";
            vm.SelectedBehavioralModelKind = BehavioralModelKind.File;
            var attachmentViewModel = (AttachmentViewModel)vm.SelectedBehavioralModelKindViewModel;

            Assert.That(attachmentViewModel.PossibleFileType, Is.EquivalentTo(new[] { this.fileType1, this.fileType2 }));
        }

        [Test]
        public void VerifyDeleteFileType()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);
            vm.SelectedBehavioralModelKind = BehavioralModelKind.File;
            var attachmentViewModel = (AttachmentViewModel)vm.SelectedBehavioralModelKindViewModel;

            attachmentViewModel.FileType.Add(new FileType());

            attachmentViewModel.SelectedFileType = attachmentViewModel.FileType.First();
            Assert.That(attachmentViewModel.CanDeleteFileType, Is.True);

            attachmentViewModel.DeleteFileTypeCommand.Execute(null);
            Assert.That(attachmentViewModel.FileType, Is.Empty);
        }

        [Test]
        public void VerifyMoveDownFileType()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);
            vm.SelectedBehavioralModelKind = BehavioralModelKind.File;
            var attachmentViewModel = (AttachmentViewModel)vm.SelectedBehavioralModelKindViewModel;

            Assert.That(attachmentViewModel.CanMoveDownFileType, Is.False);

            attachmentViewModel.FileType.Add(new FileType());
            Assert.That(attachmentViewModel.CanMoveDownFileType, Is.False);

            attachmentViewModel.SelectedFileType = attachmentViewModel.FileType.First();
            Assert.That(attachmentViewModel.CanMoveDownFileType, Is.False);

            attachmentViewModel.FileType.Add(new FileType());
            Assert.That(attachmentViewModel.CanMoveDownFileType, Is.True);

            attachmentViewModel.SelectedFileType = attachmentViewModel.FileType.Last();
            Assert.That(attachmentViewModel.CanMoveDownFileType, Is.False);

            attachmentViewModel.SelectedFileType = attachmentViewModel.FileType.First();
            attachmentViewModel.MoveDownFileTypeCommand.Execute(null);
            Assert.That(attachmentViewModel.SelectedFileType, Is.EqualTo(attachmentViewModel.FileType.Last()));
        }

        [Test]
        public void VerifyMoveUpFileType()
        {
            var vm = new BehaviorDialogViewModel(this.behavior, this.transaction, this.session.Object, false, ThingDialogKind.Create, this.navigation.Object, this.container, this.model.Iteration);
            vm.SelectedBehavioralModelKind = BehavioralModelKind.File;
            var attachmentViewModel = (AttachmentViewModel)vm.SelectedBehavioralModelKindViewModel;

            Assert.That(attachmentViewModel.CanMoveUpFileType, Is.False);

            attachmentViewModel.FileType.Add(new FileType());
            Assert.That(attachmentViewModel.CanMoveUpFileType, Is.False);

            attachmentViewModel.SelectedFileType = attachmentViewModel.FileType.First();
            Assert.That(attachmentViewModel.CanMoveUpFileType, Is.False);

            attachmentViewModel.FileType.Add(new FileType());
            Assert.That(attachmentViewModel.CanMoveUpFileType, Is.False);

            attachmentViewModel.SelectedFileType = attachmentViewModel.FileType.Last();
            Assert.That(attachmentViewModel.CanMoveUpFileType, Is.True);

            attachmentViewModel.MoveUpFileTypeCommand.Execute(null);
            Assert.That(attachmentViewModel.SelectedFileType, Is.EqualTo(attachmentViewModel.FileType.First()));
        }
    }
}
