// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CrossViewEditorRibbonPart.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CrossViewEditor
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4OfficeInfrastructure;
    using NLog;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="CrossViewEditorRibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public class CrossViewEditorRibbonPart : RibbonPart
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="IOfficeApplicationWrapper"/> that provides access to the loaded Office application
        /// </summary>
        private readonly IOfficeApplicationWrapper officeApplicationWrapper;

        /// <summary>
        /// Gets the <see cref="ISession"/> that is active for the <see cref="RibbonPart"/>
        /// </summary>
        public ISession Session { get; private set; }

        /// <summary>
        /// Gets a List of <see cref="Iteration"/> that are opened
        /// </summary>
        public List<Iteration> Iterations { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossViewEditorRibbonPart"/> class.
        /// </summary>
        /// <param name="order">
        /// The order in which the ribbon part is to be presented on the Office Ribbon
        /// </param>
        /// <param name="panelNavigationService">
        /// The instance of <see cref="IPanelNavigationService"/> that orchestrates navigation of <see cref="IPanelView"/>
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The instance of <see cref="IThingDialogNavigationService"/> that orchestrates navigation of <see cref="IThingDialogView"/>
        /// </param>
        /// <param name="dialogNavigationService">
        /// The instance of <see cref="IDialogNavigationService"/> that orchestrates navigation to dialogs
        /// </param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        /// <param name="officeApplicationWrapper">
        /// The instance of <see cref="IOfficeApplicationWrapper"/> that provides access to the loaded Office application.
        /// </param>
        public CrossViewEditorRibbonPart(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService, IOfficeApplicationWrapper officeApplicationWrapper)
            : base(order, panelNavigationService, thingDialogNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.officeApplicationWrapper = officeApplicationWrapper;
            this.Iterations = new List<Iteration>();

            CDPMessageBus.Current.Listen<SessionEvent>().Subscribe(this.SessionChangeEventHandler);

            CDPMessageBus.Current.Listen<ObjectChangedEvent>(typeof(Iteration))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(this.IterationChangeEventHandler);
        }

        /// <summary>
        /// Gets the the current executing assembly
        /// </summary>
        /// <returns>
        /// an instance of <see cref="Assembly"/>
        /// </returns>
        protected override Assembly GetCurrentAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        /// <summary>
        /// Gets the name of the resource that contains the Ribbon XML
        /// </summary>
        /// <returns>
        /// The name of the ribbon XML resource
        /// </returns>
        protected override string GetRibbonXmlResourceName()
        {
            return "CrossViewEditorRibbon";
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Session"/> that is being represented by the view-model
        /// </summary>
        /// <param name="sessionChange">
        /// The payload of the event that is being handled
        /// </param>
        private void SessionChangeEventHandler(SessionEvent sessionChange)
        {
            if (this.FluentRibbonManager == null)
            {
                return;
            }

            if (!this.FluentRibbonManager.IsActive)
            {
                return;
            }

            switch (sessionChange.Status)
            {
                case SessionStatus.Open:
                    this.Session = sessionChange.Session;
                    return;

                case SessionStatus.Closed:
                    this.Iterations.Clear();
                    this.Session = null;
                    break;
            }
        }

        /// <summary>
        /// The <see cref="ObjectChangedEvent"/> handler for <see cref="Iteration"/>
        /// </summary>
        /// <param name="iterationEvent">The <see cref="ObjectChangedEvent"/></param>
        private void IterationChangeEventHandler(ObjectChangedEvent iterationEvent)
        {
            if (this.Session == null)
            {
                return;
            }

            switch (iterationEvent.EventKind)
            {
                case EventKind.Added:
                    this.Iterations.Add(iterationEvent.ChangedThing as Iteration);
                    break;

                case EventKind.Removed:
                {
                    var iteration = iterationEvent.ChangedThing as Iteration;
                    this.Iterations.RemoveAll(x => x == iteration);
                    break;
                }
            }
        }

        /// <summary>
        /// Invokes the action as a result of a ribbon control being clicked, selected, etc.
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        public override async Task OnAction(string ribbonControlId, string ribbonControlTag = "")
        {
            if (this.Session == null)
            {
                logger.Error("The current session is null");
                return;
            }

            if (ribbonControlId.StartsWith("Editor_"))
            {
                await Task.Delay(1000);
            }
            else
            {
                logger.Debug($"The ribbon control with Id {ribbonControlId} and Tag {ribbonControlTag} is not handled by the current RibbonPart");
            }
        }

        /// <summary>
        /// Gets the the content of a Dynamic Menu
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// Ribbon XML that is the content of the Dynamic Menu
        /// </returns>
        public override string GetContent(string ribbonControlId, string ribbonControlTag = "")
        {
            var menuxml = string.Empty;

            if (ribbonControlId == "Editor")
            {
                var sb = new StringBuilder();
                sb.Append(@"<menu xmlns=""http://schemas.microsoft.com/office/2006/01/customui"">");

                foreach (var iteration in this.Iterations)
                {
                    var engineeringModel = (EngineeringModel) iteration.Container;

                    var selectedDomainOfExpertise = this.Session.QuerySelectedDomainOfExpertise(iteration);

                    var domainShortName = selectedDomainOfExpertise == null ? string.Empty : selectedDomainOfExpertise.ShortName;
                    var label = $"{engineeringModel.EngineeringModelSetup.ShortName} - {iteration.IterationSetup.IterationNumber} : [{domainShortName}]";

                    var menuContent = string.Format("<button id=\"Editor_{0}\" label=\"{1}\" onAction=\"OnAction\" tag=\"{0}\" />", iteration.Iid, label);
                    sb.Append(menuContent);
                }

                sb.Append(@"</menu>");
                menuxml = sb.ToString();
            }

            this.UpdateControlIdentifiers(menuxml);

            return menuxml;
        }

        /// <summary>
        /// Gets a value indicating whether a control is enabled or disabled
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// true if enabled, false if not enabled
        /// </returns>
        public override bool GetEnabled(string ribbonControlId, string ribbonControlTag = "")
        {
            return ribbonControlId == "Editor" && this.IsEditorEnabled();
        }

        /// <summary>
        /// Gets the label as a <see cref="string"/> for the control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// A string that represents the content of the label(min length of 1 character, max length of 1024 chars)
        /// </returns>
        public override string GetLabel(string ribbonControlId, string ribbonControlTag = "")
        {
            return ribbonControlId == "Editor" ? "Editor" : string.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether the Editor button on the Ribbon is active or not
        /// </summary>
        /// <returns>
        /// returns true if the Rebuild is enabled, false otherwise
        /// </returns>
        private bool IsEditorEnabled()
        {
            return this.Iterations.Count != 0 && this.Session != null;
        }
    }
}
