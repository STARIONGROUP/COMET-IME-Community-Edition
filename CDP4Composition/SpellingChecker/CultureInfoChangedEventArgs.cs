// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CultureInfoChangedEventArgs.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System.Globalization;
    using DevExpress.XtraSpellChecker;

    /// <summary>
    /// The active <see cref="CultureInfo"/> changed handler.
    /// </summary>
    /// <param name="source">
    /// The source that raised the event
    /// </param>
    /// <param name="e">
    /// The <see cref="CultureInfoChangedEventArgs"/> that carries the new active <see cref="CultureInfo"/> and <see cref="SpellCheckerOpenOfficeDictionary"/>
    /// </param>
    public delegate void ActiveCultureInfoChangedHandler(object source, CultureInfoChangedEventArgs e);

    /// <summary>
    /// The purpose of the <see cref="CultureInfoChangedEventArgs"/> is to act as the event argument for
    /// event that is raised when the active <see cref="CultureInfo"/> of the <see cref="ISpellDictionaryService"/> is changed.
    /// </summary>
    public class CultureInfoChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CultureInfoChangedEventArgs"/> class.
        /// </summary>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> instance that is the payload of the current event argument
        /// </param>
        /// <param name="dictionary">
        /// The <see cref="SpellCheckerOpenOfficeDictionary"/> that is the payload of the current event argument
        /// </param>
        public CultureInfoChangedEventArgs(CultureInfo culture, SpellCheckerOpenOfficeDictionary dictionary)
        {
            this.Culture = culture;
            this.Dictionary = dictionary;
        }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> that is the new active <see cref="CultureInfo"/> of the <see cref="ISpellDictionaryService"/>
        /// </summary>
        public CultureInfo Culture { get; private set; }

        /// <summary>
        /// Gets the <see cref="SpellCheckerOpenOfficeDictionary"/> that is the new active <see cref="SpellCheckerOpenOfficeDictionary"/> 
        /// of the <see cref="ISpellDictionaryService"/>
        /// </summary>
        public SpellCheckerOpenOfficeDictionary Dictionary { get; private set; }
    }
}
