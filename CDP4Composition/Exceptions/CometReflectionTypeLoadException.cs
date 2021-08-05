// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CometReflectionTypeLoadException.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
// 
//    This file is part of CDP4-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
// 
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
// 
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Exceptions
{
    using System;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Specific <see cref="Exception"/> class 
    /// </summary>
    public class CometReflectionTypeLoadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsException"/> class.
        /// </summary>
        public CometReflectionTypeLoadException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionTypeLoadException"/> class.
        /// </summary>
        /// <param name="reflectionTypeLoadException">
        /// The 
        /// </param>
        public CometReflectionTypeLoadException(ReflectionTypeLoadException reflectionTypeLoadException) 
            : base(BuildMessage(reflectionTypeLoadException), reflectionTypeLoadException)
        {
        }

        /// <summary>
        /// Adds information from the original <see cref="ReflectionTypeLoadException.LoaderExceptions"/> to the <see cref="CometReflectionTypeLoadException.Message "/> property.
        /// </summary>
        /// <param name="reflectionTypeLoadException">The original <see cref="ReflectionTypeLoadException"/></param>
        /// <returns>
        /// a string containing the loader exception description
        /// </returns>
        private static string BuildMessage(ReflectionTypeLoadException reflectionTypeLoadException)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(reflectionTypeLoadException.Message);

            foreach (var exception in reflectionTypeLoadException.LoaderExceptions)
            {
                stringBuilder.AppendLine($" ---> {exception.Message}");
            }

            return stringBuilder.ToString();
        }
    }
}
