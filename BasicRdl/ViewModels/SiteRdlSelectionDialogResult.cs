﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteRdlSelectionDialogResult.cs" company="Starion Group S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using CDP4Composition.Navigation;

    /// <summary>
    /// The <see cref="IDialogResult"/> for the <see cref="Views.SiteRdlOpeningDialog"/> Dialog
    /// </summary>
    public class SiteRdlSelectionDialogResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SiteRdlSelectionDialogResult"/> class
        /// </summary>
        /// <param name="res">
        /// The response.
        /// </param>
        public SiteRdlSelectionDialogResult(bool? res)
            : base(res)
        {
        }
    }
}