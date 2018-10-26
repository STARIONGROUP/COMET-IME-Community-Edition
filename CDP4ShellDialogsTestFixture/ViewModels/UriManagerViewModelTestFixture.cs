// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriManagerViewModelTestFixture.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    
    /// <summary>
    /// Suite of tests for the <see cref="UriManagerViewModel"/> class
    /// </summary>
    [TestFixture]
    public class UriManagerViewModelTestFixture
    {
        private FileInfo testfile = new FileInfo("TestData\\uris.json");
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
            List<UriConfig> rows = new List<UriConfig>();
            rows.Add(new UriConfig() { Alias="Alias0", Uri="Uri0", DalType="Web" });
            configurator.Write(rows);

            // Ensure the generated file is the same as the testing one
            Assert.IsTrue(File.ReadAllText(this.testfile.FullName).GetHashCode() ==
                          File.ReadAllText(configurator.ConfigurationFilePath).GetHashCode() );
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
            Assert.DoesNotThrow(() => configViewModel.CloseCommand.Execute(0));

        }

        [Test]
        public void VerifyThatUriManagerViewModelWorks()
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
            configViewModel.DeleteRowCommand.Execute(null);
            configViewModel.SelectedUriRow = row2;
            configViewModel.DeleteRowCommand.Execute(null);             
            Assert.IsTrue(configViewModel.UriRowList.Count == initialSize);

            // Re-Save config file.
            configViewModel.ApplyCommand.Execute(0);
            Assert.IsTrue(File.Exists(new UriConfigFileHandler().ConfigurationFilePath));
        }
    }
}