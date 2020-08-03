// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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


namespace CDP4IME
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Windows;

    using CDP4Composition;

    using CDP4IME.Modularity;

    using DevExpress.Xpf.Core;

    using ExceptionReporting;

    using NLog;

    /// <summary>
    /// Interaction logic for Application
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// A NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Called when the applications starts. Makes a distinction between debug and release mode
        /// </summary>
        /// <param name="e">
        /// the event argument
        /// </param>
        protected override void OnStartup(StartupEventArgs e)
        {
            // Set the Theme of the application
            ThemeManager.ApplicationThemeName = Theme.SevenName;
            AppliedTheme.ThemeName = Theme.SevenName;
            base.OnStartup(e);

            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            if (UpdateInstaller.CheckInstallAndVerifyIfTheImeShallShutdown())
            {
                return;
            }

            DXSplashScreen.Show<Views.SplashScreenView>();
            DXSplashScreen.SetState("Starting CDP4");
            
#if (DEBUG)
            RunInDebugMode();
#else
            RunInReleaseMode();
#endif
            
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;
            
            DXSplashScreen.SetState("Preparing Main Window");
            
            Current.MainWindow.Show();
            DXSplashScreen.Close();
        }
        /// <summary>
        /// Run the application in debug mode. Unhandled Exceptions are not caught.
        /// The application will crash
        /// </summary>
        private static void RunInDebugMode()
        {
            var bootstrapper = new CDP4IMEBootstrapper();

            try
            {
                bootstrapper.Run();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();

                foreach (var loaderException in ex.LoaderExceptions)
                {
                    sb.AppendLine(loaderException.Message);

                    if (loaderException is FileNotFoundException fileNotFoundException)
                    {
                        if (!string.IsNullOrEmpty(fileNotFoundException.FusionLog))
                        {
                            sb.AppendLine("FusionLog: ");
                            sb.AppendLine(fileNotFoundException.FusionLog);
                        }
                    }

                    sb.AppendLine();
                }

                var errorMessage = sb.ToString();
                logger.Fatal(errorMessage, ex);
                throw new ApplicationException(errorMessage);
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
                throw;
            }
        }

        /// <summary>
        /// Run the application in release mode. Unhandled Exceptions are caught and shown to the user.
        /// The application will exit.
        /// </summary>
        private static void RunInReleaseMode()
        {
            AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;
            
            try
            {
                var bootstrapper = new CDP4IMEBootstrapper();
                bootstrapper.Run();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        /// <summary>
        /// Event handler
        /// </summary>
        /// <param name="sender">
        /// the sender of the exception
        /// </param>
        /// <param name="e">
        /// an instance of <see cref="UnhandledExceptionEventArgs"/> that carries the <see cref="Exception"/>
        /// </param>
        private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        /// <summary>
        /// Handles the provided exception by showing it to the end-user
        /// </summary>
        /// <param name="ex">
        /// the exception that is being handled
        /// </param>
        private static void HandleException(Exception ex)
        {
            if (ex == null)
            {
                return;
            }

            var exceptionReporter = new ExceptionReporter();
            exceptionReporter.Show(ex);

            Environment.Exit(1);
        }

        /// <summary>
        /// Handles the UpdateThemeName event
        /// </summary>
        /// <param name="sender">
        /// The sender of the event
        /// </param>
        /// <param name="e">
        /// The event arguments
        /// </param>
        private void OnAppStartup_UpdateThemeName(object sender, StartupEventArgs e)
        {
            DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
        }        

    }
}
