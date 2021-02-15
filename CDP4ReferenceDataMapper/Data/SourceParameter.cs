// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SourceParameter.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4ReferenceDataMapper.Data
{
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// Specific UI class used as a wrapper for a <see cref="ParameterType"/>
    /// </summary>
    public class SourceParameter
    {
        /// <summary>
        /// The <see cref="ParameterType"/>
        /// </summary>
        private readonly ParameterType parameterType;

        /// <summary>
        /// Gets the name of the <see cref="SourceParameter"/>
        /// </summary>
        public string Name => this.parameterType?.Name;

        /// <summary>
        /// Gets the ShortName of the <see cref="SourceParameter"/>
        /// </summary>
        public string ShortName => this.parameterType?.ShortName;

        /// <summary>
        /// Gets the Iid of the <see cref="SourceParameter"/> as a string
        /// </summary>
        public string Iid => this.parameterType?.Iid.ToString();

        /// <summary>
        /// Gets the display name of the <see cref="SourceParameter"/>
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SourceParameter"/> class
        /// </summary>
        /// <param name="parameterType">
        /// The <see cref="ParameterType"/>
        /// </param>
        /// <param name="displayName">
        /// The display name in the UI
        /// </param>
        public SourceParameter(ParameterType parameterType, string displayName)
        {
            this.parameterType = parameterType;
            this.DisplayName = displayName;
        }
    }
}
