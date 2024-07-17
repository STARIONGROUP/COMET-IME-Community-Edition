// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileRevisionDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.Services;
    using CDP4Composition.Views;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using CommonServiceLocator;

    using ReactiveUI;

    using File = CDP4Common.EngineeringModelData.File;

    /// <summary>
    /// The purpose of the <see cref="FolderDialogViewModel"/> is to allow a <see cref="Folder"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Folder"/> will result in an <see cref="Folder"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.FileRevision)]
    public class FileRevisionDialogViewModel : CDP4CommonView.FileRevisionDialogViewModel, IThingDialogViewModel, IDownloadFileViewModel
    {
        /// <summary>
        /// Backing field for <see cref="CanDownloadFile"/>
        /// </summary>
        private bool canDownloadFile;

        /// <summary>
        /// Backing field for <see cref="CanDownloadFile"/>
        /// </summary>
        private bool canAddFile;

        /// <summary>
        /// Backing field for <see cref="CanAddFileType"/>
        /// </summary>
        private bool canAddFileType;

        /// <summary>
        /// Backing field for <see cref="CanDeleteFileType"/>
        /// </summary>
        private bool canDeleteFileType;

        /// <summary>
        /// Backing field for <see cref="CanMoveUpFileType"/>
        /// </summary>
        private bool canMoveUpFileType;

        /// <summary>
        /// Backing field for <see cref="CanMoveDownFileType"/>
        /// </summary>
        private bool canMoveDownFileType;

        /// <summary>
        /// Backing field for <see cref="SelectedFileType"/>
        /// </summary>
        private FileType selectedFileType;

        /// <summary>
        /// Backing field for <see cref="LocalPath"/>
        /// </summary>
        private string localPath;

        /// <summary>
        /// The backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// The backing field for <see cref="Path"/>
        /// </summary>
        private string path;

        /// <summary>
        /// Backing field for <see cref="IsCancelButtonVisible"/>
        /// </summary>
        private bool isCancelButtonVisible;

        /// <summary>
        /// Backing field for <see cref="LoadingMessage"/>
        /// </summary>
        private string loadingMessage;

        /// <summary>
        /// The <see cref="IThingSelectorDialogService"/>
        /// </summary>
        private readonly IThingSelectorDialogService thingSelectorDialogService = ServiceLocator.Current.GetInstance<IThingSelectorDialogService>();

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private readonly IOpenSaveFileDialogService fileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

        /// <summary>
        /// The (injected) <see cref="IDownloadFileService"/>
        /// </summary>
        private IDownloadFileService downloadFileService = ServiceLocator.Current.GetInstance<IDownloadFileService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRevisionDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public FileRevisionDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRevisionDialogViewModel"/> class.
        /// </summary>
        /// <param name="fileRevision">
        /// The <see cref="FileRevision"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name=""></param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="FileRevisionDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="FileRevisionDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> of the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public FileRevisionDialogViewModel(FileRevision fileRevision, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind,
            IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(fileRevision, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if (dialogKind == ThingDialogKind.Update)
            {
                throw new InvalidOperationException($"{nameof(ThingDialogKind.Update)} on a {nameof(FileRevision)} is not allowed.");
            }

            this.Disposables.Add(
                this.WhenAnyValue(x => x.Name, x => x.ContentHash)
                    .Subscribe(_ =>
                    {
                        this.UpdateOkCanExecute();
                        this.UpdatePath();
                    }));

            this.Disposables.Add(
                this.WhenAnyValue(x => x.SelectedFileType, x => x.FileType)
                    .Subscribe(_ =>
                    {
                        this.CanDeleteFileType = !this.IsReadOnly && (this.SelectedFileType != null);
                        this.AfterUpdateFileType();
                    }));

            this.Disposables.Add(this.FileType.Changed.Subscribe(_ => this.AfterUpdateFileType()));

            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedContainingFolder).Subscribe(_ => this.UpdatePath()));

            this.Disposables.Add(this.WhenAnyValue(x => x.LocalPath).Subscribe(async _ =>
            {
                this.UpdateOkCanExecute();
                this.SetContentHash();
            }));
        }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        [ValidationOverride(true, "FileRevisionName")]
        public override string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Calculates and sets the <see cref="ContentHash"/> property accordingly
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private async Task SetContentHash()
        {
            this.ContentHash = null;
            this.ContentHash = this.CalculateContentHash(this.localPath);
        }

        /// <summary>
        /// Gets or sets the <see cref="LocalPath"/> property
        /// </summary>
        public string LocalPath
        {
            get => this.localPath;
            set => this.RaiseAndSetIfChanged(ref this.localPath, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="SelectedFileType"/> property
        /// </summary>
        public FileType SelectedFileType
        {
            get => this.selectedFileType;
            set => this.RaiseAndSetIfChanged(ref this.selectedFileType, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="CanDownloadFile"/> property
        /// </summary>
        public bool CanDownloadFile
        {
            get => this.canDownloadFile;
            private set => this.RaiseAndSetIfChanged(ref this.canDownloadFile, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="CanAddFile"/> property
        /// </summary>
        public bool CanAddFile
        {
            get => this.canAddFile;
            private set => this.RaiseAndSetIfChanged(ref this.canAddFile, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="CanAddFileType"/> property
        /// </summary>
        public bool CanAddFileType
        {
            get => this.canAddFileType;
            private set => this.RaiseAndSetIfChanged(ref this.canAddFileType, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="CanDeleteFileType"/> property
        /// </summary>
        public bool CanDeleteFileType
        {
            get => this.canDeleteFileType;
            private set => this.RaiseAndSetIfChanged(ref this.canDeleteFileType, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="CanMoveUpFileType"/> property
        /// </summary>
        public bool CanMoveUpFileType
        {
            get => this.canMoveUpFileType;
            private set => this.RaiseAndSetIfChanged(ref this.canMoveUpFileType, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="CanMoveDownFileType"/> property
        /// </summary>
        public bool CanMoveDownFileType
        {
            get => this.canMoveDownFileType;
            private set => this.RaiseAndSetIfChanged(ref this.canMoveDownFileType, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="Path"/> property
        /// </summary>
        public string Path
        {
            get => this.path;
            private set => this.RaiseAndSetIfChanged(ref this.path, value);
        }

        /// <summary>
        /// Gets a value indicating whether the Cancel button is visible on the <see cref="LoadingControl"/>
        /// </summary>
        public bool IsCancelButtonVisible
        {
            get => this.isCancelButtonVisible;
            set => this.RaiseAndSetIfChanged(ref this.isCancelButtonVisible, value);
        }

        /// <summary>
        /// Gets a value the message text on the <see cref="LoadingControl"/>
        /// </summary>
        public string LoadingMessage
        {
            get => this.loadingMessage;
            set => this.RaiseAndSetIfChanged(ref this.loadingMessage, value);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to download a file to a locally available drive
        /// </summary>
        public ReactiveCommand<Unit, Unit> DownloadFileCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to add a physical file to the <see cref="FileRevision"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddFileCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to add a <see cref="FileType"/> to this <see cref="FileRevision"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddFileTypeCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to remove a <see cref="FileType"/> from this <see cref="FileRevision"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteFileTypeCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to move a <see cref="FileType"/> down in the ordering of <see cref="FileType"/>s
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveUpFileTypeCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to to move a <see cref="FileType"/> up in the ordering of <see cref="FileType"/>s
        /// </summary>
        public ReactiveCommand<Unit, Unit> MoveDownFileTypeCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to cancel download of a file
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelDownloadCommand { get; private set; }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.localPath = this.Thing.LocalPath;

            if (this.Container is File file)
            {
                this.SelectedContainingFolder = file.CurrentContainingFolder;
            }

            this.PopulateFileType();

            this.CreatedOn = this.Thing.CreatedOn.Equals(DateTime.MinValue) ? DateTime.UtcNow : this.Thing.CreatedOn;

            if (this.SelectedCreator == null)
            {
                var iteration = this.Container.GetContainerOfType<Iteration>();

                if (iteration == null)
                {
                    var engineeringModel = this.Container.GetContainerOfType<EngineeringModel>();

                    if (engineeringModel != null)
                    {
                        var dictionary = this.Session.OpenIterations.FirstOrDefault(x => x.Key.Container == engineeringModel);

                        if (dictionary.Key != null)
                        {
                            this.SelectedCreator = dictionary.Value.Item2;
                        }
                    }
                }
                else
                {
                    this.Session.OpenIterations.TryGetValue(iteration, out var tuple);
                    this.SelectedCreator = tuple?.Item2;
                }
            }
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            this.PopulatePossibleFileType();

            var isOwner = false;

            if (this.Container is File file)
            {
                if (file.Container is CommonFileStore)
                {
                    isOwner = true;
                }
                else
                {
                    var iteration = this.Container.GetContainerOfType<Iteration>();

                    if (iteration != null)
                    {
                        isOwner = this.Session.QueryDomainOfExpertise(iteration).Contains(file.Owner);
                    }
                }
            }

            this.CanDownloadFile = (this.Thing != null) && isOwner && (this.dialogKind != ThingDialogKind.Create) && this.PermissionService.CanRead(this.Thing);
            this.CanAddFile = this.dialogKind == ThingDialogKind.Create;
            this.CanAddFileType = !this.IsReadOnly;
        }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.AddFileCommand = ReactiveCommandCreator.Create(this.AddFile, this.WhenAnyValue(x => x.CanAddFile));
            this.AddFileTypeCommand = ReactiveCommandCreator.Create(this.AddFileType, this.WhenAnyValue(x => x.CanAddFileType));
            this.DeleteFileTypeCommand = ReactiveCommandCreator.Create(this.DeleteFileType, this.WhenAnyValue(x => x.CanDeleteFileType));
            this.MoveUpFileTypeCommand = ReactiveCommandCreator.Create(this.MoveUpFileType, this.WhenAnyValue(x => x.CanMoveUpFileType));
            this.MoveDownFileTypeCommand = ReactiveCommandCreator.Create(this.MoveDownFileType, this.WhenAnyValue(x => x.CanMoveDownFileType));
            this.DownloadFileCommand = ReactiveCommandCreator.Create(() => this.downloadFileService.ExecuteDownloadFile(this, this.Thing), this.WhenAnyValue(x => x.CanDownloadFile));
            this.CancelDownloadCommand = ReactiveCommandCreator.Create(() => this.downloadFileService.CancelDownloadFile(this));
        }

        /// <summary>
        /// Update the <see cref="Path"/> property using functionality in the FileRevision POCO
        /// </summary>
        private void UpdatePath()
        {
            var clone = this.Thing.Clone(false);
            clone.Name = this.Name;
            clone.ContainingFolder = this.SelectedContainingFolder;
            clone.FileType.Clear();
            clone.FileType.AddRange(this.FileType);
            this.Path = clone.Path;
        }

        /// <summary>
        /// Populates the <see cref="PossibleFileType"/> property
        /// </summary>
        protected virtual void PopulatePossibleFileType()
        {
            this.PossibleFileType.Clear();
            var model = this.Container.GetContainerOfType<EngineeringModel>();
            var mrdl = model.EngineeringModelSetup.RequiredRdl.Single();

            var allowedFileTypes = new List<FileType>(mrdl.FileType);
            allowedFileTypes.AddRange(mrdl.GetRequiredRdls().SelectMany(rdl => rdl.FileType));

            this.PossibleFileType.AddRange(allowedFileTypes.OrderBy(c => c.ShortName));
        }

        /// <summary>
        /// Populates the <see cref="FileType"/> property
        /// </summary>
        protected virtual void PopulateFileType()
        {
            this.FileType.Clear();
            this.FileType.AddRange(this.Thing.FileType.SortedItems.Values);
        }

        /// <summary>
        /// Move a <see cref="FileType"/> down in the list of <see cref="FileType"/>s
        /// </summary>
        private void MoveDownFileType()
        {
            var currentIndex = this.FileType.IndexOf(this.SelectedFileType);
            this.FileType.Move(currentIndex, Math.Min(currentIndex + 1, this.FileType.Count - 1));
        }

        /// <summary>
        /// Move a <see cref="FileType"/> up in the list of <see cref="FileType"/>s
        /// </summary>
        private void MoveUpFileType()
        {
            var currentIndex = this.FileType.IndexOf(this.SelectedFileType);
            this.FileType.Move(currentIndex, Math.Max(currentIndex - 1, 0));
        }

        /// <summary>
        /// Remove a <see cref="FileType"/> from the list of <see cref="FileType"/>s
        /// </summary>
        private void DeleteFileType()
        {
            this.FileType.Remove(this.SelectedFileType);
        }

        /// <summary>
        /// Add the file of a physical file to this <see cref="FileRevision"/>
        /// </summary>
        private void AddFile()
        {
            var result = this.fileDialogService.GetOpenFileDialog(false, false, false, string.Empty, string.Empty, string.Empty, 1);

            if (result.Length != 1)
            {
                return;
            }

            var currentPath = result.First();
            var fileName = System.IO.Path.GetFileName(currentPath);

            this.LocalPath = currentPath;
            this.FileType.Clear();

            if (fileName != null)
            {
                var extensionArray = fileName.Split(new[] { "." }, StringSplitOptions.None);
                var fileTypes = new List<FileType>();

                for (var i = extensionArray.Length - 1; i >= 0; i--)
                {
                    var fileType = this.PossibleFileType.FirstOrDefault(x => x.Extension.ToLower().Equals(extensionArray[i].ToLower()));

                    if (fileType == null)
                    {
                        break;
                    }

                    fileTypes.Insert(0, fileType);
                    fileName = string.Join(".", extensionArray.Take(i));
                }

                this.FileType.AddRange(fileTypes);
            }

            this.Name = fileName;
        }

        /// <summary>
        /// Add a <see cref="FileType"/> to the list of <see cref="FileType"/>s
        /// </summary>
        private void AddFileType()
        {
            var result = this.thingSelectorDialogService.SelectThing(
                this.PossibleFileType.Except(this.FileType).ToList(),
                new List<string> { "Name", "ShortName", "Extension" });

            if (result != null && !this.FileType.Contains(result))
            {
                this.FileType.Add(result);
            }
        }

        /// <summary>
        /// Actions to do after FileType changes
        /// </summary>
        private void AfterUpdateFileType()
        {
            this.UpdateOkCanExecute();

            this.CanMoveUpFileType = !this.IsReadOnly
                                     && (this.SelectedFileType != null)
                                     && (this.FileType.Count > 1)
                                     && (this.FileType.IndexOf(this.SelectedFileType) > 0);

            this.CanMoveDownFileType = !this.IsReadOnly
                                       && (this.SelectedFileType != null)
                                       && (this.FileType.Count > 1)
                                       && (this.FileType.IndexOf(this.SelectedFileType) < this.FileType.Count - 1);

            this.UpdatePath();
        }

        /// <summary>
        /// Calculate the Hash of the contents of some filecontent
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        private string CalculateContentHash(string filePath)
        {
            if (filePath == null)
            {
                return null;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return StreamToHashComputer.CalculateSha1HashFromStream(fileStream);
            }
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();

            this.Thing.ContentHash = this.ContentHash;
            this.Thing.LocalPath = this.LocalPath;
        }

        /// <summary>
        /// Returns whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();

            this.OkCanExecute = this.OkCanExecute
                                && !string.IsNullOrWhiteSpace(this.Name)
                                && this.FileType.Any()
                                && (this.ContentHash != null)
                                && (this.LocalPath != null);
        }
    }
}
