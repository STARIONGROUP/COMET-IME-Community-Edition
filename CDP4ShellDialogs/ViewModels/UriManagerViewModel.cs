// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriManagerViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.ViewModels
{
    using System;
    using System.Linq;
    using CDP4Composition.Navigation;
    using CDP4Composition.Utilities;
    using CDP4Dal.Composition;
    using ReactiveUI;
    
    /// <summary>
    /// The purpose of the <see cref="UriManagerViewModel"/> is to manage the configured uris
    /// </summary>
    public class UriManagerViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedUriRow"/> property.
        /// </summary>
        private UriRowViewModel selectedUriRow;

        /// <summary>
        /// Initializes a new instance of the <see cref="UriManagerViewModel"/> class.
        /// </summary>
        public UriManagerViewModel()
        {
            this.ApplyCommand = ReactiveCommand.Create();
            this.ApplyCommand.Subscribe(_ => this.ExecuteApply());
            this.DeleteRowCommand = ReactiveCommand.Create();
            this.DeleteRowCommand.Subscribe(_ => this.DeleteSelectedRow());

            this.UriRowList = new ReactiveList<UriRowViewModel>();
            this.RePopulateUriRows();

            this.DalTypesList = new ReactiveList<DalType>();
            foreach (DalType type in Enum.GetValues(typeof(DalType)))
            {
                this.DalTypesList.Add(type);
            }

            this.CloseCommand = ReactiveCommand.Create();
            this.CloseCommand.Subscribe(_ => this.ExecuteClose());
        }

        /// <summary>
        /// Clears and populates the <see cref="UriRowList"/>
        /// </summary>
        private void RePopulateUriRows()
        {
            this.UriRowList.Clear();

            var configHandler = new UriConfigFileHandler();
            var uriList = configHandler.Read();

            foreach (var uri in uriList)
            {
                var row = new UriRowViewModel { UriConfig = uri };
                this.UriRowList.Add(row);
            }
        }
       
        /// <summary>
        /// Executes the Close Command
        /// </summary>
        private void ExecuteClose()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Method to write into a file the JSON configuration
        /// </summary>
        private void ExecuteApply()
        {
            var writer = new UriConfigFileHandler();
            writer.Write(this.UriRowList.Select(row => row.UriConfig).ToList());
            this.ExecuteClose();
        }

        /// <summary>
        /// Method to delete the selected grid row
        /// </summary>
        private void DeleteSelectedRow()
        {
            this.UriRowList.Remove(this.selectedUriRow);
        }

        /// <summary>
        /// Gets the <see cref="CloseCommand"/>
        /// </summary>
        public ReactiveCommand<object> CloseCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ApplyCommand"/>
        /// </summary>
        public ReactiveCommand<object> ApplyCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="DeleteRowCommand"/>
        /// </summary>
        public ReactiveCommand<object> DeleteRowCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="UriRowList"/> to display
        /// </summary>
        public ReactiveList<UriRowViewModel> UriRowList { get; private set; }

        /// <summary>
        /// Gets the <see cref="DalTypesList"/> to display
        /// </summary>
        public ReactiveList<DalType> DalTypesList { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="selectedUriRow"/> to display
        /// </summary>       
        public UriRowViewModel SelectedUriRow
        {
            get { return this.selectedUriRow; }
            set { this.RaiseAndSetIfChanged(ref this.selectedUriRow, value); }
        }
    }
}