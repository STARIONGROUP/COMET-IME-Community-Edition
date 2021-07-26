// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomOfficeData.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using System;

    using CDP4Common.MetaInfo;

    using CDP4JsonSerializer;

    /// <summary>
    /// an abstract super class that shall be derived to store data in office documents
    /// as custom XML parts.
    /// </summary>
    public abstract class CustomOfficeData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomOfficeData"/> class
        /// </summary>
        protected CustomOfficeData()
        {
            this.MetaDataProvider = StaticMetadataProvider.GetMetaDataProvider;
            if (this.MetaDataProvider == null)
            {
                throw new InvalidOperationException($"The {nameof(IMetaDataProvider)} could not be found.");
            }

            this.Serializer = new Cdp4JsonSerializer(this.MetaDataProvider, new Version(1, 0, 0));
        }

        /// <summary>
        /// Gets the <see cref="IMetaDataProvider"/>
        /// </summary>
        protected IMetaDataProvider MetaDataProvider { get; private set; }

        /// <summary>
        /// Gets the <see cref="Cdp4JsonSerializer"/>
        /// </summary>
        protected Cdp4JsonSerializer Serializer { get; private set; }
    }
}
