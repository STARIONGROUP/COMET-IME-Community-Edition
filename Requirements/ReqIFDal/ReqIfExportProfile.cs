// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReqIfExportProfile.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ReqIFDal
{
    /// <summary>
    /// The interoperability profile that a ReqIF export targets. Both profiles produce valid OMG ReqIF; they differ
    /// only in the conventions that ease consumption by other tools.
    /// </summary>
    public enum ReqIfExportProfile
    {
        /// <summary>
        /// The prostep ivip / DOORS / Capella interoperability profile: the requirement text is additionally exported
        /// as rich-text (XHTML) and the identity attributes use the reserved <c>ReqIF.*</c> names so that those tools
        /// map them to their built-in fields automatically.
        /// </summary>
        DoorsCapella,

        /// <summary>
        /// Plain OMG ReqIF with the COMET-native attribute names and no rich-text (XHTML) attribute.
        /// </summary>
        Omg
    }
}
