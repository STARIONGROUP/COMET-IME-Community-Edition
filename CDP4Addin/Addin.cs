// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Addin.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4AddinCE
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4AddinCE.Settings;
    using CDP4AddinCE.Utils;

    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services.AppSettingService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CDP4OfficeInfrastructure;

    using DevExpress.Xpf.Core;

    using Microsoft.Practices.ServiceLocation;

    using NetOffice.ExcelApi.Tools;
    using NetOffice.OfficeApi;
    using NetOffice.Tools;
    using NetOffice.Tools.Native;

    using NLog;

    using ReactiveUI;

    using ExceptionReporting;

    using MessageBox = System.Windows.Forms.MessageBox;

    /// <summary>
    /// The <see cref="Addin"/> provides CDP4 integration with the Office Suite. It self-registers in the registry and
    /// provides the Fluent XML Ribbon and call-back implementations for the Fluent XML Ribbon controls
    /// </summary>
    [COMAddin("CDP4 COMET-CE Office Add-in", "The CDP4 COMET-CE Office Add-in provides CDP4 COMET application integration with Microsoft Office Suite", 3)]
    [Guid("FD48B640-1D3F-4922-854B-C69028CA469E")]
    [ProgId("CDP4CE.Addin")]
    [RegistryLocation(RegistrySaveLocation.CurrentUser)]
    public class Addin : COMAddin
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The ribbon XML
        /// </summary>
        private string fluentRibbonXml;

        /// <summary>
        /// Gets or sets the instance of the <see cref="IFluentRibbonManager"/> used to control the content and behavior of the Office Fluent Ribbon.
        /// </summary>
        internal IFluentRibbonManager FluentRibbonManager { get; set; }

        /// <summary>
        /// Excel application instance
        /// </summary>
        private NetOffice.ExcelApi.Application excelApplication;

        /// <summary>
        /// The list of current loaded custom task panes
        /// </summary>
        private readonly Dictionary<Guid, IdentifiableCustomTaskPane> customTaskPanes = new Dictionary<Guid, IdentifiableCustomTaskPane>();

        /// <summary>
        /// A wrapper class that provides access to the different office application instances
        /// </summary>
        private IOfficeApplicationWrapper officeApplicationWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="Addin"/> class.
        /// </summary>
        /// <remarks>
        /// order of start events:
        /// 1. OnConnection
        /// 2. OnAddInsUpdate
        /// 3. GetCustomUI
        /// 4. OnStartupComplete
        /// order of stop events
        /// 1. OnBeginShutdown
        /// 2. OnDisconnection
        /// </remarks>
        public Addin()
        {
            logger.Debug("starting COMET-CE addin");

            this.PreloadAssemblies();
            this.RedirectAssemblies();
            this.SetupIdtExtensibility2Events();
            this.SetupEventListeners();

            // Set the Theme of the application
            ThemeManager.ApplicationThemeName = Theme.SevenName;

            logger.Debug("COMET-CE addin started");
        }

        /// <summary>
        /// Error handler for the Registration methods
        /// </summary>
        /// <param name="methodKind">
        /// The registration method where the exception occurred
        /// </param>
        /// <param name="exception">
        /// The exception that occurred
        /// </param>
        [RegisterErrorHandler]
        public static void RegisterErrorHandler(RegisterErrorMethodKind methodKind, Exception exception)
        {
            MessageBox.Show($"An register error occurend in {methodKind.ToString()}", "COMET-CE.Addin");
        }

        /// <summary>
        /// Loads the XML markup, either from an XML customization file or from XML markup embedded in the procedure, that customizes the Ribbon user interface
        /// </summary>
        /// <param name="ribbonID">
        /// The unique id of the Ribbon to be loaded
        /// </param>
        /// <returns>
        /// the ribbon UI
        /// </returns>
        public override string GetCustomUI(string ribbonID)
        {
            var ribbonXml = this.fluentRibbonXml;
            logger.Debug(ribbonXml);

            return ribbonXml;
        }

        /// <summary>
        /// Executes the OnAction callback that is invoked from the <see cref="Office.IRibbonControl"/>
        /// </summary>
        /// <param name="control">
        /// The ribbon control that invokes the callback
        /// </param>
        public async Task OnAction(IRibbonControl control)
        {
            logger.Trace("{0} OnAction", control.Id);

            try
            {
                await this.FluentRibbonManager.OnAction(control.Id, control.Tag);
            }
            catch (Exception ex)
            {
                // NOTE: manually handle exceptions as the UI specific dispatcher thread does not work
                HandleException(ex);
            }

            if (this.RibbonUI != null)
            {
                this.RibbonUI.Invalidate();
            }
            else
            {
                logger.Warn("The RibbonUI is null and cannot be invalidated");
            }
        }

        /// <summary>
        /// The GetContent callback function is called when a DynamicMenu menu is dropped
        /// </summary>
        /// <param name="control">
        /// The ribbon control that invokes the callback
        /// </param>
        /// <returns>
        /// ribbon XML containing the controls
        /// </returns>
        public string GetContent(IRibbonControl control)
        {
            logger.Trace("{0} GetContent", control.Id);

            try
            {
                var content = this.FluentRibbonManager.GetContent(control.Id, control.Tag);
                return content;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }

            return string.Empty;
        }

        /// <summary>
        /// Executes the GetLabel callback that is invoked from the <see cref="Office.IRibbonControl"/>
        /// </summary>
        /// <param name="control">
        /// The ribbon control that invokes the callback
        /// </param>
        /// <returns>
        /// a string with the content of the label
        /// </returns>
        public string GetLabel(IRibbonControl control)
        {
            logger.Trace("{0} GetLabel", control.Id);
            return this.FluentRibbonManager.GetLabel(control.Id, control.Tag);
        }

        /// <summary>
        /// Executes the GetEnabled callback that is invoked from the <see cref="Office.IRibbonControl"/>
        /// </summary>
        /// <param name="control">
        /// The ribbon control that invokes the callback
        /// </param>
        /// <returns>
        /// true if enabled, false if not enabled
        /// </returns>
        public bool GetEnabled(IRibbonControl control)
        {
            logger.Trace("{0} GetEnabled", control.Id);
            return this.FluentRibbonManager.GetEnabled(control.Id, control.Tag);
        }

        /// <summary>
        /// Executes the GetEnabled callback that is invoked from a Toggle control on the <see cref="Office.IRibbonControl"/>
        /// </summary>
        /// <param name="control">
        /// The ribbon control that invokes the callback
        /// </param>
        /// <returns>
        /// true if pressed, false if not pressed
        /// </returns>
        public bool GetPressed(IRibbonControl control)
        {
            logger.Trace("{0} GetPressed", control.Id);
            return this.FluentRibbonManager.GetPressed(control.Id, control.Tag);
        }

        /// <summary>
        /// Executes the GetVisible callback that is invoked from the <see cref="Office.IRibbonControl"/>
        /// </summary>
        /// <param name="control">
        /// The ribbon control that invokes the callback
        /// </param>
        /// <returns>
        /// returns true if visible, false if not
        /// </returns>
        public bool GetVisible(IRibbonControl control)
        {
            logger.Trace("{0} GetVisible", control.Id);
            return this.FluentRibbonManager.GetVisible(control.Id, control.Tag);
        }

        /// <summary>
        /// Executes the GetImage callback that is invoked from the <see cref="Office.IRibbonControl"/>
        /// </summary>
        /// <param name="control">
        /// The ribbon control that invokes the callback
        /// </param>
        /// <returns>
        /// an image if found, null otherwise
        /// </returns>
        public Image GetImage(IRibbonControl control)
        {
            logger.Trace("{0} GetImage", control.Id);
            return this.FluentRibbonManager.GetImage(control.Id, control.Tag);
        }

        /// <summary>
        /// ICustomTaskPaneConsumer implementation
        /// </summary>
        /// <param name="CTPFactoryInst">
        /// factory proxy from host application
        /// </param>
        public override void CTPFactoryAvailable(object CTPFactoryInst)
        {
            base.CTPFactoryAvailable(CTPFactoryInst);
            this.TaskPaneFactory = new ICTPFactory(this.Factory, null, CTPFactoryInst);
        }

        /// <summary>
        /// Exception handler for unhandled exceptions
        /// </summary>
        /// <param name="methodKind">The method where the error occurred.</param>
        /// <param name="exception">The exception that occurred.</param>
        protected override void OnError(ErrorMethodKind methodKind, Exception exception)
        {
            this.Utils.Dialog.ShowError(exception, "Unexpected state in COMET-CE.Addin " + methodKind.ToString());
        }

        /// <summary>
        /// Preload specific assemblies so they can be found at runtime.
        /// There is a problem loading the assemblies in Office x64 on a development machine.
        /// It cannot find the library if it isn't preloaded somewhere on Excel startup.
        /// </summary>
        private void PreloadAssemblies()
        {
            //These assemblies are present in the main folder (bin), but are not used (yet) bij the Addin itself
            logger.Trace("Pre-register Addin Assemblies");
            Assembly.Load("Markdown.Xaml");
            Assembly.Load("System.Net.Http.Formatting");
            Assembly.Load("System.Threading.Tasks.Extensions");
        }

        /// <summary>
        /// execute <see cref="RedirectAssembly"/> for specific assemblies
        /// </summary>
        private void RedirectAssemblies()
        {
            logger.Trace("Microsoft.Practices.ServiceLocation");
            var serviceLocationTargetVersion = new Version("1.3.0.0");
            this.RedirectAssembly("Microsoft.Practices.ServiceLocation", serviceLocationTargetVersion, "31bf3856ad364e35");

            logger.Trace("System.Windows.Interactivity");
            var windowsInteractivity = new Version("4.5.0.0");
            this.RedirectAssembly("System.Windows.Interactivity", windowsInteractivity, "31bf3856ad364e35");

            logger.Trace("System.Threading.Tasks.Extensions");
            var threadingTasksExtensions = new Version("4.5.4.0");
            this.RedirectAssembly("System.Threading.Tasks.Extensions", threadingTasksExtensions, "31bf3856ad364e35");
        }

        /// <summary>
        /// Adds an AssemblyResolve handler to redirect all attempts to load a specific assembly name to the specified version.
        /// </summary>
        /// <param name="shortName">
        /// The name of the Assembly that needs to be redirected.
        /// </param>
        /// <param name="targetVersion">
        /// The target <see cref="Version"/> of the redirected assembly.
        /// </param>
        /// <param name="publicKeyToken">
        /// The public Key Token of the redirected assembly
        /// </param>
        /// <see cref="http://blog.slaks.net/2013-12-25/redirecting-assembly-loads-at-runtime/"/>
        private void RedirectAssembly(string shortName, Version targetVersion, string publicKeyToken)
        {
            ResolveEventHandler handler = null;

            handler = (sender, args) =>
            {
                // Use latest strong name & version when trying to load SDK assemblies
                var requestedAssembly = new AssemblyName(args.Name);

                if (requestedAssembly.Name != shortName)
                {
                    return null;
                }

                var requestingAssembly = args.RequestingAssembly == null ? "(unknown)" : args.RequestingAssembly.FullName;
                logger.Debug("Redirecting assembly load of {0} loaded by {1}", args.Name, requestingAssembly);

                requestedAssembly.Version = targetVersion;
                requestedAssembly.SetPublicKeyToken(new AssemblyName("x, PublicKeyToken=" + publicKeyToken).GetPublicKeyToken());
                requestedAssembly.CultureInfo = CultureInfo.InvariantCulture;

                AppDomain.CurrentDomain.AssemblyResolve -= handler;

                return Assembly.Load(requestedAssembly);
            };

            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }

        /// <summary>
        /// create event-handlers for the <see cref="IDTExtensibility2"/> events
        /// </summary>
        private void SetupIdtExtensibility2Events()
        {
            this.OnConnection += this.AddinOnConnection;
            this.OnAddInsUpdate += this.AddinOnAddInsUpdate;
            this.OnStartupComplete += this.AddinOnStartupComplete;
            this.OnBeginShutdown += this.AddinOnBeginShutdown;
            this.OnDisconnection += this.AddinOnDisconnection;
        }

        /// <summary>
        /// Setup event listeners on the <see cref="CDPMessageBus"/>
        /// </summary>
        private void SetupEventListeners()
        {
            CDPMessageBus.Current.Listen<NavigationPanelEvent>()
                .Where(x => x.PanelStatus == PanelStatus.Open)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.HandleOpenPanel);

            CDPMessageBus.Current.Listen<NavigationPanelEvent>()
                .Where(x => x.PanelStatus == PanelStatus.Closed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.HandleClosePanel);

            CDPMessageBus.Current.Listen<SessionEvent>()
                .Where(x => x.Status == SessionStatus.Closed)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.HandleCloseSession);
        }

        /// <summary>
        /// Handles the <see cref="NavigationPanelEvent"/> with <see cref="PanelStatus.Open"/>
        /// </summary>
        /// <param name="navigationPanelEvent">
        /// The event that carries the view, view-model combination and <see cref="PanelStatus"/>
        /// </param>
        [ExcludeFromCodeCoverage]
        private void HandleOpenPanel(NavigationPanelEvent navigationPanelEvent)
        {
            logger.Debug("Opening Panel {0}", navigationPanelEvent.ViewModel);

            try
            {
                if (!(navigationPanelEvent.View is UIElement uiElement))
                {
                    return;
                }

                var identifier = navigationPanelEvent.ViewModel.Identifier;

                var taskPaneExists = this.customTaskPanes.TryGetValue(identifier, out var identifiableCustomTaskPane);

                if (taskPaneExists)
                {
                    identifiableCustomTaskPane.CustomTaskPane.Visible = !identifiableCustomTaskPane.CustomTaskPane.Visible;
                }
                else
                {
                    var title = navigationPanelEvent.ViewModel.Caption;

                    var dockPosition = navigationPanelEvent.ViewModel.TargetName.ToDockPosition();
                    logger.Trace("Create new Task Pane with title {0}", title);
                    var taskPane = this.TaskPaneFactory.CreateCTP("CDP4AddinCE.TaskPaneWpfHostControl", title);
                    taskPane.DockPosition = dockPosition;
                    taskPane.Width = 300;
                    taskPane.Visible = true;

                    if (taskPane is CustomTaskPane customTaskPane)
                    {
                        customTaskPane.VisibleStateChangeEvent += this.CustomTaskPane_VisibleStateChangeEvent;
                    }

                    if (taskPane.ContentControl is TaskPaneWpfHostControl wpfHostControl)
                    {
                        wpfHostControl.SetContent(uiElement);
                    }

                    identifiableCustomTaskPane = new IdentifiableCustomTaskPane(identifier, taskPane);

                    this.customTaskPanes.Add(identifier, identifiableCustomTaskPane);
                }
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : string.Empty;

                logger.Fatal(ex, $"handle open panel failed: {ex.Message} - {innerExceptionMessage}");
            }
        }

        /// <summary>
        /// Handles a <see cref="CustomTaskPane"/>'s VisibleStateChangeEvent
        /// </summary>
        /// <param name="customTaskPaneInst">
        /// The <see cref="_CustomTaskPane"/>
        /// </param>
        private void CustomTaskPane_VisibleStateChangeEvent(_CustomTaskPane customTaskPaneInst)
        {
            if (customTaskPaneInst.Visible)
            {
                return;
            }

            var identifier = this.customTaskPanes.SingleOrDefault(x => x.Value.CustomTaskPane == customTaskPaneInst).Key;
            var hidePanelEvent = new HidePanelEvent(identifier);
            CDPMessageBus.Current.SendMessage(hidePanelEvent);
        }

        /// <summary>
        /// Handles the <see cref="NavigationPanelEvent"/> with <see cref="PanelStatus.Closed"/>
        /// </summary>
        /// <param name="navigationPanelEvent">
        /// The event that carries the view, view-model combination and <see cref="PanelStatus"/>
        /// </param>
        private void HandleClosePanel(NavigationPanelEvent navigationPanelEvent)
        {
            logger.Debug("Closing Panel {0}", navigationPanelEvent.ViewModel);

            try
            {
                var identifier = navigationPanelEvent.ViewModel.Identifier;

                var taskPaneExists = this.customTaskPanes.TryGetValue(identifier, out var identifiableCustomTaskPane);

                if (taskPaneExists)
                {
                    identifiableCustomTaskPane.CustomTaskPane.Visible = false;
                    identifiableCustomTaskPane.Dispose();
                    this.customTaskPanes.Remove(identifier);
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "handle close panel failed");
            }
        }

        /// <summary>
        /// Handles the Close <see cref="SessionEvent"/>
        /// </summary>
        /// <param name="sessionEvent">
        /// The <see cref="SessionEvent"/> that has the <see cref="ISession"/> that is to be closed as payload
        /// </param>
        private void HandleCloseSession(SessionEvent sessionEvent)
        {
            logger.Debug("clean up panels from session {0}", sessionEvent.Session.DataSourceUri);

            try
            {
                foreach (var customTaskPane in this.customTaskPanes.Values)
                {
                    customTaskPane.CustomTaskPane.Visible = false;
                    customTaskPane.Dispose();
                }

                this.customTaskPanes.Clear();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "handle close session failed");
            }
            finally
            {
                if (this.RibbonUI != null)
                {
                    this.RibbonUI.Invalidate();
                }
            }
        }

        /// <summary>
        /// The OnConnection method is called when a COM add-in is loaded into the environment.
        /// This method is the main entry point for the COM add-in because it provides the Application object
        /// from the Office application's object model that the add-in will use to communicate with the Office application.
        /// </summary>
        /// <param name="application">
        /// The application object of the Office application passed as an object.
        /// Because IDTExtensibility2 is a general-purpose interface, this has to be an object rather than a strongly typed parameter.
        /// This object can be cast to the Application object type of the Office application.</param>
        /// <param name="connectMode">
        /// Specification how the COM add-in was loaded.
        /// </param>
        /// <param name="addInInst">
        /// An object representing the COM add-in. This can be cast to a COMAddIn object from the office DLL PIA in the Microsoft.Office.Core namespace.
        /// </param>
        /// <param name="custom">
        /// An array of object that the host application can use to provide additional data. None of the Office applications set this value
        /// </param>
        private void AddinOnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            this.Factory.Console.WriteLine("AddinOnConnection");
            logger.Trace("AddinOnConnection");

            try
            {
                logger.Trace("new up bootstrapper");
                var bootstrapper = new CDP4AddinBootstrapper();
                logger.Trace("run bootstrapper");
                bootstrapper.Run();

                logger.Trace("InitializeMefImports");
                this.InitializeMefImports();

                logger.Trace("Initialize FluentRibbon");
                logger.Debug(this.fluentRibbonXml);

                logger.Trace("Initialize Application");
                this.InitializeApplication(application);

                logger.Trace("set theme");
                ThemeManager.ApplicationThemeName = Theme.SevenName;
                AppliedTheme.ThemeName = Theme.SevenName;
            }
            catch (CompositionException compositionException)
            {
                logger.Fatal(compositionException, $"CompositionException: {compositionException.Message}");

                foreach (var error in compositionException.Errors)
                {
                    logger.Fatal(error.Exception, $"CompositionException Error: {error.Element.DisplayName} {error.Description}");
                }
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Bootstrapper exception: ");
                this.Utils.Dialog.ShowError(ex, "Unexpected state in COMET-CE.Addin");
            }
        }

        /// <summary>
        /// Handles the provided exception by showing it to the end-user
        /// </summary>
        /// <param name="ex">
        /// The exception that is being handled
        /// </param>
        private static void HandleException(Exception ex)
        {
            logger.Error(ex);

            var thread = new Thread(() =>
            {
                var exceptionReporter = new ExceptionReporter();
                exceptionReporter.Show(ex);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>
        /// Initialize the Office application objects, setup event handlers
        /// </summary>
        /// <param name="application">
        /// The application object that is used in the current addin
        /// </param>
        private void InitializeApplication(object application)
        {
            var excel = application as NetOffice.ExcelApi.Application;

            if (excel == null)
            {
                return;
            }

            // set the excel application object
            this.excelApplication = excel;
            this.excelApplication.DisplayAlerts = true;
            this.excelApplication.ScreenUpdating = true;

            // set the excel instance to the office application wrapper
            this.officeApplicationWrapper.Excel = excel;

            // create event handlers
            this.excelApplication.WorkbookActivateEvent += this.OnWorkbookActivateEvent;
            this.excelApplication.WorkbookDeactivateEvent += this.OnWorkbookDeactivateEvent;

            logger.Debug("The current addin is loaded for Excel");
        }

        /// <summary>
        /// Destruct the Office application objects and remove event handlers
        /// </summary>
        private void DestructApplication()
        {
            if (this.excelApplication == null)
            {
                return;
            }

            // unregister events
            this.excelApplication.WorkbookActivateEvent -= this.OnWorkbookActivateEvent;
            this.excelApplication.WorkbookDeactivateEvent -= this.OnWorkbookDeactivateEvent;

            // set excel instance to null
            this.officeApplicationWrapper.Excel = null;
            this.excelApplication = null;
        }

        /// <summary>
        /// the event-handler for the <see cref="Excel.Application.WorkbookActivateEvent"/>
        /// </summary>
        /// <param name="workbook">
        /// The workbook that was activated
        /// </param>
        private void OnWorkbookActivateEvent(NetOffice.ExcelApi.Workbook workbook)
        {
            logger.Debug("Workbook {0} activated", workbook.Name);

            if (this.RibbonUI != null)
            {
                this.RibbonUI.Invalidate();
            }
        }

        /// <summary>
        /// the event-handler for the <see cref="Excel.Application.WorkbookDeactivateEvent"/>
        /// </summary>
        /// <param name="workbook">
        /// The workbook that was deactivated
        /// </param>
        private void OnWorkbookDeactivateEvent(NetOffice.ExcelApi.Workbook workbook)
        {
            logger.Debug("Workbook {0} deactivated", workbook.Name);

            if (this.RibbonUI != null)
            {
                this.RibbonUI.Invalidate();
            }
        }

        /// <summary>
        /// Initializes the MEF instantiated services and managers
        /// </summary>
        private void InitializeMefImports()
        {
            var panelNavigationService = ServiceLocator.Current.GetInstance<IPanelNavigationService>();
            this.FluentRibbonManager = ServiceLocator.Current.GetInstance<IFluentRibbonManager>();
            var thingDialogNavigationService = ServiceLocator.Current.GetInstance<IThingDialogNavigationService>();
            var dialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            var pluginSettingsService = ServiceLocator.Current.GetInstance<IPluginSettingsService>();

            this.FluentRibbonManager.IsActive = true;
            var appSettingsService = ServiceLocator.Current.GetInstance<IAppSettingsService<AddinAppSettings>>();
            var ribbonpart = new AddinRibbonPart(0, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService, appSettingsService);
            this.FluentRibbonManager.RegisterRibbonPart(ribbonpart);
            this.fluentRibbonXml = this.FluentRibbonManager.GetFluentXml();

            this.officeApplicationWrapper = ServiceLocator.Current.GetInstance<IOfficeApplicationWrapper>();
        }

        /// <summary>
        /// The OnAddInsUpdate method is called when any COM add-in is loaded or unloaded in the Office application.
        /// </summary>
        /// <param name="custom">
        /// An array of object that the host application can use to provide additional data. None of the Office applications set this value.
        /// </param>
        private void AddinOnAddInsUpdate(ref Array custom)
        {
            this.Factory.Console.WriteLine("AddinOnAddInsUpdate");
            logger.Trace("AddinOnAddInsUpdate");
        }

        /// <summary>
        /// The OnStartupComplete method is called when the Office application has completed starting up and has loaded all the COM add-ins
        /// that were registered to load on startup
        /// </summary>
        /// <param name="custom">
        /// array of parameters
        /// </param>
        private void AddinOnStartupComplete(ref Array custom)
        {
            this.Factory.Console.WriteLine("AddinOnStartupComplete");
            logger.Trace("AddinOnStartupComplete");
        }

        /// <summary>
        /// The OnBeginShutdown method is called on a connected COM add-in when the Office application is being shut down
        /// </summary>
        /// <param name="custom">
        /// An array of object that the host application can use to provide additional data. None of the Office applications set this value.
        /// </param>
        private void AddinOnBeginShutdown(ref Array custom)
        {
            this.Factory.Console.WriteLine("AddinOnBeginShutdown");
            logger.Trace("AddinOnBeginShutdown");
        }

        /// <summary>
        /// The OnDisconnection method is called when a COM add-in is unloaded from the application either because the application is shutting
        ///  down or because the user disabled the COM add-in using the COM Add-Ins dialog
        /// </summary>
        /// <param name="removeMode">
        /// specification why the COM add-in was unloaded.
        /// </param>
        /// <param name="custom">
        /// An array of object that the host application can use to provide additional data. None of the Office applications set this value
        /// </param>
        private void AddinOnDisconnection(ext_DisconnectMode removeMode, ref Array custom)
        {
            this.DestructApplication();

            this.Factory.Console.WriteLine("AddinOnDisconnection");
            logger.Trace("AddinOnDisconnection");
        }
    }
}
