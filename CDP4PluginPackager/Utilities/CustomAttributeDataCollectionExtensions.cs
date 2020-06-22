// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomAttributeDataCollectionExtensions.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4PluginPackager.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides extensions for <see cref="IEnumerable{CustomAttributeData}"/> to help retrieve <see cref="Assembly"/> <see cref="Attribute"/>
    /// </summary>
    public static class CustomAttributeDataCollectionExtensions
    {
        /// <summary>
        /// Retrieves the value of a attribute type on a <see cref="IEnumerable{CustomAttributeData}"/>
        /// </summary>
        /// <typeparam name="T">the assembly <see cref="Attribute"/> type to retriveve</typeparam>
        /// <param name="collection">the collection to query on</param>
        /// <returns>Return the value of the queried attribute</returns>
        public static string QueryAssemblySpecificInfo<T>(this IEnumerable<CustomAttributeData> collection)
        {
            return collection.FirstOrDefault(a => a.Constructor.ReflectedType == typeof(T))?.ConstructorArguments[0].Value.ToString();
        }
    }
}
