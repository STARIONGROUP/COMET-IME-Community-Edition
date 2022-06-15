// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriManagerViewModel.cs" company="RHEA System S.A.">
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

namespace CDP4ShellDialogs.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;

    using CDP4Composition.Mvvm;
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
            this.ApplyCommand = ReactiveCommandCreator.Create(this.ExecuteApply);
            this.DeleteRowCommand = ReactiveCommandCreator.Create(this.DeleteSelectedRow);

            this.UriRowList = new ReactiveList<UriRowViewModel>();
            this.RePopulateUriRows();

            this.DalTypesList = new ReactiveList<DalType>();

            foreach (DalType type in Enum.GetValues(typeof(DalType)))
            {
                this.DalTypesList.Add(type);
            }

            this.CloseCommand = ReactiveCommandCreator.Create(this.ExecuteClose);
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
        public ReactiveCommand<Unit, Unit> CloseCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="ApplyCommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> ApplyCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="DeleteRowCommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> DeleteRowCommand { get; private set; }

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