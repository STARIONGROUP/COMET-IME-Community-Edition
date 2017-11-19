// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DictionaryServiceTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Tests.SpellingChecker
{
    using System;    
    using System.IO;
    using System.Linq;
    using NUnit.Framework;

    /// <summary>
    /// Suite of tests for the <see cref="SpellDictionaryService"/>
    /// </summary>
    [TestFixture]
    public class DictionaryServiceTestFixture
    {
        /// <summary>
        /// The <see cref="SpellDictionaryService"/> under test
        /// </summary>
        private SpellDictionaryService dictionaryService;

        [SetUp]
        public void SetUp()
        {
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var directoryInfo = Directory.CreateDirectory(folderPath + "\\RHEA\\CDP4\\dictionaries\\en-US");
            try
            {
                File.Copy("SpellingChecker\\en_US.aff", directoryInfo.FullName + "\\en_US.aff");
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }

            try
            {
                File.Copy("SpellingChecker\\en_US.dic", directoryInfo.FullName + "\\en_US.dic");
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
        }

        [TearDown]
        public void TearDown()
        {
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            try
            {
                Directory.Delete(folderPath + "\\RHEA\\CDP4\\dictionaries", true);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
        }

        [Test]
        public void VerifyThatEmbeddedDictionariesAreLoaded()
        {
            this.dictionaryService = new SpellDictionaryService();
            Assert.NotNull(this.dictionaryService.SpellCheckerDictionaries);

            var dict = this.dictionaryService.SpellCheckerDictionaries.ToList();
            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.Any(d => d.Culture.Name == "en-GB"));

            var cultureInfos = this.dictionaryService.Cultures.ToList();
            Assert.IsTrue(cultureInfos.Any(ci => ci.Name == "en-GB"));
        }

        [Test]
        public void VerifyThatAvailableCulturesAreLoaded()
        {
            this.dictionaryService = new SpellDictionaryService();

            var cultureInfos = this.dictionaryService.Cultures.ToList();
            Assert.IsTrue(cultureInfos.Any(ci => ci.Name == "en-US"));
        }

        [Test]
        public void VerifyThatActiveCultureInfoCanBeSet()
        {
            this.dictionaryService = new SpellDictionaryService();

            var cultureInfos = this.dictionaryService.Cultures.ToList();

            var cultureInfo = cultureInfos.SingleOrDefault(ci => ci.Name == "en-US");
            Assert.IsNotNull(cultureInfo);

            this.dictionaryService.SetActiveCulture(cultureInfo);

            var dict = this.dictionaryService.SpellCheckerDictionaries.ToList();
            var dictExists = dict.Any(d => d.Culture.Equals(cultureInfo));

            Assert.IsTrue(dictExists);

            var activeDictionary = this.dictionaryService.ActiveDictionary;
            Assert.AreEqual(activeDictionary.Culture, cultureInfo);
        }

        [Test]
        public void VerifyThatEventIsRaisedWhenActiveCultureIsUpdated()
        {
            this.dictionaryService = new SpellDictionaryService();

            var cultureInfo = this.dictionaryService.Cultures.SingleOrDefault(ci => ci.Name == "en-US");
            Assert.IsNotNull(cultureInfo);

            this.dictionaryService.CultureInfoChanged += (source, args) =>
                {
                    Assert.AreEqual(this.dictionaryService, source);
                    Assert.AreEqual(cultureInfo, args.Culture);
                    Assert.NotNull(args.Dictionary);
                };

            this.dictionaryService.SetActiveCulture(cultureInfo);
        }
    }
}
