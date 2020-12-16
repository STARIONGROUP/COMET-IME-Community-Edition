// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Adrian Chivu, Cozmin Velciu, Alex Vorobiev
//
//    This file is part of CDP4-Server-Administration-Tool.
//    The CDP4-Server-Administration-Tool is an ECSS-E-TM-10-25 Compliant tool
//    for advanced server administration.
//
//    The CDP4-Server-Administration-Tool is free software; you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as
//    published by the Free Software Foundation; either version 3 of the
//    License, or (at your option) any later version.
//
//    The CDP4-Server-Administration-Tool is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor.RowModels
{
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;

    /// <summary>
    /// Row class representing a <see cref="ParameterType"/> as a plain object
    /// </summary>
    public class ParameterTypeRowViewModel : CDP4CommonView.ParameterTypeRowViewModel<ParameterType>
    {
        /// <summary>
        /// Gets the <see cref="ClassKind"/> of the <see cref="ParameterType"/>
        /// </summary>
        public string Type => this.Thing.ClassKind.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeRowViewModel"/> class.
        /// </summary>
        /// <param name="parameterType">The <see cref="ParameterType"/></param>
        /// <param name="session">The session.</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParameterTypeRowViewModel(ParameterType parameterType, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterType, session, containerViewModel)
        {
        }
    }
}
