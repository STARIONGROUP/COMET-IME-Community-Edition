// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingPreferenceHelper.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
// 
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Nathanael Smiechowski, Ahmed Ahmed, Simon Wood
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
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//    Lesser General Public License for more details.
// 
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Utilities
{
    using System.Collections.Generic;

    using CDP4Common.CommonData;

    using Newtonsoft.Json;

    /// <summary>
    /// Helper for handling Thing Preference json property of any <see cref="Thing"/>
    /// </summary>
    public static class ThingPreferenceHelper
    {
        /// <summary>
        /// Retrives the <see cref="Dictionary{TKey,TValue}"/> version of the ThingPreference property
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/></param>
        /// <param name="dictionary">The returned <see cref="Dictionary{TKey,TValue}"/></param>
        /// <returns>True if dictionary is there and not null</returns>
        private static bool GetThingPreferenceDictionary(Thing thing, out Dictionary<string, string> dictionary)
        {
            if (string.IsNullOrWhiteSpace(thing.ThingPreference))
            {
                dictionary = null;
                return false;
            }

            try
            {
                dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(thing.ThingPreference);
            }
            catch
            {
                dictionary = null;
                return false;
            }

            return dictionary != null;
        }

        /// <summary>
        /// Gets a value of the preference by key
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/></param>
        /// <param name="key">The key to look up</param>
        /// <returns>The value</returns>
        public static string GetThingPreference(this Thing thing, string key)
        {
            if (!GetThingPreferenceDictionary(thing, out var dictionary))
            {
                return null;
            }

            return dictionary.TryGetValue(key, out var result) ? result : null;
        }

        /// <summary>
        /// Set a key value pair. Overrides the value if the key is set.
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/></param>
        /// <param name="key">The key to look up</param>
        /// <param name="value">The value to set</param>
        public static void SetThingPreference(this Thing thing, string key, string value)
        {
            if (!GetThingPreferenceDictionary(thing, out var dictionary))
            {
                dictionary = new Dictionary<string, string>();
            }

            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }

            var serialized = JsonConvert.SerializeObject(dictionary);

            if (!string.IsNullOrWhiteSpace(serialized))
            {
                thing.ThingPreference = serialized;
            }
        }
    }
}
