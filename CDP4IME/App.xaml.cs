// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using CDP4Composition.Modularity;

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

            new PluginUpdateInstaller().CheckAndRunUpdater();

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
