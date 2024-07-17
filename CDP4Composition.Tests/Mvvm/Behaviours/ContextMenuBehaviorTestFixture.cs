// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContextMenuBehaviorTestFixture.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2024 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.Mvvm.Behaviours
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;

    using CDP4Dal;
    using CDP4Dal.Permission;

    using DevExpress.Xpf.Grid;

    using Moq;

    using NUnit.Framework;

    using ReactiveUI;

    /// <summary>
    /// Suite of tests for the <see cref="ContextMenuBehavior"/> class.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ContextMenuBehaviorTestFixture
    {
        private ContextMenuBehavior contextMenuBehavior;

        private GridControl gridControl;

        private Mock<ISession> session;

        private GridColumn gridColumn;

        private Mock<IPermissionService> permissionService;

        private TestBrowserViewModel testBrowserViewModel;

        private Mock<IPanelNavigationService> panelNavigation;

        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.messageBus = new CDPMessageBus();
            this.session = new Mock<ISession>();
            this.session.Setup(x => x.CDPMessageBus).Returns(this.messageBus);

            this.permissionService = new Mock<IPermissionService>();
            this.panelNavigation = new Mock<IPanelNavigationService>();
            this.permissionService.Setup(x => x.CanRead(It.IsAny<Thing>())).Returns(true);
            this.permissionService.Setup(x => x.CanWrite(It.IsAny<Thing>())).Returns(true);
            var uri = new Uri("https://www.stariongroup.eu");
            var fileType = new FileType(Guid.NewGuid(), null, uri);
            var testRdl = new SiteReferenceDataLibrary(Guid.NewGuid(), null, uri) { Name = "Test RDL" };
            testRdl.FileType.Add(fileType);
            var siteDir = new SiteDirectory(Guid.NewGuid(), null, uri) { Name = "SiteDir" };
            siteDir.SiteReferenceDataLibrary.Add(testRdl);

            this.session.Setup(x => x.OpenReferenceDataLibraries)
                .Returns(new List<SiteReferenceDataLibrary> { testRdl });

            this.session.Setup(x => x.PermissionService).Returns(this.permissionService.Object);

            this.testBrowserViewModel = new TestBrowserViewModel(
                this.session.Object,
                siteDir,
                null,
                this.panelNavigation.Object,
                null, null);

            this.gridControl = new GridControl { DataContext = this.testBrowserViewModel };
            this.gridColumn = new GridColumn { FieldName = "Name" };
            this.gridControl.Columns.Add(this.gridColumn);

            this.contextMenuBehavior = new ContextMenuBehavior();
            this.contextMenuBehavior.Attach(this.gridControl);
        }

        [TearDown]
        public void TearDown()
        {
            this.messageBus.ClearSubscriptions();
        }

        [Test]
        public void VerifyThatDetachingBehaviourDoesNotThrowException()
        {
            Assert.DoesNotThrow(() => this.contextMenuBehavior.Detach());
        }

        [Test]
        public void VerifyThatContextMenuIsPopulatedWhenMouseEventIsRaised()
        {
            var mouseButtonEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
            {
                RoutedEvent = UIElement.MouseRightButtonUpEvent,
                Source = this.gridControl
            };

            this.gridControl.RaiseEvent(mouseButtonEventArgs);

            Assert.IsTrue(this.testBrowserViewModel.ContextMenu.Any(x => x.Header == "Create a File Type"));
        }

        [Test]
        public void VerifyThatBrowserCommandsAreInitializedOnLeftMouseClickOnRow()
        {
            Assert.IsFalse(((ICommand)this.testBrowserViewModel.UpdateCommand).CanExecute(null));
            Assert.IsFalse(((ICommand)this.testBrowserViewModel.InspectCommand).CanExecute(null));

            this.testBrowserViewModel.SelectedThing = this.testBrowserViewModel.FileTypes.First();

            var rightClickEventArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                Source = this.gridControl
            };

            this.gridControl.RaiseEvent(rightClickEventArgs);

            Assert.IsTrue(((ICommand)this.testBrowserViewModel.UpdateCommand).CanExecute(null));
            Assert.IsTrue(((ICommand)this.testBrowserViewModel.InspectCommand).CanExecute(null));
        }

        internal class FileTypeRowViewModel : CDP4CommonView.FileTypeRowViewModel
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FileTypeRowViewModel"/> class. 
            /// </summary>
            /// <param name="filetype">The <see cref="FileType"/> that is represented by the current view-model</param>
            /// <param name="session">The session.</param>
            /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
            public FileTypeRowViewModel(FileType filetype, ISession session, IViewModelBase<Thing> containerViewModel)
                : base(filetype, session, containerViewModel)
            {
            }
        }

        /// <summary>
        /// The purpose of the <see cref="TestBrowserViewModel"/> is to represent a view-model for testing purposes
        /// </summary>
        internal class TestBrowserViewModel : BrowserViewModelBase<SiteDirectory>, IPanelViewModel
        {
            /// <summary>
            /// Backing field for <see cref="CanWriteFileType"/>
            /// </summary>
            private bool canWriteFileType;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestBrowserViewModel"/> class.
            /// </summary>
            /// <param name="session">the associated <see cref="ISession"/></param>
            /// <param name="siteDir">The unique <see cref="SiteDirectory"/></param>
            /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
            /// <param name="panelNavigationService">The <see cref="IPanelNavigationService"/></param>
            /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
            internal TestBrowserViewModel(
                ISession session,
                SiteDirectory siteDir,
                IThingDialogNavigationService thingDialogNavigationService,
                IPanelNavigationService panelNavigationService,
                IDialogNavigationService dialogNavigationService,
                IPluginSettingsService pluginSettingsService)
                : base(siteDir, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
            {
                this.FileTypes = new ReactiveList<FileTypeRowViewModel>();

                var fileType = new FileType();
                var rowViewModel = new FileTypeRowViewModel(fileType, session, this);
                this.FileTypes.Add(rowViewModel);
            }

            /// <summary>
            /// Gets the caption displayed by the docking panel
            /// </summary>
            public string Caption { get; private set; }

            /// <summary>
            /// Gets the ToolTip displayed by the docking panel
            /// </summary>
            public string ToolTip { get; private set; }

            /// <summary>
            /// Gets or sets a value indicating if the current person can create, edit or delete a <see cref="FileType"/>
            /// </summary>
            public bool CanWriteFileType
            {
                get => this.canWriteFileType;
                set => this.RaiseAndSetIfChanged(ref this.canWriteFileType, value);
            }

            /// <summary>
            /// Gets the <see cref="FileTypes"/> rows that are contained by this view-model
            /// </summary>
            public ReactiveList<FileTypeRowViewModel> FileTypes { get; private set; }

            /// <summary>
            /// Gets or sets the dock layout group target name to attach this panel to on opening
            /// </summary>
            public string TargetName { get; set; }

            /// <summary>
            /// Loads the <see cref="FileType"/>s from the cache when the browser is instantiated.
            /// </summary>
            protected override void Initialize()
            {
                this.CreateCommand = ReactiveCommandCreator.Create(() => this.ExecuteCreateCommand<FileType>(), this.WhenAnyValue(x => x.CanWriteFileType));
            }

            /// <summary>
            /// Compute the permissions
            /// </summary>
            public override void ComputePermission()
            {
                base.ComputePermission();
                this.CanWriteFileType = this.Session.OpenReferenceDataLibraries.Any();
            }

            /// <summary>
            /// Populate the context menu
            /// </summary>
            public override void PopulateContextMenu()
            {
                base.PopulateContextMenu();

                this.ContextMenu.Add(new ContextMenuItemViewModel("Create a File Type", "", this.CreateCommand, MenuItemKind.Create, ClassKind.FileType));
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <param name="disposing">
            /// a value indicating whether the class is being disposed of
            /// </param>
            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                foreach (var fileType in this.FileTypes)
                {
                    fileType.Dispose();
                }
            }
        }
    }
}
