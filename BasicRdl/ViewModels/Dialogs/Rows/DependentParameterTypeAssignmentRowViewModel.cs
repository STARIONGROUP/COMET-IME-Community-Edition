// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DependentParameterTypeAssignmentRowViewModel.cs" company="Starion Group S.A.">
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

namespace BasicRdl.ViewModels.Dialogs.Rows
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    /// <summary>
    /// The row view model for the <see cref="DependentParameterTypeAssignment"/>
    /// </summary>
    public class DependentParameterTypeAssignmentRowViewModel : ParameterTypeAssignmentRowViewModel<DependentParameterTypeAssignment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependentParameterTypeAssignmentRowViewModel"/> class.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> represented by the row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The parent Row</param>
        public DependentParameterTypeAssignmentRowViewModel(DependentParameterTypeAssignment thing, ISession session, SampledFunctionParameterTypeDialogViewModel containerViewModel) : base(thing, session, containerViewModel)
        {
        }
    }
}
