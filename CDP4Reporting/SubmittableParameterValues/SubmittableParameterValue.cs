// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubmittableParameterValue.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Reporting.SubmittableParameterValues
{
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// <see cref="SubmittableParameterValue"/> holds data needed to submit parameter values to a model.
    /// </summary>
    public class SubmittableParameterValue
    {
        /// <summary>
        /// The key value of a path setting that can be read from a report control's tag property
        /// </summary>
        public const string PathKey = "path";

        /// <summary>
        /// The key value of an exact path setting that can be read from a report control's tag property
        /// </summary>
        public const string ExactPathKey = "exactpath";

        /// <summary>
        /// The name of the control that originally held the (text) value
        /// </summary>
        public string ControlName { get; set; }

        /// <summary>
        /// The value as a <see cref="string"/>
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The <see cref="NestedParameter.Path"/> of the <see cref="ParameterBase"/> in the Product Tree.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Indicates that the option part of the path should match exactly
        /// </summary>
        public bool IsExactOptionPath { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="SubmittableParameterValue"/>
        /// </summary>
        /// <param name="path">
        /// The <see cref="NestedParameter.Path"/> of the <see cref="ParameterBase"/> in the Product Tree.
        /// </param>
        /// <param name="isExactOptionPath">
        /// Indication if the path is an exact Option path;
        /// </param>
        public SubmittableParameterValue(string path, bool isExactOptionPath)
        {
            this.Path = path;
            this.IsExactOptionPath = isExactOptionPath;
        }

        /// <summary>
        /// Tries to extract a value from a comma separated string that contains
        /// key/value pairs divided by an '=' character
        /// </summary>
        /// <param name="extractFrom">The <see cref="string"/> to extract the value from</param>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        /// <returns>true if the path was found, otherwise false</returns>
        public static bool TryExtractValue(string extractFrom, string key, out string value)
        {
            value = null;
            var extractArray = extractFrom.Split(',');

            if (extractArray.Length == 0)
            {
                return false;
            }

            foreach (var tag in extractArray)
            {
                if (!tag.Contains("="))
                {
                    continue;
                }

                var tagSplit = tag.Split('=');

                if (!tagSplit[0].ToLower().Equals(key))
                {
                    continue;
                }

                value = tagSplit[1].Trim();

                return true;
            }

            return false;
        }
    }
}
