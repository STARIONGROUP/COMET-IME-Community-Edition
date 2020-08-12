// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DownloadManagerBarItemViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using CDP4Common.CommonData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using ReactiveUI;

    /// <summary>
    /// The View Model of the <see cref="Views.DownloadManagerBarItem"/> Bar item
    /// </summary>
    [DialogViewModelExport("DownloadManager", "The download manager window")]
    public class DownloadManagerViewModel : ReactiveObject, IFloatingDialogViewModel<Thing>
    {
        /// <summary>
        /// Backing field for <see cref="DialogResult"/> Property
        /// </summary>
        private IDialogResult dialogResult;

        /// <summary>
        /// Backing field for <see cref="IsBusy"/> Property
        /// </summary>
        private bool isBusy;

        /// <summary>
        /// Backing field for <see cref="LoadingMessage"/> Property
        /// </summary>
        private string loadingMessage;

        /// <summary>
        /// Backing field for <see cref="Thing"/> Property
        /// </summary>
        private Thing thing;
        
        /// <summary>
        /// Backing field for the <see cref="OverallProgressValue"/> property
        /// </summary>
        private double overallProgressValue;

        /// <summary>
        /// Gets the overall progress of the downloads 
        /// </summary>
        public double OverallProgressValue
        {
            get => this.overallProgressValue;
            set => this.RaiseAndSetIfChanged(ref this.overallProgressValue, value);
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public IDialogResult DialogResult
        {
            get => this.dialogResult;
            set => this.RaiseAndSetIfChanged(ref this.dialogResult, value);
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        public string LoadingMessage
        {
            get => this.loadingMessage;
            set => this.RaiseAndSetIfChanged(ref this.loadingMessage, value);
        }

        public Thing Thing => this.thing;
    }
}
