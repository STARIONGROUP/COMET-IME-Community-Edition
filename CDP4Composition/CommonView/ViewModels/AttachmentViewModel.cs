// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AttachmentViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4Composition.CommonView.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// A view model that represents an <see cref="Attachment"/>
    /// </summary>
    public class AttachmentViewModel : ViewModelBase<Attachment>, IBehavioralModelKindViewModel
    {
        /// <summary>
        /// Backing field for <see cref="ContentHash"/>
        /// </summary>
        private string contentHash;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachmentViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public AttachmentViewModel()
        {
        }

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
        /// <param name="container">
        /// The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
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

            this.Disposables.Add(this.WhenAnyValue(x => x.Name, x => x.ContentHash).Subscribe(_ => this.UpdatePath()));

            this.Disposables.Add
            (
                this.WhenAnyValue(x => x.SelectedFileType, x => x.FileType).Subscribe(_ =>
                {
                    this.CanDeleteFileType = !this.IsReadOnly && this.SelectedFileType is not null;
                    this.AfterUpdateFileType();
                })
            );

            this.Disposables.Add(this.FileType.Changed.Subscribe(_ => this.AfterUpdateFileType()));
            this.Disposables.Add(this.WhenAnyValue(x => x.LocalPath).Subscribe(_ => this.SetContentHash()));
        }

        /// <summary>
        /// Gets or sets the ContentHash    
        /// </summary>
        public virtual string ContentHash
        {
            get { return this.contentHash; }
            set { this.RaiseAndSetIfChanged(ref this.contentHash, value); }
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
            get { return this.fileType; }
            set { this.RaiseAndSetIfChanged(ref this.fileType, value); }
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
            this.AddFileCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanAddFile));
            this.Disposables.Add(this.AddFileCommand.Subscribe(_ => this.AddFile()));

            this.AddFileTypeCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanAddFileType));
            this.Disposables.Add(this.AddFileTypeCommand.Subscribe(_ => this.AddFileType()));

            this.DeleteFileTypeCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanDeleteFileType));
            this.Disposables.Add(this.DeleteFileTypeCommand.Subscribe(_ => this.DeleteFileType()));

            this.MoveUpFileTypeCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanMoveUpFileType));
            this.Disposables.Add(this.MoveUpFileTypeCommand.Subscribe(_ => this.MoveUpFileType()));

            this.MoveDownFileTypeCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanMoveDownFileType));
            this.Disposables.Add(this.MoveDownFileTypeCommand.Subscribe(_ => this.MoveDownFileType()));
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        private void UpdateThing()
        {
            var clone = this.Thing;
            clone.ContentHash = this.ContentHash;
            clone.LocalPath = this.LocalPath;
            clone.FileName = this.Name;

            if (!clone.FileType.SequenceEqual(this.FileType))
            {
                var fileTypeCount = this.FileType.Count;
                for (var i = 0; i < fileTypeCount; i++)
                {
                    var item = this.FileType[i];
                    var currentIndex = clone.FileType.IndexOf(item);

                    if (currentIndex != -1 && currentIndex != i)
                    {
                        clone.FileType[i] = item;
                    }
                    else if (currentIndex == -1)
                    {
                        clone.FileType.Insert(i, item);
                    }
                }

                // remove items that are no longer referenced
                for (var i = fileTypeCount; i < clone.FileType.Count; i++)
                {
                    var toRemove = clone.FileType[i];
                    clone.FileType.Remove(toRemove);
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
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected void UpdateProperties()
        {
            this.ContentHash = this.Thing.ContentHash;
            this.Name = this.Thing.FileName;

            this.localPath = this.Thing.LocalPath;
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
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Calculates and sets the <see cref="ContentHash"/> property accordingly
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/></returns>
        private void SetContentHash()
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
        /// Gets the <see cref="ICommand"/> to download a file to a locally available drive
        /// </summary>
        public ReactiveCommand<object> DownloadFileCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to add a physical file to the <see cref="Attachment"/>
        /// </summary>
        public ReactiveCommand<object> AddFileCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to add a <see cref="FileType"/> to this <see cref="Attachment"/>
        /// </summary>
        public ReactiveCommand<object> AddFileTypeCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to remove a <see cref="FileType"/> from this <see cref="Attachment"/>
        /// </summary>
        public ReactiveCommand<object> DeleteFileTypeCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to move a <see cref="FileType"/> down in the ordering of <see cref="FileType"/>s
        /// </summary>
        public ReactiveCommand<object> MoveUpFileTypeCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to to move a <see cref="FileType"/> up in the ordering of <see cref="FileType"/>s
        /// </summary>
        public ReactiveCommand<object> MoveDownFileTypeCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> to cancel download of a file
        /// </summary>
        public ReactiveCommand<object> CancelDownloadCommand { get; private set; }

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
            clone.FileName = this.GetFileName();
            clone.FileType.Clear();
            clone.FileType.AddRange(this.FileType);
            this.Path = clone.FileName;
        }

        /// <summary>
        /// Gets the file name including extensions
        /// </summary>
        /// <returns></returns>
        private string GetFileName()
        {
            var path = new StringBuilder();

            path.Append(this.Name);

            foreach (var fileType in this.FileType.Where(x => !string.IsNullOrWhiteSpace(x.Extension)))
            {
                path.Append(".");
                path.Append(fileType.Extension);
            }

            return path.ToString();
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
        /// Calculate the Hash of the contents of some filecontent
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns>The <see cref="string"/> hash of the file</returns>
        private string CalculateContentHash(string filePath)
        {
            if (filePath is null)
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
            return !string.IsNullOrWhiteSpace(this.Name)
                   && this.FileType.Any()
                   && this.ContentHash is not null
                   && this.LocalPath is not null;
        }

        /// <summary>
        /// Update the transaction with the <see cref="BehavioralModelKind"/> information represented by this view model
        /// </summary>
        /// <param name="transaction">The transaction for the <see cref="Thing"/></param>
        /// <param name="clone">The <see cref="Behavior"/> for which to update the <see cref="IThingTransaction"/></param>
        public void UpdateTransaction(IThingTransaction transaction, Behavior thing)
        {
            thing.Script = null;
            this.UpdateThing();
            transaction.CreateOrUpdate(this.Thing);
            thing.Attachment.Add(this.Thing);
        }
    }
}
