// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ErrorReporter.xaml.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Geren�, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Th�ate, Omar Elebiary
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

namespace CDP4Composition.ErrorReporting.Views
{
    using System;

    using CDP4Composition.ErrorReporting.ViewModels;

    /// <summary>
    /// Interaction logic for ErrorReporter
    /// </summary>
    public partial class ErrorReporter
    {
        /// <summary>
        /// Creates a new instance of <see cref="ErrorReporter"/>
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to build the <see cref="ErrorReporter"/> for.</param>
        private ErrorReporter(Exception exception)
        {
            this.InitializeComponent();

            this.DataContext = new ErrorReporterViewModel(exception);
        }

        /// <summary>
        /// Shows a modal <see cref="ErrorReporter"/>
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/></param>
        public static void ShowDialog(Exception ex)
        {
            new ErrorReporter(ex).ShowDialog();
        }

        /// <summary>
        /// Shows an <see cref="ErrorReporter"/>
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/></param>
        public static void Show(Exception ex)
        {
            new ErrorReporter(ex).Show();
        }
    }
}
