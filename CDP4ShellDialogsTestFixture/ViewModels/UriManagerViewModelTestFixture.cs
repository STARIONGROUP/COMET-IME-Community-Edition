// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriManagerViewModelTestFixture.cs" company="RHEA System S.A.">
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

namespace CDP4ShellDialogsTestFixture.ViewModels
{
    using CDP4Composition.Utilities;

    using CDP4ShellDialogs.ViewModels;
    
    using NUnit.Framework;
   
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    /// <summary>
    /// Suite of tests for the <see cref="UriManagerViewModel"/> class
    /// </summary>
    [TestFixture]
    public class UriManagerViewModelTestFixture
    {
        private FileInfo testfile = new FileInfo(Path.Combine(new System.Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)).LocalPath, "TestData\\uris.json"));
        private FileInfo tempfile = new FileInfo("TestData\\tmp\\temporaluri.json");

        [SetUp]
        public void SetUp()
        {
            UriConfigFileHandler.AppDataFolder = "TestData";
            UriConfigFileHandler.ConfigFileRelativeFolder = @"tmp\";
            UriConfigFileHandler.ConfigFileName = @"temporaluri.json";
        }

        [TearDown]
        public void TearDown()
        {
            // Delete tmp dir
            File.Delete(this.tempfile.FullName);
            Directory.Delete(this.tempfile.DirectoryName);
        }

        [Test]
        public void VerifyThatUriConfigFileHandlerWorks()
        {
            var configurator = new UriConfigFileHandler();

            // Open file that does not exist
            Assert.IsFalse(File.Exists(configurator.ConfigurationFilePath));
            Assert.IsEmpty(configurator.Read());

            // Try to write null
            Assert.Throws<ArgumentNullException>(() => configurator.Write(null));

            // Write row the same as the testing one
            var rows = new List<UriConfig>
            {
                new UriConfig
                {
                    Alias = "Alias0", 
                    Uri = "Uri0", 
                    DalType = "Web"
                }
            };

            configurator.Write(rows);

            // Normalize line endings, because JSON.Net serialization is a bit inconsistent on handling indentation and line endings on different platforms
            var testFileContent = File.ReadAllText(this.testfile.FullName).Replace("\r\n", "\n");
            var configFileContent = File.ReadAllText(configurator.ConfigurationFilePath).Replace("\r\n", "\n");

            // Ensure the generated file is the same as the testing one
            Assert.AreEqual(testFileContent.Length, configFileContent.Length,
                $"test file content:\n{testFileContent}\n, config file content:\n{configFileContent}\n");

            Assert.AreEqual(testFileContent,  configFileContent, 
                $"test file content:\n{testFileContent}\n, config file content:\n{configFileContent}\n" );
        }

        [Test]
        public void VerifyThatUriManagerViewModelCanBeConstructed()
        {
            // Create tmp file
            Directory.CreateDirectory(this.tempfile.DirectoryName);
            this.testfile.CopyTo(this.tempfile.FullName, true);

            var configViewModel = new UriManagerViewModel();

            Assert.NotNull(configViewModel);
            Assert.NotNull(configViewModel.UriRowList);

            Assert.IsTrue(configViewModel.UriRowList[0].Alias.Equals("Alias0"));
            Assert.IsTrue(configViewModel.UriRowList[0].Uri.Equals("Uri0"));
            Assert.IsTrue(configViewModel.UriRowList[0].DalType == CDP4Dal.Composition.DalType.Web);

            // Close viewmodel
            Assert.DoesNotThrowAsync(async () => await configViewModel.CloseCommand.Execute());
        }

        [Test]
        public async Task VerifyThatUriManagerViewModelWorks()
        {
            // Create tmp file
            Directory.CreateDirectory(this.tempfile.DirectoryName);
            this.testfile.CopyTo(this.tempfile.FullName, true);

            // Create an empty UriManagerViewModel
            var configViewModel = new UriManagerViewModel();
            int initialSize = configViewModel.UriRowList.Count;

            // Create a new UriRowViewModel from a UriConfig
            var cl1 = new UriConfig() { Alias = "Alias1", Uri = "Uri1", DalType = "Web" };
            var row1 = new UriRowViewModel() { UriConfig = cl1 };
            configViewModel.UriRowList.Add(row1);            
            Assert.IsNotNull(configViewModel.UriRowList);
            Assert.IsTrue(configViewModel.UriRowList[initialSize].Alias.Equals("Alias1"));
            Assert.IsTrue(configViewModel.UriRowList[initialSize].Uri.Equals("Uri1"));
            Assert.IsTrue(configViewModel.UriRowList[initialSize].DalType == CDP4Dal.Composition.DalType.Web);

            // Create a new UriRowViewModel 
            var row2 = new UriRowViewModel() { Uri = "Uri2", DalType = CDP4Dal.Composition.DalType.File };
            configViewModel.UriRowList.Add(row2);

            // Test get Name
            Assert.IsTrue(row2.Name.Equals("Uri2"));
            row2.Alias = "Alias2";
            Assert.IsTrue(row2.Name.Equals("Alias2"));

            // Test get UriConfig from a UriRowViewModel
            var cl2 = row2.UriConfig;
            Assert.IsTrue(cl2.Alias.Equals("Alias2"));
            Assert.IsTrue(cl2.Uri.Equals("Uri2"));
            Assert.IsTrue(cl2.DalType.Equals("File"));

            // Test there are 2 existing DalTypes
            Assert.IsTrue(configViewModel.DalTypesList.Count >= 2);

            // Test removing elemnt
            configViewModel.SelectedUriRow = row1;
            await configViewModel.DeleteRowCommand.Execute();
            configViewModel.SelectedUriRow = row2;
            await configViewModel.DeleteRowCommand.Execute();             
            Assert.IsTrue(configViewModel.UriRowList.Count == initialSize);

            // Re-Save config file.
            await configViewModel.ApplyCommand.Execute();
            Assert.IsTrue(File.Exists(new UriConfigFileHandler().ConfigurationFilePath));
        }
    }
}