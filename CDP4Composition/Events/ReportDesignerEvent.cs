// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportDesignerViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4Composition.Events
{
    /// <summary>
    /// Assertions that determine the kind of notification to trigger/subscribe
    /// </summary>
    public enum ReportNotificationKind
    {
        /// <summary>
        /// Notification raised when report archive file is opened
        /// </summary>
        REPORT_OPEN = 0,

        /// <summary>
        /// Notification raised when report archive file is saved
        /// </summary>
        REPORT_SAVE = 1,
    }

    /// <summary>
    /// The event that is used to bind different report notifications
    /// </summary>
    public class ReportDesignerEvent
    {
        /// <summary>
        /// Get sets report archive file of the notification
        /// </summary>
        public string Rep4File { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReportNotificationKind"/> of the notification
        /// </summary>
        public ReportNotificationKind NotificationKind { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportDesignerEvent"/> class
        /// </summary>
        /// <param name="rep4File">The report archive file for which the notification is created</param>
        /// <param name="kind">The <see cref="ReportNotificationKind"/></param>
        public ReportDesignerEvent(string rep4File, ReportNotificationKind kind)
        {
            Rep4File = rep4File;
            NotificationKind = kind;
        }
    }
}
