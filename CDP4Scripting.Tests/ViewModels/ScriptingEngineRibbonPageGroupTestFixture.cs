// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptingEngineRibbonPageGroupTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2023 RHEA System S.A.
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

namespace CDP4Scripting.Tests.ViewModels
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using CDP4Composition;
    using CDP4Composition.Composition;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;

    using CDP4Dal;
    
    using CDP4Scripting.Events;
    using CDP4Scripting.Interfaces;
    using CDP4Scripting.ViewModels;

    using ICSharpCode.AvalonEdit;
    
    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ScriptingEngineRibbonPageGroup"/>
    /// </summary>
    [TestFixture, Apartment(ApartmentState.STA)]
    public class ScriptingEngineRibbonPageGroupTestFixture
    {
        private ScriptingEngineRibbonPageGroupViewModel scriptingEngineRibbonPageGroupViewModel;
        private Mock<IPanelNavigationService> panelNavigationService;
        private Mock<IOpenSaveFileDialogService> fileDialogService;
        private Mock<IScriptingProxy> scriptingProxy;
        private Mock<ScriptPanelViewModel> scriptPanelViewModel;
        private ReactiveList<ISession> openSessions;
        private string filePathOpenTest = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.py");
        private readonly string filePathSaveTest = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.py");
        private readonly string filePath2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "testFile2.py");

        [SetUp]
        public void SetUp()
        {
            RxApp.DefaultExceptionHandler = new RxAppObservableExceptionHandler();
            RxApp.MainThreadScheduler = Scheduler.CurrentThread;

            this.openSessions = new ReactiveList<ISession>();
            this.panelNavigationService = new Mock<IPanelNavigationService>();
            this.scriptingProxy = new Mock<IScriptingProxy>();

            var avalonEditor = new TextEditor();
            avalonEditor.Text = "Content of the editor";
            this.scriptPanelViewModel = new Mock<ScriptPanelViewModel>("header", this.scriptingProxy.Object, "*.py", openSessions);
            this.scriptPanelViewModel.SetupProperty(x => x.AvalonEditor, avalonEditor);

            this.fileDialogService = new Mock<IOpenSaveFileDialogService>();
            this.fileDialogService.Setup(x => x.GetOpenFileDialog(true, true, false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4)).Returns((string[])null);

            this.scriptingEngineRibbonPageGroupViewModel = new ScriptingEngineRibbonPageGroupViewModel(this.panelNavigationService.Object, this.fileDialogService.Object, this.scriptingProxy.Object);
        }

        [TearDown]
        public void TearDown()
        {
            CDPMessageBus.Current.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatNullParametersThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new ScriptingEngineRibbonPageGroupViewModel(null, this.fileDialogService.Object, this.scriptingProxy.Object));
            Assert.Throws<ArgumentNullException>(() => new ScriptingEngineRibbonPageGroupViewModel(this.panelNavigationService.Object, null, this.scriptingProxy.Object));
            Assert.Throws<ArgumentNullException>(() => new ScriptingEngineRibbonPageGroupViewModel(this.panelNavigationService.Object, this.fileDialogService.Object, null));
        }

        [Test]
        public void VerifyThatNewScriptCommandsWork()
        {
            Assert.DoesNotThrowAsync(async () => await this.scriptingEngineRibbonPageGroupViewModel.NewPythonScriptCommand.Execute());
            this.panelNavigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Once);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 1);
            var scriptViewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(0);
            Assert.AreEqual(scriptViewModel.Caption, "python0");
            Assert.AreEqual(scriptViewModel.FileExtension, "*.py");

            Assert.DoesNotThrowAsync(async () => await this.scriptingEngineRibbonPageGroupViewModel.NewPythonScriptCommand.Execute());
            this.panelNavigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(2));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 2);
            scriptViewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(1);
            Assert.AreEqual(scriptViewModel.Caption, "python1");
            Assert.AreEqual(scriptViewModel.FileExtension, "*.py");

            Assert.DoesNotThrowAsync(async () => await this.scriptingEngineRibbonPageGroupViewModel.NewTextScriptCommand.Execute());
            this.panelNavigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(3));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 3);
            scriptViewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(2);
            Assert.AreEqual(scriptViewModel.Caption, "text2");
            Assert.AreEqual(scriptViewModel.FileExtension, "*.txt");

            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.RemoveAt(1);
            Assert.DoesNotThrowAsync(async () => await this.scriptingEngineRibbonPageGroupViewModel.NewTextScriptCommand.Execute());
            this.panelNavigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(4));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 3);
            scriptViewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(2);
            Assert.AreEqual(scriptViewModel.Caption, "text3");
            Assert.AreEqual(scriptViewModel.FileExtension, "*.txt");
        }

        [Test]
        public void VerifyThatSaveScriptWorks()
        {
            // Initialization of a script panel view model
            var editor = new TextEditor();
            editor.Text = "some text";
            var panelVM = new Mock<IScriptPanelViewModel>();
            panelVM.SetupProperty(x => x.Caption, "header");
            panelVM.SetupProperty(x => x.AvalonEditor, editor);

            // The fileDialogService.GetSaveFileDialog should be called for the first time and return a valid path. 
            // A couple should be added in the dictionary to store the path of the file associated to the panel saved.
            this.fileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), null, ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1)).Returns(this.filePathSaveTest);
            var scriptSaved = new ScriptPanelEvent(panelVM.Object, ScriptPanelStatus.Saved);
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(scriptSaved));
            this.fileDialogService.Verify(x => x.GetSaveFileDialog(It.IsAny<string>(), null, ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1), Times.Once);
            Assert.IsTrue(File.Exists(this.filePathSaveTest));
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.py"));
            string result;
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.py", out result);
            Assert.AreEqual(this.filePathSaveTest, result);

            // The path of the file associated to the script panel is already stored in the dictionnary, the file should be overwritten
            panelVM.SetupProperty(x => x.Caption, "test.py*");
            editor.Text = "new content";
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(scriptSaved));
            var content = File.ReadAllText(this.filePathSaveTest);
            Assert.AreEqual("new content", content);

            // The file has been deleted, the dictionary should be updated with the new value of the path.
            File.Delete(this.filePathSaveTest);
            this.fileDialogService.Setup(x => x.GetSaveFileDialog("test.py", null, It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(this.filePath2);
            Assert.DoesNotThrow(() => CDPMessageBus.Current.SendMessage(scriptSaved));
            this.fileDialogService.Verify(x => x.GetSaveFileDialog("test.py", null, ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1), Times.Once);
            Assert.IsTrue(File.Exists(this.filePath2));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("testFile2.py", out result);
            Assert.AreEqual(this.filePath2, result);
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsValue(this.filePathSaveTest));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.py"));

            // The fileDialogService.GetSaveFileDialog should be called once and returns "" that leads to an exception.
            panelVM.SetupProperty(x => x.Caption, "new header");
            this.fileDialogService.Setup(x => x.GetSaveFileDialog(It.IsAny<string>(), null, It.IsAny<string>(), It.IsAny<string>(), 1)).Returns("");
            Assert.Throws<ArgumentNullException>(() => CDPMessageBus.Current.SendMessage(scriptSaved));
            this.fileDialogService.Verify(x => x.GetSaveFileDialog("new header", It.IsAny<string>(), ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1), Times.Once);

            // The button "save all" has been pressed, the 2 scripts should be saved
            // The first one has been saved previously, the content of the file associated should be overwritten
            // The second one has never been saved, a new file should be created
            // A couple should be added to the dictionary to store the path of the second file.
            panelVM.SetupProperty(x => x.Caption, "testFile2.py*");
            editor.Text = "content of the testFile2.py file";
            var panelVM2 = new Mock<IScriptPanelViewModel>();
            panelVM2.SetupProperty(x => x.Caption, "python*");
            panelVM2.SetupProperty(x => x.AvalonEditor, editor);
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Clear();
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panelVM.Object);
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panelVM2.Object);
            var pythonFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.py");
            this.fileDialogService.Setup(x => x.GetSaveFileDialog("python", null, It.IsAny<string>(), It.IsAny<string>(), 1)).Returns(pythonFilePath);
            Assert.DoesNotThrowAsync(async () => await this.scriptingEngineRibbonPageGroupViewModel.SaveAllCommand.Execute());
            content = File.ReadAllText(this.filePath2);
            Assert.AreEqual("content of the testFile2.py file", content);
            this.fileDialogService.Verify(x => x.GetSaveFileDialog("python", null, ScriptingEngineRibbonPageGroupViewModel.DialogFilters, It.IsAny<string>(), 1), Times.Once);
            Assert.IsTrue(File.Exists(pythonFilePath));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.py", out result);
            Assert.AreEqual(pythonFilePath, result);
        }

        [Test]
        public void VerifyFindFilterIndexWorks()
        {
            Assert.That(this.scriptingEngineRibbonPageGroupViewModel.FindFilterIndex("*.py"), Is.EqualTo(1));
            Assert.That(this.scriptingEngineRibbonPageGroupViewModel.FindFilterIndex("*.txt"), Is.EqualTo(2));
            Assert.That(this.scriptingEngineRibbonPageGroupViewModel.FindFilterIndex("*.jar"), Is.EqualTo(1));
        }

        [Test]
        public async Task VerifyThatOpenScriptFileWorks()
        {
            // No path returned by the fileDialogService.GetOpenFileDialog, the method should return at the first conditionnal statement 
            Assert.DoesNotThrowAsync(async () => await this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute());
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Once);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 0);

            // The file is already opened in the scripting engine, the method should return without open a new panel
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Add("tab1", "C\\Users\\test.py");
            string[] paths = { "C\\Users\\test.py" };
            this.fileDialogService.Setup(x => x.GetOpenFileDialog(false, false, true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4)).Returns(paths);
            await this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute();
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(2));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 0);

            // The path returns a directory. An exception should be trhown and no panel should be created. 
            paths[0] = "C\\Users";
            Assert.ThrowsAsync<NotSupportedException>(async () => await this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute());
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(3));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 0);

            // The path returns a filename with an invalid extension. An exception should be trhown and no panel should be created. 
            paths[0] = "C\\Users\\test.jar";
            Assert.ThrowsAsync<NotSupportedException>(async () => await this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute());
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(4));
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 0);

            // The path returns a filename with a lua extension, the panel navigation service should open a panel.
            // The collection of script panels should add the new panel.
            // The dictionary should add the path of the file opened.
            File.WriteAllText(this.filePathOpenTest, "content of the python file");
            paths[0] = this.filePathOpenTest;
            Assert.DoesNotThrowAsync( async () => await this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute());
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(5));
            this.panelNavigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Once);
            var viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(0);
            Assert.AreEqual("content of the python file", viewModel.AvalonEditor.Text);
            Assert.AreEqual("test.py", viewModel.Caption);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.py"));
            string result;
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.py", out result);
            Assert.AreEqual(this.filePathOpenTest, result);

            this.scriptPanelViewModel.SetupProperty(x => x.Caption, "test.py");

            // The path returns a filename with a python extension.
            // The collection of script panels should add the new panels.
            // The dictionary should add the path of the file opened.
            this.filePathOpenTest = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.py");
            File.WriteAllText(this.filePathOpenTest, "content of the python file");
            paths[0] = this.filePathOpenTest;
            Assert.DoesNotThrowAsync(async () => await this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute());
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(6));
            this.panelNavigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(1));
            viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(0);
            Assert.AreEqual("content of the python file", viewModel.AvalonEditor.Text);
            Assert.AreEqual("test.py", viewModel.Caption);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.py"));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.py", out result);
            Assert.AreEqual(this.filePathOpenTest, result);

            // The path returns a filename with a text extension, the tabItemFactory.CreateTabItem should be called once.
            // The collection of tab items should add the new tab item.
            // The dictionary should add the path of the file opened.
            this.filePathOpenTest = Path.Combine(TestContext.CurrentContext.TestDirectory, "test.txt");
            File.WriteAllText(this.filePathOpenTest, "content of the text file");
            paths[0] = this.filePathOpenTest;
            Assert.DoesNotThrowAsync(async () => await this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute());
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(7));
            this.panelNavigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(2));
            viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(1);
            Assert.AreEqual("content of the text file", viewModel.AvalonEditor.Text);
            Assert.AreEqual("test.txt", viewModel.Caption);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test.txt"));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test.txt", out result);
            Assert.AreEqual(this.filePathOpenTest, result);

            // The method GetOpenFileDialog returns 3 paths
            // 1 of the 3 panels is already open, therefore only 2 panels should be created
            var filePathOpenTest2 = Path.Combine(TestContext.CurrentContext.TestDirectory, "test2.txt");
            File.WriteAllText(filePathOpenTest2, "content of the text file 2");
            var filePathOpenTest3 = Path.Combine(TestContext.CurrentContext.TestDirectory, "test2.py");
            File.WriteAllText(filePathOpenTest3, "content of the python file 2");
            paths = new[] {this.filePathOpenTest, filePathOpenTest2, filePathOpenTest3};
            this.fileDialogService.Setup(x => x.GetOpenFileDialog(false, false, true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4)).Returns(paths);
            Assert.DoesNotThrowAsync(async () => await this.scriptingEngineRibbonPageGroupViewModel.OpenScriptCommand.Execute());
            this.fileDialogService.Verify(x => x.GetOpenFileDialog(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 4), Times.Exactly(8));
            this.panelNavigationService.Verify(x => x.OpenInDock(It.IsAny<IPanelViewModel>()), Times.Exactly(4));
            viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(2);
            Assert.AreEqual("test2.txt", viewModel.Caption);
            Assert.AreEqual("content of the text file 2", viewModel.AvalonEditor.Text);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test2.txt"));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test2.txt", out result);
            Assert.AreEqual(filePathOpenTest2, result);
            viewModel = this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.ElementAt(3);
            Assert.AreEqual("test2.py", viewModel.Caption);
            Assert.AreEqual("content of the python file 2", viewModel.AvalonEditor.Text);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("test2.py"));
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.TryGetValue("test2.py", out result);
            Assert.AreEqual(filePathOpenTest3, result);
        }

        [Test]
        public void VerifyThatHandleClosedPanelWorks()
        {
            var avalonEditor = new TextEditor();

            var panel1 = new Mock<ScriptPanelViewModel>("panel 1", this.scriptingProxy.Object, "*.py", this.openSessions, true);
            panel1.As<IPanelViewModel>();
            panel1.As<IScriptPanelViewModel>().SetupProperty(x => x.Caption, "panel 1");
            panel1.SetupProperty(x => x.AvalonEditor, avalonEditor);

            var panel2 = new Mock<ScriptPanelViewModel>("panel 2", this.scriptingProxy.Object, "*.py", this.openSessions, true);
            panel2.As<IPanelViewModel>();
            panel2.As<IScriptPanelViewModel>().SetupProperty(x => x.Caption, "panel 2");
            panel2.SetupProperty(x => x.AvalonEditor, avalonEditor);
            
            var panel3 = new Mock<ScriptPanelViewModel>("panel 3", this.scriptingProxy.Object, "*.py", this.openSessions, true);
            panel3.As<IPanelViewModel>();
            panel3.As<IScriptPanelViewModel>().SetupProperty(x => x.Caption, "panel 3");
            panel3.SetupProperty(x => x.AvalonEditor, avalonEditor);

            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panel1.Object);
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panel2.Object);
            this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Add(panel3.Object);

            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Add("panel 1", "path panel 1");
            this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Add("panel 3", "path panel 3");

            var panelView = new Mock<IPanelView>();

            var navigationPanelEvent = new NavigationPanelEvent(panel3.Object, panelView.Object, PanelStatus.Closed);
            CDPMessageBus.Current.SendMessage(navigationPanelEvent);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 2);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Count, 1);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel1.Object));
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel2.Object));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel3.Object));
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("panel 1"));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("panel 3"));

            navigationPanelEvent = new NavigationPanelEvent(panel1.Object, panelView.Object, PanelStatus.Closed);
            CDPMessageBus.Current.SendMessage(navigationPanelEvent);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Count, 1);
            Assert.AreEqual(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.Count, 0);
            Assert.IsTrue(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel2.Object));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.CollectionScriptPanelViewModels.Contains(panel1.Object));
            Assert.IsFalse(this.scriptingEngineRibbonPageGroupViewModel.PathScriptingFiles.ContainsKey("panel 1"));
        }
    }
}
