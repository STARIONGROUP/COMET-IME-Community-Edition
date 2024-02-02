 // --------------------------------------------------------------------------------------------------------------------

// <copyright file="AttachmentViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2024 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The CDP4-COMET IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Composition.CommonView.ViewModels
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

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Services;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    using CDP4Composition.Views;

    using CommonServiceLocator;

    using DevExpress.Xpo;

    /// <summary>
    /// A view model that represents an <see cref="Attachment"/>
    /// </summary>
    public class AttachmentViewModel : ViewModelBase<Attachment>, IBehavioralModelKindViewModel, IDownloadFileViewModel
    {
        /// <summary>
        /// The (injected) <see cref="IDownloadFileService"/>
        /// </summary>
        private readonly IDownloadFileService downloadFileService = ServiceLocator.Current.GetInstance<IDownloadFileService>();

        /// <summary>
        /// Backing field for <see cref="IsCancelButtonVisible"/>
        /// </summary>
        private bool isCancelButtonVisible;

        /// <summary>
        /// Backing field for <see cref="LoadingMessage"/>
        /// </summary>
        private string loadingMessage;

        /// <summary>
        /// Backing field for <see cref="ContentHash"/>
        /// </summary>
        private string contentHash;

        /// <summary>
        /// Backing field for <see cref="FileName"/>
        /// </summary>
        private string fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentViewModel"/> class
        /// </summary>
        /// <param name="attachment">
        /// The <see cref="Attachment"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited. 
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="chainOfContainer">
        /// The optional chain of containers
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="DialogViewModelBase{T}"/> performs
        /// </param>
        public AttachmentViewModel(Attachment attachment, ISession session, List<Thing> chainOfContainer, ThingDialogKind dialogKind)
            : base(attachment, session)
        {
            this.chainOfContainer = chainOfContainer;
            this.dialogKind = dialogKind;

            this.Initialize();
            this.InitializeCommands();

            this.UpdateProperties();

            this.Disposables.Add(this.WhenAnyValue(x => x.FileName).Subscribe(_ => this.UpdatePath()));

            this.Disposables.Add(
                this.WhenAnyValue(x => x.SelectedFileType, x => x.FileType).Subscribe(
                    _ =>
                    {
                        this.CanDeleteFileType = !this.IsReadOnly && this.SelectedFileType is not null;
                        this.AfterUpdateFileType();
                    }));

            this.Disposables.Add(this.FileType.Changed.Subscribe(_ => this.AfterUpdateFileType()));
        }

        /// <summary>
        /// Gets or sets the ContentHash    
        /// </summary>
        public string ContentHash
        {
            get => this.contentHash;
            set => this.RaiseAndSetIfChanged(ref this.contentHash, value);
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
        /// Backing field for <see cref="FileType"/>s
        /// </summary>
        private ReactiveList<FileType> fileType;

        /// <summary>
        /// Gets or sets the list of selected <see cref="FileType"/>s
        /// </summary>
        public ReactiveList<FileType> FileType
        {
            get => this.fileType;
            set => this.RaiseAndSetIfChanged(ref this.fileType, value);
        }

        /// <summary>
        /// Gets or sets the Possible <see cref="FileType"/> for <see cref="FileType"/>
        /// </summary>
        public ReactiveList<FileType> PossibleFileType { get; protected set; }

        /// <summary>
        /// Initialize the <see cref="ICommand"/>s and listeners
        /// </summary>
        protected void InitializeCommands()
        {
            this.DownloadFileCommand = ReactiveCommandCreator.Create(() => this.downloadFileService.ExecuteDownloadFile(this, this.Thing), this.WhenAnyValue(x => x.CanDownloadFile));

            this.CancelDownloadCommand = ReactiveCommandCreator.Create(() => this.downloadFileService.CancelDownloadFile(this));

            this.AddFileCommand = ReactiveCommandCreator.Create(this.AddFile, this.WhenAnyValue(x => x.CanAddFile));

            this.AddFileTypeCommand = ReactiveCommandCreator.Create(this.AddFileType, this.WhenAnyValue(x => x.CanAddFileType));

            this.DeleteFileTypeCommand = ReactiveCommandCreator.Create(this.DeleteFileType, this.WhenAnyValue(x => x.CanDeleteFileType));

            this.MoveUpFileTypeCommand = ReactiveCommandCreator.Create(this.MoveUpFileType, this.WhenAnyValue(x => x.CanMoveUpFileType));

            this.MoveDownFileTypeCommand = ReactiveCommandCreator.Create(this.MoveDownFileType, this.WhenAnyValue(x => x.CanMoveDownFileType));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        private void UpdateThing(Attachment thing)
        {
            thing.ContentHash = this.ContentHash;
            thing.LocalPath = this.LocalPath;
            thing.FileName = this.FileName;

            if (!thing.FileType.SequenceEqual(this.FileType))
            {
                var fileTypeCount = this.FileType.Count;

                for (var i = 0; i < fileTypeCount; i++)
                {
                    var item = this.FileType[i];
                    var currentIndex = thing.FileType.IndexOf(item);

                    if (currentIndex != -1 && currentIndex != i)
                    {
                        thing.FileType[i] = item;
                    }
                    else if (currentIndex == -1)
                    {
                        thing.FileType.Insert(i, item);
                    }
                }

                // remove items that are no longer referenced
                for (var i = fileTypeCount; i < thing.FileType.Count; i++)
                {
                    var toRemove = thing.FileType[i];
                    thing.FileType.Remove(toRemove);
                }
            }
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        private void Initialize()
        {
            this.FileType = new ReactiveList<FileType>();
            this.PossibleFileType = new ReactiveList<FileType>();
            this.PopulatePossibleFileType();

            this.CanAddFile = !this.IsReadOnly;
            this.CanAddFileType = !this.IsReadOnly;

            this.CanDownloadFile = (this.Thing != null) && (this.dialogKind != ThingDialogKind.Create) && this.PermissionService.CanRead(this.Thing);
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected void UpdateProperties()
        {
            this.ContentHash = this.Thing.ContentHash;
            this.FileName = this.Thing.FileName;
            this.Path = this.Thing.Path;

            this.PopulateFileType();
        }

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
        /// The backing field for <see cref="Path"/>
        /// </summary>
        private string path;

        /// <summary>
        /// The <see cref="IThingSelectorDialogService"/>
        /// </summary>
        private readonly IThingSelectorDialogService thingSelectorDialogService = ServiceLocator.Current.GetInstance<IThingSelectorDialogService>();

        /// <summary>
        /// The <see cref="IOpenSaveFileDialogService"/>
        /// </summary>
        private readonly IOpenSaveFileDialogService fileDialogService = ServiceLocator.Current.GetInstance<IOpenSaveFileDialogService>();

        /// <summary>
        /// Gets the chain of <see cref="Thing"/>s that contains the current one
        /// </summary>
        private readonly List<Thing> chainOfContainer;

        /// <summary>
        /// The <see cref="ThingDialogKind"/> of the containing dialog
        /// </summary>
        private readonly ThingDialogKind dialogKind;

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string FileName
        {
            get => this.fileName;
            set => this.RaiseAndSetIfChanged(ref this.fileName, value);
        }

        /// <summary>
        /// Calculates and sets the <see cref="ContentHash"/> property accordingly
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private void SetContentHash()
        {
            this.ContentHash = this.localPath == null
                ? this.Thing.ContentHash
                : this.CalculateContentHash(this.localPath);
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
        /// Gets the <see cref="ICommand"/> to download a file to a locally available drive
        /// </summary>
        public ReactiveCommand<Unit, Unit> DownloadFileCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to add a physical file to the <see cref="Attachment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddFileCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to add a <see cref="FileType"/> to this <see cref="Attachment"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> AddFileTypeCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to remove a <see cref="FileType"/> from this <see cref="Attachment"/>
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
        /// Gets a value indicating whether the associated view is read-only
        /// </summary>
        public virtual bool IsReadOnly => this.dialogKind == ThingDialogKind.Inspect;

        /// <summary>
        /// Update the <see cref="Path"/> property using functionality in the Attachment POCO
        /// </summary>
        private void UpdatePath()
        {
            var clone = this.Thing.Clone(false);
            clone.FileName = this.FileName;
            clone.FileType.Clear();
            clone.FileType.AddRange(this.FileType);
            this.Path = clone.Path;
        }

        /// <summary>
        /// Populates the <see cref="PossibleFileType"/> property
        /// </summary>  
        protected virtual void PopulatePossibleFileType()
        {
            if (this.IsReadOnly)
            {
                return;
            }

            this.PossibleFileType.Clear();

            var fileTypes = this.chainOfContainer.OfType<Iteration>()
                .Single()
                .GetContainerOfType<EngineeringModel>()
                .RequiredRdls
                .SelectMany(rdl => rdl.FileType)
                .OrderBy(c => c.ShortName);

            this.PossibleFileType.AddRange(fileTypes);
        }

        /// <summary>
        /// Populates the <see cref="FileType"/> property
        /// </summary>
        protected void PopulateFileType()
        {
            this.FileType.Clear();
            this.FileType.AddRange(this.Thing.FileType);
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
        /// Add the file of a physical file to this <see cref="Attachment"/>
        /// </summary>
        private void AddFile()
        {
            var result = this.fileDialogService.GetOpenFileDialog(false, false, false, string.Empty, string.Empty, string.Empty, 1);

            if (result?.Length != 1)
            {
                return;
            }

            var currentPath = result.First();
            var fileName = System.IO.Path.GetFileName(currentPath);

            this.LocalPath = currentPath;
            this.FileType.Clear();

            if (fileName is not null)
            {
                var extensionArray = fileName.Split(new[] { "." }, StringSplitOptions.None);
                var fileTypes = new List<FileType>();

                for (var i = extensionArray.Length - 1; i >= 0; i--)
                {
                    var fileType = this.PossibleFileType.FirstOrDefault(x => x.Extension.ToLower().Equals(extensionArray[i].ToLower()));

                    if (fileType is null)
                    {
                        break;
                    }

                    fileTypes.Insert(0, fileType);
                    fileName = string.Join(".", extensionArray.Take(i));
                }

                this.FileType.AddRange(fileTypes);
            }

            this.FileName = fileName;

            this.SetContentHash();
        }

        /// <summary>
        /// Add a <see cref="FileType"/> to the list of <see cref="FileType"/>s
        /// </summary>
        private void AddFileType()
        {
            var result = this.thingSelectorDialogService.SelectThing(
                this.PossibleFileType.Except(this.FileType).ToList(),
                new List<string> { "Name", "ShortName", "Extension" });

            if (result is not null && !this.FileType.Contains(result))
            {
                this.FileType.Add(result);
            }
        }

        /// <summary>
        /// Actions to do after FileType changes
        /// </summary>
        private void AfterUpdateFileType()
        {
            this.CanMoveUpFileType = !this.IsReadOnly &&
                                     this.SelectedFileType is not null &&
                                     this.FileType.Count > 1 &&
                                     this.FileType.IndexOf(this.SelectedFileType) > 0;

            this.CanMoveDownFileType = !this.IsReadOnly &&
                                       this.SelectedFileType is not null &&
                                       this.FileType.Count > 1 &&
                                       this.FileType.IndexOf(this.SelectedFileType) < this.FileType.Count - 1;

            this.UpdatePath();
        }

        /// <summary>
        /// Calculate the Hash of the contents of some file content
        /// </summary>
        /// <param name="filePath">The path to the physical file</param>
        /// <returns>The <see cref="string"/> hash of the file</returns>
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
        /// Returns the if the OK command can be executed for this view model
        /// </summary>
        /// <returns>The ok status</returns>
        public bool OkCanExecute()
        {
            return !string.IsNullOrWhiteSpace(this.FileName)
                   && this.FileType.Any()
                   && this.ContentHash is not null;
        }

        /// <summary>
        /// Update the transaction with the <see cref="BehavioralModelKind"/> information represented by this view model
        /// </summary>
        /// <param name="transaction">The transaction for the <see cref="Thing"/></param>
        /// <param name="clone">The <see cref="Behavior"/> for which to update the <see cref="IThingTransaction"/></param>
        public void UpdateTransaction(IThingTransaction transaction, Behavior clone)
        {
            clone.Script = null;

            if (this.Thing.ContentHash != this.ContentHash
                || this.Thing.FileName != this.FileName
                || !this.Thing.FileType.SequenceEqual(this.FileType))
            {
                var newThing = new Attachment();
                this.UpdateThing(newThing);
                transaction.CreateOrUpdate(newThing);
                clone.Attachment.Clear();
                clone.Attachment.Add(newThing);
            }
        }
    }
}
