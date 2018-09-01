// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Addin.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4AddinCE
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Drawing;
    using System.Globalization;    
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;

    using CDP4AddinCE.Utils;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Events;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4OfficeInfrastructure;
    using DevExpress.Xpf.Core;
    using Microsoft.Practices.ServiceLocation;
    using NetOffice;
    using NetOffice.ExcelApi;
    using NetOffice.ExcelApi.Tools;
    using NetOffice.Tools;
    using NetOffice.WordApi.Enums;
    using NLog;
    using ReactiveUI;
    using Excel = NetOffice.ExcelApi;
    using MessageBox = System.Windows.Forms.MessageBox;
    using Office = NetOffice.OfficeApi;

    /// <summary>
    /// The <see cref="Addin"/> provides CDP4 integration with the Office Suite. It self-registers in the registry and
    /// provides the Fluent XML Ribbon and call-back implementations for the Fluent XML Ribbon controls
    /// </summary>
    [COMAddin("CDP4-CE Office Add-in", "The CDP4-CE Office Add-in provides CDP4 application integration with Microsoft Office Suite", 3)]
    [Guid("FD48B640-1D3F-4922-854B-C69028CA469E"), ProgId("CDP4CE.Addin")]
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
        /// Word application instance
        /// </summary>
        private NetOffice.WordApi.Application wordApplication;

        /// <summary>
        /// The list of current loaded custom task panes
        /// </summary>
        private Dictionary<Guid, IdentifiableCustomTaskPane> customTaskPanes = new Dictionary<Guid, IdentifiableCustomTaskPane>();
        
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
            logger.Debug("starting CDP4-CE addin");

            this.RedirectAssemblies();
            this.SetupIdtExtensibility2Events();
            this.SetupEventListeners();

            // Set the Theme of the application
            DevExpress.Xpf.Core.ThemeManager.ApplicationThemeName = DevExpress.Xpf.Core.Theme.SevenName;

            logger.Debug("CDP4-CE addin started");
        }

        /// <summary>
        /// Gets the Ribbon instance
        /// </summary>
        internal Office.IRibbonUI RibbonUi { get; set; }

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
            MessageBox.Show("An register error occurend in " + methodKind.ToString(), "CDP4-CE.Addin");
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
        /// Executes the "OnLoadRibbonUi" call-back that is invoked from the <see cref="Office.IRibbonUI"/>
        /// </summary>
        /// <param name="ribbonUi">
        /// The ribbon control that invokes the callback
        /// </param>
        /// <remarks>
        /// defined in RibbonUI XML to get an instance for ribbon GUI.
        /// </remarks>
        public void OnLoadRibbonUi(Office.IRibbonUI ribbonUi)
        {
            this.RibbonUi = ribbonUi;
        }

        /// <summary>
        /// Executes the OnAction callback that is invoked from the <see cref="Office.IRibbonControl"/>
        /// </summary>
        /// <param name="control">
        /// The ribbon control that invokes the callback
        /// </param>
        public async void OnAction(Office.IRibbonControl control)
        {
            logger.Trace("{0} OnAction", control.Id);
            try
            {
                await this.FluentRibbonManager.OnAction(control.Id, control.Tag);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
            
            this.RibbonUi.Invalidate();
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
        public string GetContent(Office.IRibbonControl control)
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
        public string GetLabel(Office.IRibbonControl control)
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
        public bool GetEnabled(Office.IRibbonControl control)
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
        public bool GetPressed(Office.IRibbonControl control)
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
        public bool GetVisible(Office.IRibbonControl control)
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
        public Image GetImage(Office.IRibbonControl control)
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
            this.TaskPaneFactory = new NetOffice.OfficeApi.ICTPFactory(Factory, null, CTPFactoryInst);
        }

        /// <summary>
        /// Exception handler for unhandled exceptions
        /// </summary>
        /// <param name="methodKind">The method where the error occurred.</param>
        /// <param name="exception">The exception that occurred.</param>
        protected override void OnError(ErrorMethodKind methodKind, Exception exception)
        {
            Utils.Dialog.ShowError(exception, "Unexpected state in CDP4-CE.Addin " + methodKind.ToString());
        }

        /// <summary>
        /// execute <see cref="RedirectAssembly"/> for specific assemblies
        /// </summary>
        private void RedirectAssemblies()
        {
            logger.Trace("Microsoft.Practices.ServiceLocation");
            var serviceLocationTargetVersion = new Version("1.3.0.0");
            this.RedirectAssembly("Microsoft.Practices.ServiceLocation", serviceLocationTargetVersion, "31bf3856ad364e35");

            logger.Trace("Microsoft.Practices.Prism.PubSubEvents");
            var pubSubEventsTargetVersion = new Version("1.1.0.0");
            this.RedirectAssembly("Microsoft.Practices.Prism.PubSubEvents", pubSubEventsTargetVersion, "31bf3856ad364e35");

            logger.Trace("Microsoft.Practices.Prism.SharedInterfaces");
            var pubSubEventsSharedInterfaces = new Version("1.1.1.0");
            this.RedirectAssembly("Microsoft.Practices.Prism.SharedInterfaces", pubSubEventsSharedInterfaces, "31bf3856ad364e35");

            logger.Trace("System.Windows.Interactivity");
            var windowsInteractivity = new Version("4.5.0.0");
            this.RedirectAssembly("System.Windows.Interactivity", windowsInteractivity, "31bf3856ad364e35");
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
        private void HandleOpenPanel(NavigationPanelEvent navigationPanelEvent)
        {
            logger.Debug("Opening Panel {0}", navigationPanelEvent.ViewModel);

            try
            {
                var uiElement = navigationPanelEvent.View as UIElement;
                if (uiElement != null)
                {
                    var identifier = navigationPanelEvent.ViewModel.Identifier;

                    IdentifiableCustomTaskPane identifiableCustomTaskPane = null;
                    var taskPaneExists = this.customTaskPanes.TryGetValue(identifier, out identifiableCustomTaskPane);
                    if (taskPaneExists)
                    {
                        identifiableCustomTaskPane.CustomTaskPane.Visible = !identifiableCustomTaskPane.CustomTaskPane.Visible;
                    }
                    else
                    {
                        var title = navigationPanelEvent.ViewModel.Caption;

                        var dockPosition = navigationPanelEvent.RegionName.ToDockPosition();
                        logger.Trace("Create new Task Pane with title {0}", title);
                        var taskPane = this.TaskPaneFactory.CreateCTP("CDP4AddinCE.TaskPaneWpfHostControl", title);
                        taskPane.DockPosition = dockPosition;
                        taskPane.Width = 300;
                        taskPane.Visible = true;
                        var wpfHostControl = taskPane.ContentControl as TaskPaneWpfHostControl;
                        if (wpfHostControl != null)
                        {
                            wpfHostControl.SetContent(uiElement);
                        }

                        identifiableCustomTaskPane = new IdentifiableCustomTaskPane(identifier, taskPane);

                        this.customTaskPanes.Add(identifier, identifiableCustomTaskPane);
                    }
                }
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : string.Empty;

                logger.Fatal(ex, string.Format("handle open panel failed: {0} - {1}", ex.Message, innerExceptionMessage));
            }
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

                IdentifiableCustomTaskPane identifiableCustomTaskPane = null;
                var taskPaneExists = this.customTaskPanes.TryGetValue(identifier, out identifiableCustomTaskPane);
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
                if (this.RibbonUi != null)
                {
                    this.RibbonUi.Invalidate();
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
            Factory.Console.WriteLine("AddinOnConnection");
            logger.Trace("AddinOnConnection");

            try
            {
                logger.Trace("new up bootstrapper");
                var bootstrapper = new CDP4AddinBootstrapper();
                logger.Trace("run bootstrapper");
                bootstrapper.Run(true);

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
                logger.Fatal(compositionException, string.Format("CompositionException: {0}", compositionException.Message));

                foreach (var error in compositionException.Errors)
                {
                    logger.Fatal(error.Exception, string.Format("CompositionException Error: {0} {1}", error.Element.DisplayName, error.Description));
                }                
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Bootstrapper exception: ");
                Utils.Dialog.ShowError(ex, "Unexpected state in CDP4-CE.Addin");
            }
        }

        /// <summary>
        /// Initialize the Office application objects, setup event handlers
        /// </summary>
        /// <param name="application">
        /// The application object that is used in the current addin
        /// </param>
        private void InitializeApplication(object application)
        {
            var comObject = Core.Default.CreateObjectFromComProxy(null, application, false);

            var excel = comObject as NetOffice.ExcelApi.Application;
            if (excel != null)
            {
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
                return;
            }

            var word = comObject as NetOffice.WordApi.Application;
            if (word != null)
            {
                // set the word application object
                this.wordApplication = word;
                this.wordApplication.DisplayAlerts = WdAlertLevel.wdAlertsAll;
                this.wordApplication.ScreenUpdating = true;

                // set the word instance to the office application wrapper
                this.officeApplicationWrapper.Word = word;

                //// create event handlers

                logger.Debug("The current addin is loaded for Word");
                return;
            }
        }

        /// <summary>
        /// Destruct the Office application objects and remove event handlers
        /// </summary>
        private void DestructApplication()
        {
            if (this.excelApplication != null)
            {
                // unregister events
                this.excelApplication.WorkbookActivateEvent -= this.OnWorkbookActivateEvent;
                this.excelApplication.WorkbookDeactivateEvent -= this.OnWorkbookDeactivateEvent;
                
                // set excel instance to null
                this.officeApplicationWrapper.Excel = null;
                this.excelApplication = null;
            }

            if (this.wordApplication != null)
            {
                //// unregister events

                // set excel instance to null
                this.officeApplicationWrapper.Word = null;
                this.wordApplication = null;
            }
        }

        /// <summary>
        /// the event-handler for the <see cref="Excel.Application.WorkbookActivateEvent"/>
        /// </summary>
        /// <param name="workbook">
        /// The workbook that was activated
        /// </param>
        private void OnWorkbookActivateEvent(Excel.Workbook workbook)
        {
            logger.Debug("Workbook {0} activated", workbook.Name);
            if (this.RibbonUi != null)
            {
                this.RibbonUi.Invalidate();
            }
        }

        /// <summary>
        /// the event-handler for the <see cref="Excel.Application.WorkbookDeactivateEvent"/>
        /// </summary>
        /// <param name="workbook">
        /// The workbook that was deactivated
        /// </param>
        private void OnWorkbookDeactivateEvent(Excel.Workbook workbook)
        {
            logger.Debug("Workbook {0} deactivated", workbook.Name);
            if (this.RibbonUi != null)
            {
                this.RibbonUi.Invalidate();
            }            
        }

        /// <summary>
        /// Initializes the MEF instantiated services and managers        
        /// </summary>
        private void InitializeMefImports()
        {
            // IFluentRibbonManager
            var panelNavigationService = ServiceLocator.Current.GetInstance<IPanelNavigationService>();
            this.FluentRibbonManager = ServiceLocator.Current.GetInstance<IFluentRibbonManager>();
            var thingDialogNavigationService = ServiceLocator.Current.GetInstance<IThingDialogNavigationService>();
            var dialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();

            this.FluentRibbonManager.IsActive = true;
            var ribbonpart = new AddinRibbonPart(0, panelNavigationService, thingDialogNavigationService, dialogNavigationService);
            this.FluentRibbonManager.RegisterRibbonPart(ribbonpart);
            this.fluentRibbonXml = this.FluentRibbonManager.GetFluentXml();

            // IOfficeApplicationWrapper
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
            Factory.Console.WriteLine("AddinOnAddInsUpdate");
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
            Factory.Console.WriteLine("AddinOnStartupComplete");
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
            Factory.Console.WriteLine("AddinOnBeginShutdown");
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

            Factory.Console.WriteLine("AddinOnDisconnection");
            logger.Trace("AddinOnDisconnection");
        }
    }
}
