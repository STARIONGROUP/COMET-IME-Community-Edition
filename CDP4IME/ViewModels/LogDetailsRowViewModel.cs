// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogDetailsRowViewModel.cs" company="Starion Group S.A.">
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
    using NLog;

    using ReactiveUI;

    /// <summary>
    /// The row-view-model representing a combination of property/content of a <see cref="LogEventInfo"/>
    /// </summary>
    public class LogDetailsRowViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Property"/>
        /// </summary>
        private string property;

        /// <summary>
        /// Backing field for <see cref="Content"/>
        /// </summary>
        private string content;

        /// <summary>
        /// Gets or Sets the name of the property to display
        /// </summary>
        public string Property
        {
            get { return this.property; }
            set { this.RaiseAndSetIfChanged(ref this.property, value); }
        }

        /// <summary>
        /// Gets or Sets the content associated to this <see cref="Property"/> to display
        /// </summary>
        public string Content
        {
            get { return this.content; }
            set { this.RaiseAndSetIfChanged(ref this.content, value); }
        }
    }
}
