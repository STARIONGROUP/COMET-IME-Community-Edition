// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISpecialTermsService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;

    /// <summary>
    /// The <see cref="ISpecialTermsService"/> handles dictionaries and words that are required for special textbox decoration.
    /// These can be, for example, <see cref="Term"/> or forbidden words.
    /// </summary>
    public interface ISpecialTermsService
    {
        /// <summary>
        /// Gets the dictionary of terms and their definitions.
        /// </summary>
        Dictionary<string, Definition> TermDefinitionDictionary { get; }

        /// <summary>
        /// Gets the list of <see cref="Term"/> that need to be highlighted.
        /// </summary>
        IEnumerable<string> TermsList { get; }

        /// <summary>
        /// Rebuilds the dictionaries.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        void RebuildDictionaries(ISession session);
    }
}
