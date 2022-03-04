// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReportOutputEvent.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.Events
{
    /// <summary>
    /// The purpose of the <see cref="ReportOutputEvent"/> is to notify an observer
    /// that a text should be added to the report designer's output panel.
    /// </summary>
    public class ReportOutputEvent
    {
        /// <summary>
        /// The output to be added to the Output panel
        /// </summary>
        public string Output { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="ReportOutputEvent"/> class
        /// </summary>
        /// <param name="output">
        /// The output to be added to the Output panel
        /// </param>
        public ReportOutputEvent(string output)
        {
            this.Output = output;
        }
    }
}
