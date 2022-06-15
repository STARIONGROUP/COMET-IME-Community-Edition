// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateInstaller.cs" company="RHEA System S.A.">
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

namespace COMET.Modularity
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using CDP4Composition.Modularity;

    using COMET.Services;
    using COMET.ViewModels;
    using COMET.Views;

    /// <summary>
    /// The <see cref="UpdateInstaller"/> is responsible to check all the CDP4 download folders and to install/update the availables user-selected plugins 
    /// </summary>
    public static class UpdateInstaller
    {
        /// <summary>
        /// The message that will be displayed when ever a new ime installer is found
        /// </summary>
        private const string ImeNewVersionMessage = "A new IME version has been found! Would you like to install it now?";

        /// <summary>
        /// Check for any update available and run the plugin installer
        /// </summary>
        /// <param name="viewInvoker">An <see cref="IViewInvokerService"/></param>
        /// <param name="processRunner">An <see cref="IProcessRunnerService"/></param>
        /// <returns>An Assert whether the IME have to shut down</returns>
        public static bool CheckInstallAndVerifyIfTheImeShallShutdown(IViewInvokerService viewInvoker = null, IProcessRunnerService processRunner = null)
        {
            var imeUpdate = CheckForImeUpdate();

            if (imeUpdate != null && (viewInvoker ?? new ViewInvokerService()).ShowMessageBox(ImeNewVersionMessage, "IME Update", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                RunInstaller(imeUpdate, processRunner);
                return true;
            }

            var updatablePlugins = PluginUtilities.GetDownloadedInstallablePluginUpdate().ToList();

            if (updatablePlugins.Any())
            {
                var pluginInstallerView = new UpdateDownloaderInstaller() { DataContext = new UpdateDownloaderInstallerViewModel(updatablePlugins) };
                (viewInvoker ?? new ViewInvokerService()).ShowDialog(pluginInstallerView);
            }

            return false;
        }

        /// <summary>
        /// Look for any downloaded installer in the download folder
        /// </summary>
        /// <returns>a <see cref="string"/> path to the installer</returns>
        private static string CheckForImeUpdate()
        {
            var downloadDirectory = new UpdateFileSystemService().ImeDownloadPath;
            var installers = downloadDirectory.Exists ? downloadDirectory.EnumerateFiles().ToArray() : null;

            if (installers?.Any() != true)
            {
                return null;
            }

            var lastVersion = installers.OrderBy(f => f.Name).LastOrDefault();

            if (VerifyIfTheInstallerCanBeACandidate(lastVersion))
            {
                return lastVersion?.FullName;
            }

            CleanUpDownloadedImeInstallers(downloadDirectory);
            return null;
        }

        /// <summary>
        /// Check if the provided <see cref="FileInfo"/> can be a candidate
        /// </summary>
        /// <param name="lastVersion">the installer file</param>
        /// <returns>An assert whether the installer can be run to update the IME</returns>
        private static bool VerifyIfTheInstallerCanBeACandidate(FileInfo lastVersion)
        {
            var (prefix, platform, version, extension) = SplitUpInstallerName(lastVersion);

            var currentAssembly = Assembly.GetExecutingAssembly().GetName();

            var isItANewerVersion = currentAssembly.Version < version;
            var isTheNameCompliant = extension == ".msi" && prefix.Contains("CDP4IME");

            var isThePlatformCompatible = currentAssembly.ProcessorArchitecture == ProcessorArchitecture.MSIL
                                           || currentAssembly.ProcessorArchitecture == platform;

            return isItANewerVersion && isThePlatformCompatible && isTheNameCompliant;
        }

        /// <summary>
        /// Delete any installer when no candidate are found
        /// </summary>
        /// <param name="downloadDirectory">The download directory</param>
        private static void CleanUpDownloadedImeInstallers(DirectoryInfo downloadDirectory)
        {
            downloadDirectory.Delete(true);
        }

        /// <summary>
        /// Splits up the installer file name and return it as tuple
        /// </summary>
        /// <returns>a tuple containing parts of the name</returns>
        private static (string prefix, ProcessorArchitecture platform, Version version, string extension) SplitUpInstallerName(FileInfo file)
        {
            var splittedUp = Path.GetFileNameWithoutExtension(file.FullName).Split('-');
            var editionAndPlaform = splittedUp[1].Split('.');

            if (!Enum.TryParse(editionAndPlaform[1].ToUpper(), out ProcessorArchitecture platform))
            {
                platform = ProcessorArchitecture.Amd64;
            }

            return (prefix: splittedUp[0], platform: platform, version: new Version(splittedUp[2]), extension: file.Extension);
        }

        /// <summary>
        /// Closes the IME and run MSI in order to install the new version
        /// </summary>
        /// <param name="installerPath">The path to the msi</param>
        /// <param name="processRunner">The <see cref="IProcessRunnerService"/></param>
        private static void RunInstaller(string installerPath, IProcessRunnerService processRunner)
        {
            var runner = processRunner ?? new ProcessRunnerService();
            runner.RunAsAdmin(installerPath);
        }
    }
}
