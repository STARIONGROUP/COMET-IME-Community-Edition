// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataSourceSelectionResult.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2025 Starion Group S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
// 
//    This file is part of CDP4-COMET-IME Community Edition.
//    The CDP4-COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The CDP4-COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The CDP4-COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.ViewModels
{
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using CDP4DalCommon.Authentication;

    /// <summary>
    /// The <see cref="IDialogResult" /> for the <see cref="DataSourceSelection" /> dialog
    /// </summary>
    public class DataSourceSelectionResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceSelectionResult" /> class
        /// </summary>
        /// <param name="res">An instance of <see cref="T?" /> that gives the action of the user</param>
        /// <param name="session">The <see cref="ISession" /> that was opened</param>
        /// <param name="authenticationSchemeResponse">
        /// The associated <see cref="AuthenticationSchemeResponse" /> to the opened
        /// <see cref="ISession" />
        /// </param>
        /// <param name="openModel">The value indicating whether to open the model selection dialog</param>
        public DataSourceSelectionResult(bool? res, ISession session, AuthenticationSchemeResponse authenticationSchemeResponse, bool openModel = false)
            : base(res)
        {
            this.Session = session;
            this.OpenModel = openModel;
            this.AuthenticationSchemeResponse = authenticationSchemeResponse;
        }

        /// <summary>
        /// Gets or sets the associated <see cref="AuthenticationSchemeResponse" /> to the opened <see cref="ISession" />
        /// </summary>
        public AuthenticationSchemeResponse AuthenticationSchemeResponse { get; set; }

        /// <summary>
        /// Gets the <see cref="ISession" /> that was opened
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to open the model selection dialog
        /// </summary>
        public bool OpenModel { get; private set; }
    }
}
