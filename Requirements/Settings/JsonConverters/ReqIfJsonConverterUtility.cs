// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConverterUtility.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Requirements.Settings.JsonConverters
{
    using CDP4Dal;

    using Newtonsoft.Json;

    using ReqIFSharp;

    /// <summary>
    /// Provides utility method for JsonConverters
    /// </summary>
    public static class ReqIfJsonConverterUtility
    {
        /// <summary>
        /// Initializes necessary <see cref="JsonConverter"/>  for the requirement plugin
        /// </summary>
        /// <param name="reqIf">The <see cref="ReqIF"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <returns>An array of <see cref="JsonConverter"/></returns>
        public static JsonConverter[] BuildConverters(ReqIF reqIf, ISession session)
        {
            return new JsonConverter[]
            {
                new DataTypeDefinitionMapConverter(reqIf, session),
                new SpecObjectTypeMapConverter(reqIf, session),
                new SpecRelationTypeMapConverter(reqIf, session),
                new RelationGroupTypeMapConverter(reqIf, session),
                new SpecificationTypeMapConverter(reqIf, session),
            };
        }

        /// <summary>
        /// Initializes necessary <see cref="JsonConverter"/>  for the requirement plugin, use this overload only for writting
        /// </summary>
        /// <returns>An array of <see cref="JsonConverter"/></returns>
        public static JsonConverter[] BuildConverters()
        {
            return new JsonConverter[]
            {
                new DataTypeDefinitionMapConverter(),
                new SpecObjectTypeMapConverter(),
                new SpecRelationTypeMapConverter(),
                new RelationGroupTypeMapConverter(),
                new SpecificationTypeMapConverter(),
            };
        }
    }
}
