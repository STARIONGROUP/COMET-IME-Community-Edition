// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellDictionaryService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using DevExpress.Xpf.SpellChecker;
    using DevExpress.XtraSpellChecker;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="SpellDictionaryService"/> is to provide access to spelling dictionaries
    /// for <see cref="SpellChecker"/>s.
    /// </summary>
    [Export(typeof(ISpellDictionaryService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SpellDictionaryService : ISpellDictionaryService
    {
        /// <summary>
        /// The name of the application directoryInfo
        /// </summary>
        private const string ApplicationFolderName = "RHEA\\CDP4";

        /// <summary>
        /// The name of the directoryInfo where spelling and grammar dictionaries are to be stored.
        /// </summary>
        private const string DictionaryFolderName = "dictionaries";

        /// <summary>
        /// A <see cref="Logger"/> instance
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="CultureInfo"/> for which there are dictionaries available on the current system
        /// </summary>
        private readonly List<CultureInfo> cultures = new List<CultureInfo>();

        /// <summary>
        /// Backing field for the <see cref="SpellCheckerDictionaries"/> property
        /// </summary>
        private readonly List<SpellCheckerOpenOfficeDictionary> spellCheckerDictionaries = new List<SpellCheckerOpenOfficeDictionary>();

        /// <summary>
        /// Backing field for the <see cref="ActiveCulture"/> property;
        /// </summary>
        private CultureInfo activeCulture;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SpellDictionaryService"/> class.
        /// </summary>
        public SpellDictionaryService()
        {
            this.LoadEmbeddedDictionaries();
            this.LoadAvailableCultures();
        }

        /// <summary>
        /// The event that is raised when the active <see cref="CultureInfo"/> is changed
        /// </summary>
        public event ActiveCultureInfoChangedHandler CultureInfoChanged;

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> objects of the available dictionaries.
        /// </summary>
        public IEnumerable<CultureInfo> Cultures
        {
            get
            {
                return this.cultures;
            }
        }

        /// <summary>
        /// Gets the <see cref="SpellCheckerOpenOfficeDictionary"/> that are loaded by the <see cref="SpellDictionaryService"/>
        /// </summary>
        public IEnumerable<SpellCheckerOpenOfficeDictionary> SpellCheckerDictionaries 
        {
            get
            {
                return this.spellCheckerDictionaries;
            }
        }

        /// <summary>
            /// Sets the active <see cref="CultureInfo"/> of the <see cref="SpellDictionaryService"/>
            /// </summary>
            /// <param name="cultureInfo">
            /// The <see cref="CultureInfo"/> that is to be set as the <see cref="ActiveCulture"/>
            /// </param>        
        public void SetActiveCulture(CultureInfo cultureInfo)
        {
            if (this.activeCulture.Equals(cultureInfo))
            {
                return;
            }

            if (this.cultures.Contains(cultureInfo))
            {
                var dictionary = this.spellCheckerDictionaries.SingleOrDefault(dic => dic.Culture.Equals(cultureInfo));
                if (dictionary == null)
                {
                    dictionary = this.LoadDictionary(cultureInfo);
                }

                if (dictionary != null)
                {
                    this.spellCheckerDictionaries.Add(dictionary);
                    this.activeCulture = cultureInfo;

                    if (this.CultureInfoChanged != null)
                    {
                        var e = new CultureInfoChangedEventArgs(cultureInfo, dictionary);
                        this.CultureInfoChanged(this, e);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> object that is the active one.
        /// </summary>
        public CultureInfo ActiveCulture
        {
            get
            {
                return this.activeCulture;
            }
        }

        /// <summary>
        /// Gets the <see cref="SpellCheckerOpenOfficeDictionary"/> that corresponds to the <see cref="ActiveCulture"/>
        /// </summary>
        public SpellCheckerOpenOfficeDictionary ActiveDictionary
        {
            get
            {
                return this.spellCheckerDictionaries.SingleOrDefault(d => d.Culture.Equals(this.activeCulture));
            }
        }

        /// <summary>
        /// Loads the dictionaries that are available as embedded resource
        /// </summary>
        private void LoadEmbeddedDictionaries()
        {
            var dict = Assembly.GetExecutingAssembly().GetManifestResourceStream("CDP4Composition.Resources.Dictionaries.en_GB.dic");
            var grammar = Assembly.GetExecutingAssembly().GetManifestResourceStream("CDP4Composition.Resources.Dictionaries.en_GB.aff");

            var dictionary = new SpellCheckerOpenOfficeDictionary();
            dictionary.LoadFromStream(dict, grammar, null);
            this.spellCheckerDictionaries.Add(dictionary);

            var culture = new CultureInfo("en-GB");
            dictionary.Culture = culture;
            this.cultures.Add(culture);

            this.activeCulture = culture;
        }

        /// <summary>
        /// Loads the <see cref="CultureInfo"/> of the dictionaries that are available
        /// </summary>
        private void LoadAvailableCultures()
        {
            string applicationdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dictionaryfolder = Path.Combine(applicationdata, string.Format("{0}\\{1}", ApplicationFolderName, DictionaryFolderName));

            var directoryInfo = new DirectoryInfo(dictionaryfolder);

            if (directoryInfo.Exists)
            {
                foreach (var dir in directoryInfo.EnumerateDirectories())
                {
                    try
                    {
                        var cultureInfo = new CultureInfo(dir.Name);
                        this.cultures.Add(cultureInfo);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }
            else
            {
                logger.Debug("The dictionaries directoryInfo {0} does not exist. The dictionaries are not loaded", dictionaryfolder);
            }
        }

        /// <summary>
        /// Loads and returns the <see cref="SpellCheckerOpenOfficeDictionary"/> for the specified <see cref="CultureInfo"/>
        /// </summary>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> for which the <see cref="SpellCheckerOpenOfficeDictionary"/> has to be loaded
        /// </param>
        /// <returns>
        /// An instance of <see cref="SpellCheckerOpenOfficeDictionary"/> if it can be found, null otherwise.
        /// </returns>
        private SpellCheckerOpenOfficeDictionary LoadDictionary(CultureInfo cultureInfo)
        {
            string applicationdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string dictionaryfolder = Path.Combine(applicationdata, string.Format("{0}\\{1}", ApplicationFolderName, DictionaryFolderName));

            var directoryInfo = new DirectoryInfo(dictionaryfolder);
            var directories = directoryInfo.EnumerateDirectories(cultureInfo.Name);
            var dictionaryDirectory = directories.SingleOrDefault();

            if (dictionaryDirectory == null)
            {
                return null;
            }

            var dicFiles = dictionaryDirectory.GetFiles("*.dic");
            var dicFile = dicFiles.First();
            var dicstream = dicFile.OpenRead();

            var affFiles = dictionaryDirectory.GetFiles("*.aff");
            var affFile = affFiles.First();
            var affstream = affFile.OpenRead();

            var dictionary = new SpellCheckerOpenOfficeDictionary();
            dictionary.LoadFromStream(dicstream, affstream, null);

            var culture = new CultureInfo(dictionaryDirectory.Name);
            dictionary.Culture = culture;

            return dictionary;
        }
    }
}
