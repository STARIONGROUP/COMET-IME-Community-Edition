// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorReporterViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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
// <remarks>
//    Ideas taken from ExceptionReporter.NET: https://github.com/PandaWood/ExceptionReporter.NET
// </remarks>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ErrorReporting.ViewModels
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// The ErrorReporter logic.
    /// </summary>
    public class ErrorReporterViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="MainException"/>
        /// </summary>
        private Exception mainException;

        /// <summary>
        /// Backing field for <see cref="ShowingDetails"/>
        /// </summary>
        private bool showingDetails;

        /// <summary>
        /// Backing field for <see cref="ReportText"/>
        /// </summary>
        private string reportText;

        /// <summary>
        /// The <see cref="ICommand"/> that implements copy functionality
        /// </summary>
        public ReactiveCommand<Unit, Unit> CopyCommand { get; }

        /// <summary>
        /// The <see cref="ICommand"/> that implements SHowDetails functionality
        /// </summary>
        public ReactiveCommand<Unit, Unit> ShowDetailsCommand { get; }

        /// <summary>
        /// Creates a new instance of <see cref="ErrorReporterViewModel"/>
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/></param>
        public ErrorReporterViewModel(Exception exception)
        {
            this.reportText = this.GenerateReport(exception);
            this.MainException = exception;

            this.CopyCommand = ReactiveCommandCreator.Create(this.Copy);
            this.ShowDetailsCommand = ReactiveCommandCreator.Create(this.ShowDetails);
        }

        /// <summary>
        /// Gets or sets the tectual representation of the <see cref="Exception"/>
        /// </summary>
        public string ReportText
        {
            get => this.reportText;
            set => this.RaiseAndSetIfChanged(ref this.reportText, value);
        }

        /// <summary>
        /// Gets a value indicating that summary is shown, or not
        /// </summary>
        public bool ShowingSummary => !this.ShowingDetails;

        /// <summary>
        /// Gets or sets a value indicating that details are shown, or not
        /// </summary>
        public bool ShowingDetails
        {
            get => this.showingDetails;
            set
            {
                this.RaiseAndSetIfChanged(ref this.showingDetails, value);
                this.RaisePropertyChanged(nameof(this.ShowingSummary));
            }
        }

        /// <summary>
        /// The main <see cref="Exception"/>
        /// </summary>
        public Exception MainException
        {
            get => this.mainException;
            set => this.RaiseAndSetIfChanged(ref this.mainException, value);
        }

        /// <summary>
        /// Copy the report text to the clipboard
        /// </summary>
        private void Copy()
        {
            Clipboard.SetText(this.reportText);
        }

        /// <summary>
        /// Reverses the <see cref="ShowingDetails"/> value
        /// </summary>
        private void ShowDetails()
        {
            this.ShowingDetails = !this.ShowingDetails;
        }

        /// <summary>
        /// Generate textual representation of the <see cref="Exception"/>
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/></param>
        /// <returns>The textual representation</returns>
        private string GenerateReport(Exception exception)
        {
            var strBuilder = new StringBuilder();

            this.WriteException(exception, strBuilder, 0);

            return strBuilder.ToString();
        }

        /// <summary>
        /// Write an exception to a <see cref="StringBuilder"/>
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/></param>
        /// <param name="strBuilder">The <see cref="StringBuilder"/></param>
        /// <param name="level">The hierarchical level of the <see cref="Exception"/></param>
        private void WriteException(Exception exception, StringBuilder strBuilder, int level)
        {
            var prefix = string.Concat(Enumerable.Repeat('\t', level));

            strBuilder.AppendLine($"{prefix}{exception.Message}");

            strBuilder.AppendLine($"{prefix}{exception.StackTrace?.Replace("\n", $"\n{prefix}")}");

            if (exception.InnerException != null && level < 10)
            {
                strBuilder.AppendLine($"{prefix}InnerException:");
                this.WriteException(exception.InnerException, strBuilder, ++level);
            }
        }
    }
}
