﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogDetailsViewModel.cs" company="Starion Group S.A.">
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
// --------------------------------------------------------------------------------------------------------------------

namespace COMET.ViewModels
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using NLog;

    /// <summary>
    /// The Log Detail dialogBox view-model
    /// </summary>
    [DialogViewModelExport("LogDetails", "The dialog detailing a log message")]
    public class LogDetailsViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogDetailsViewModel"/> class.
        /// Used by MEF.
        /// </summary>
        public LogDetailsViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogDetailsViewModel"/> class
        /// </summary>
        /// <param name="log">The <see cref="LogEventInfo"/> to display</param>
        public LogDetailsViewModel(LogEventInfo log)
        {
            this.DetailRows = new ReactiveList<LogDetailsRowViewModel>();

            foreach (var propertyInfo in log.GetType().GetProperties())
            {
                var propertyName = propertyInfo.Name;
                var content = propertyInfo.GetValue(log) ?? string.Empty;

                var row = new LogDetailsRowViewModel
                {
                    Property = propertyName,
                    Content = content.ToString()
                };

                this.DetailRows.Add(row);
            }
        }

        /// <summary>
        /// Gets the Dialog Title
        /// </summary>
        public string DialogTitle
        {
            get { return "Error Details"; }
        }

        /// <summary>
        /// Gets the <see cref="LogDetailsRowViewModel"/> to display
        /// </summary>
        public ReactiveList<LogDetailsRowViewModel> DetailRows { get; private set; }
    }
}