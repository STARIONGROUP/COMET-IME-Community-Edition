// -------------------------------------------------------------------------------------------------
// <copyright file="FilterEditorSavedUserPreference.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smieckowski
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
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services.FilterEditorService
{
    using CDP4Common.SiteDirectoryData;

    using DevExpress.Xpf.Grid;

    using Newtonsoft.Json;

    /// <summary>
    /// Holds data that can be used to store data in a <see cref="UserPreference"/>
    /// </summary>
    public class FilterEditorSavedUserPreference : SavedUserPreference
    {
        /// <summary>
        /// The name of the corresponding <see cref="DataControlBase"/>
        /// </summary>
        public string DataControlName { get; set; }

        /// <summary>
        /// Instanciates a new <see cref="FilterEditorSavedUserPreference"/>
        /// </summary>
        /// <param name="dataControlName">The name of the </param>
        /// <param name="name">The name</param>
        /// <param name="value">The value</param>
        /// <param name="description">The description</param>
        [JsonConstructor]
        public FilterEditorSavedUserPreference(string dataControlName, string name, string value, string description = "") 
            : base(name, value, description)
        {
            this.DataControlName = dataControlName;
        }
    }
}
