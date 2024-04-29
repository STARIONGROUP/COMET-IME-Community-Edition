// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISpellDictionaryService.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System.Collections.Generic;
    using System.Globalization;
    using DevExpress.Xpf.SpellChecker;
    using DevExpress.XtraSpellChecker;

    /// <summary>
    /// The purpose of the <see cref="ISpellDictionaryService"/> is to provide access to spelling dictionaries
    /// for <see cref="SpellChecker"/>s.
    /// </summary>
    public interface ISpellDictionaryService
    {
        /// <summary>
        /// Gets the <see cref="CultureInfo"/> objects of the available dictionaries.
        /// </summary>
        IEnumerable<CultureInfo> Cultures { get; }

        /// <summary>
        /// The event that is raised when the active <see cref="CultureInfo"/> is changed
        /// </summary>
        event ActiveCultureInfoChangedHandler CultureInfoChanged;

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> object that is the active one.
        /// </summary>
        CultureInfo ActiveCulture { get; }

        /// <summary>
        /// Gets the <see cref="SpellCheckerOpenOfficeDictionary"/> that corresponds to the <see cref="ActiveCulture"/>
        /// </summary>
        SpellCheckerOpenOfficeDictionary ActiveDictionary { get; }

        /// <summary>
        /// Gets the <see cref="SpellCheckerOpenOfficeDictionary"/> that are loaded by the <see cref="SpellDictionaryService"/>
        /// </summary>
        IEnumerable<SpellCheckerOpenOfficeDictionary> SpellCheckerDictionaries { get;  }

        /// <summary>
        /// Sets the active <see cref="CultureInfo"/> of the <see cref="SpellDictionaryService"/>
        /// </summary>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> that is to be set as the <see cref="ActiveCulture"/>
        /// </param>        
        void SetActiveCulture(CultureInfo cultureInfo);
    }
}
