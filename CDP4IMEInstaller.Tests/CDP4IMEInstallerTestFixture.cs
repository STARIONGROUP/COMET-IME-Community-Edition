// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CDP4IMEInstallerTestFixture.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using NUnit.Framework;

namespace CDP4IMEInstaller.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    using DotLiquid;
    using DotLiquid.NamingConventions;

    /// <summary>
    /// Suite of tests that check the compatibility of the CDP4IMEInstaller project
    /// </summary>
    public class CDP4IMEInstallerTestFixture
    {
        private KnownAssembliesClassDrop knownAssembliesClassDrop;
        private string devExpressVersion = "v20.2";
        private string currentConfiguration;

        [SetUp]
        public void Setup()
        {
#if DEBUG
            this.currentConfiguration = "Debug";
#else
            this.currentConfiguration = "Release";
#endif

            Template.NamingConvention = new CSharpNamingConvention();

            Template.RegisterSafeType(typeof(KnownAssembliesClassDrop), new[]
            {
                nameof(KnownAssembliesClassDrop.WxsObjects)
            });

            Template.RegisterSafeType(typeof(WxsObject), new[]
            {
                nameof(WxsObject.AssemblyName),
                nameof(WxsObject.Guid),
                nameof(WxsObject.ComponentId),
                nameof(WxsObject.FileId),
            });

            this.knownAssembliesClassDrop = new KnownAssembliesClassDrop(this.devExpressVersion);
        }

        [Test]
        public void VerifyThatDevExpressWxsIsCorrect()
        {
            var template = Template.Parse(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "DevExpress.wxs.liquid")));

            // the contents of the code variable can be copied to DevExpress.wxs manually when changes are found.
            var code = template.Render(Hash.FromAnonymousObject(new { KnownAssemblies = this.knownAssembliesClassDrop }));

            var myProjectLocation = this.FindParentFileLocation(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), "CDP4IMEInstaller.Tests.csproj");
            Assert.IsNotNull(myProjectLocation, "CDP4IMEInstaller.Tests.csproj file not found");

            var devExpressWxsLocation = Path.Combine(myProjectLocation.Parent?.FullName, "CDP4IMEInstaller" , "DevExpress.wxs");
            Assert.IsTrue(File.Exists(devExpressWxsLocation), $"File {devExpressWxsLocation} not found");

            var text = File.ReadAllText(devExpressWxsLocation);
            Assert.AreEqual(code, text);
        }

        [Test]
        public void VerifyThatAllDevExpressAssembliesAreKnownAssemblies()
        {
            var knownAssemblies = this.knownAssembliesClassDrop.Assemblies;

            var myProjectLocation = this.FindParentFileLocation(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), "CDP4IMEInstaller.Tests.csproj");
            Assert.IsNotNull(myProjectLocation, "CDP4IMEInstaller.Tests.csproj file not found");

            var separator = Path.DirectorySeparatorChar;

            var frameworkVersion = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)).Name;

            var allAssemblies = 
                Directory.GetFiles(
                    myProjectLocation.Parent?.FullName, 
                    "DevExpress*.dll", SearchOption.AllDirectories)
                    .Where(x => x.Contains($"{separator}bin{separator}{this.currentConfiguration}{separator}{frameworkVersion}{separator}DevExpress")).ToList();

            var imeAssemblies = allAssemblies.Where(x => x.Contains($"CDP4IME{separator}bin")).ToList();
            var otherAssemblies = allAssemblies.Where(x => !x.Contains($"CDP4IME{separator}bin")).ToList();

            // A file in a plugin folder should always be added to the IME project
            var inOtherButNotInIme = otherAssemblies.Select(Path.GetFileName).Where(x => !imeAssemblies.Select(Path.GetFileName).Contains(x)).ToList();
            
            // Files in the IME folder should always be added to the installer
            var inImeButNotInKnownAssemblies = imeAssemblies.Select(Path.GetFileName).Where(x => !knownAssemblies.Contains(x)).ToList();

            // Should not happen. Every DevExpress file known to the installer should be in IME project, otherwise remove it from the installer.
            var inKnownAssembliesButNotInIme = knownAssemblies.Where(x => !imeAssemblies.Select(Path.GetFileName).Contains(x)).ToList();

            // This is OK
            var inImeButNotInOther = imeAssemblies.Select(Path.GetFileName).Where(x => !otherAssemblies.Select(Path.GetFileName).Contains(x));

            CollectionAssert.IsEmpty(inOtherButNotInIme,
                $"There are DevExpress files found in projects that are not found in the IME project's {this.currentConfiguration} folder: {string.Join(", ", inOtherButNotInIme)} ");

            CollectionAssert.IsEmpty(inImeButNotInKnownAssemblies, 
                $"There are DevExpress files in the IME project's {this.currentConfiguration} folder that are not known to the installer project: {string.Join(", ", inImeButNotInKnownAssemblies)}");

            CollectionAssert.IsEmpty(inKnownAssembliesButNotInIme,
                $"There are DevExpress files that are known to the installer project, but are not found in the IME project's {this.currentConfiguration} folder (rebuild all needed?): {string.Join(", ", inKnownAssembliesButNotInIme)}");
        }

        [Test]
        public void VerifyThatRtfLicenseFileIsOk()
        {
            var knownAssembliesWithoutExtension = this.knownAssembliesClassDrop.Assemblies.Select(x => x.Replace(".dll", "")).ToList();
            var devExpressTextBuilder = new StringBuilder();

            foreach (var assembly in knownAssembliesWithoutExtension)
            {
                devExpressTextBuilder.Append($"If you modify this Program, or any covered work, by linking or combining it with {assembly} " +
                                             "(or a modified version of that library), containing parts covered by the terms of DevExpress EULA, " +
                                             "the licensors of this Program grant you additional permission to convey the resulting work.\r\n");
            }

            //The content of the textToCopiedManuallyToLicenseFile variable can be manually copied to the License.rtf file in the CDPIME-CE project
            var textToCopiedManuallyToLicenseFile = devExpressTextBuilder.ToString();

            var myProjectLocation = this.FindParentFileLocation(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), "CDP4IMEInstaller.Tests.csproj");
            Assert.IsNotNull(myProjectLocation, "CDP4IMEInstaller.Tests.csproj file not found");

            var licenseRtfLocation = Path.Combine(myProjectLocation.Parent?.FullName, "CDP4IME", "license.rtf");
            Assert.IsTrue(File.Exists(licenseRtfLocation), $"File {licenseRtfLocation} not found");

            var rtfText = RichTextStripper.StripRichTextFormat(File.ReadAllText(licenseRtfLocation));

            var expectedTimes = knownAssembliesWithoutExtension.Count * 2;
            var foundTimes = Regex.Matches(rtfText, "DevExpress").Count;

            Assert.AreEqual(expectedTimes, foundTimes,
                $"Expected to find the text 'DevExpress' {knownAssembliesWithoutExtension.Count * 2} times, but {foundTimes} were found instead.");

            foreach (var assembly in knownAssembliesWithoutExtension)
            {
                Assert.IsTrue(rtfText.Contains(assembly), $"Assembly {assembly} not found");
            }
        }

        public DirectoryInfo FindParentFileLocation(DirectoryInfo currentDirectory, string filenameWildCard)
        {
            if (currentDirectory.GetFiles(filenameWildCard).Any())
            {
                return currentDirectory;
            }

            return currentDirectory.Parent != null ? this.FindParentFileLocation(currentDirectory.Parent, filenameWildCard) : null;
        }
    }
}
