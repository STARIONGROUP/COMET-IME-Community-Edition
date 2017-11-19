// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RibbonPart.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.OfficeRibbon;
    using CDP4Dal.Permission;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="RibbonPart"/> class is to describe and provide a part of the Fluent Ribbon
    /// that is used in an Office addin. A <see cref="RibbonPart"/> always describes a ribbon group containing different controls
    /// </summary>
    public abstract class RibbonPart : IFluentRibbonCallback
    {
        /// <summary>
        /// A <see cref="Logger"/> instance
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// the list of unique identifiers of the controls declared in the Ribbon XML that are managed by the current <see cref="RibbonPart"/>
        /// </summary>
        private readonly List<string> controlIdentifiers = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonPart"/> class.
        /// </summary>
        /// <param name="order">
        /// The order in which the ribbon part is to be presented on the Office Ribbon
        /// </param>
        /// <param name="panelNavigationService">
        /// An instance of <see cref="IPanelNavigationService"/> that orchestrates navigation of <see cref="IPanelView"/>
        /// </param>
        /// <param name="thingDialogNavigationService">The instance of <see cref="IThingDialogNavigationService"/> used to navigate to dialogs</param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        protected RibbonPart(int order, IPanelNavigationService panelNavigationService, IThingDialogNavigationService thingDialogNavigationService, IDialogNavigationService dialogNavigationService)
        {
            var ribbonXmlResource = this.GetRibbonXmlResourceName();

            this.RibbonXml = this.GetRibbonXmlFromResource(ribbonXmlResource);
            this.Order = order;

            this.PanelNavigationService = panelNavigationService;
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.DialogNavigationService = dialogNavigationService;
        }

        /// <summary>
        /// Updates the <see cref="ControlIdentifiers"/> list
        /// </summary>
        /// <param name="xml">
        /// The XML string that contains the ribbon xml for the current ribbon part
        /// </param>
        protected virtual void UpdateControlIdentifiers(string xml)
        {
            var doc = XDocument.Parse(xml);
            var ribbonxmlnode = doc.Root;
            var ids = ribbonxmlnode.Descendants().Attributes("id").Select(x => x.Value).ToList();
            this.controlIdentifiers.AddRange(ids);
        }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> used to navigate to Panels
        /// </summary>
        public IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IPermissionService"/>
        /// </summary>
        public IPermissionService PermissionService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> used to navigate to dialogs
        /// </summary>
        public IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> used to navigate to dialogs
        /// </summary>
        public IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the order in which the ribbon xml is to be ordered in the Office Fluent Ribbon 
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IFluentRibbonManager"/> instance responsible for orchestrating the <see cref="RibbonPart"/>s
        /// </summary>
        public IFluentRibbonManager FluentRibbonManager { get; set; }

        /// <summary>
        /// Gets the xml describing the part of the Office Fluent Ribbon
        /// </summary>
        public string RibbonXml { get; private set; }

        /// <summary>
        /// Gets the list of unique id's of the controls present on the current Ribbon
        /// </summary>
        public List<string> ControlIdentifiers
        {
            get
            {
                return this.controlIdentifiers;
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
        public virtual Task OnAction(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The OnAction method of the {0} is not overriden, therefore the OnAction method only shows this log message", this.GetType());
            return null;
        }

        /// <summary>
        /// Gets the description of a control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// a string that represents the content of the description
        /// </returns>
        /// <remarks>
        /// minimum length of 1 character, maximum length of 4096 characters
        /// </remarks>
        public virtual string GetDescription(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetDescription method of the {0} is not overriden", this.GetType());
            return string.Empty;
        }

        /// <summary>
        /// Gets the key-tip of a control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// a string that represents the content of the key-tip
        /// </returns>
        /// <remarks>
        /// the minimum length = 1 character, the maximum length = 3 characters, 
        /// </remarks>
        public virtual string GetKeytip(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetKeytip method of the {0} is not overriden", this.GetType());
            return string.Empty;
        }

        /// <summary>
        /// Gets the Screen-tip of a control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// a string that represents the content of the Screen-tip
        /// </returns>
        /// <remarks>
        /// minimum length of 1 character, maximum length of 1024 characters
        /// </remarks>
        public virtual string GetScreentip(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetScreentip method of the {0} is not overriden", this.GetType());
            return string.Empty;
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
        public virtual string GetContent(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetContent method of the {0} is not overriden", this.GetType());
            return string.Empty;
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
        /// a string that represents the content of the label
        /// </returns>
        /// <remarks>
        /// minimum length of 1 character, maximum length of 1024 characters
        /// </remarks>
        public virtual string GetLabel(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetLabel method of the {0} is not overriden", this.GetType());
            return string.Empty;
        }

        /// <summary>
        /// Gets the size for the control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// a string that represents size of the control, possible values are normal and large
        /// </returns>
        public virtual string GetSize(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetSize method of the {0} is not overriden", this.GetType());
            return "normal";
        }

        /// <summary>
        /// Gets the tooltip as a <see cref="string"/> for the control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// a string that represents the content of the tooltip
        /// </returns>
        public virtual string GetToolTip(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetToolTip method of the {0} is not overriden", this.GetType());
            return string.Empty;
        }

        /// <summary>
        /// Gets the super-tip as a <see cref="string"/> for the control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// a string that represents the content of the super-tip
        /// </returns>
        /// <remarks>
        /// minimum length of 1 character, maximum length of 1024 characters
        /// </remarks>
        public virtual string GetSupertip(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetSupertip method of the {0} is not overriden", this.GetType());
            return string.Empty;
        }

        /// <summary>
        /// Gets the <see cref="Image"/> to decorate the control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// an instance of <see cref="Image"/> that will be used for the association Ribbon Control
        /// </returns>
        public virtual Image GetImage(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetImage method of the {0} is not overriden", this.GetType());
            return null;
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
        public virtual bool GetEnabled(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetEnabled method of the {0} is not overriden", this.GetType());
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether a control is pressed or not
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// true if pressed, false if not pressed
        /// </returns>
        public virtual bool GetPressed(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetPressed method of the {0} is not overriden", this.GetType());
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether a control is visible or not
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// true if visible, false is not
        /// </returns>
        public virtual bool GetVisible(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetVisible method of the {0} is not overriden", this.GetType());
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether the image of a control is visible or not
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// true if visible, false is not
        /// </returns>
        public virtual bool GetShowImage(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetShowImage method of the {0} is not overriden", this.GetType());
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether the label of a control is visible or not
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// true if visible, false is not
        /// </returns>
        public virtual bool GetShowLabel(string ribbonControlId, string ribbonControlTag = "")
        {
            logger.Debug("The GetShowLabel method of the {0} is not overriden", this.GetType());
            return true;
        }

        /// <summary>
        /// Gets the the current executing assembly
        /// </summary>
        /// <returns>
        /// an instance of <see cref="Assembly"/>
        /// </returns>
        protected abstract Assembly GetCurrentAssembly();

        /// <summary>
        /// Gets the name of the resource that contains the Ribbon XML
        /// </summary>
        /// <returns>
        /// The name of the ribbon XML resource
        /// </returns>
        protected abstract string GetRibbonXmlResourceName();

        /// <summary>
        /// Gets the ribbon resource
        /// </summary>
        /// <param name="ribbonXmlResource">
        /// The name of the resource that contains the ribbon xml
        /// </param>
        /// <returns>
        /// a string containing the ribbon xml
        /// </returns>
        private string GetRibbonXmlFromResource(string ribbonXmlResource)
        {
            var type = this.GetType();
            var @namespace = type.Namespace;
            var resourceName = string.Format("{0}.Resources.RibbonXml.{1}.xml", @namespace, ribbonXmlResource);

            var assembly = this.GetCurrentAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    logger.Debug("The {0} could not be found", resourceName);
                    return string.Empty;
                }
                
                using (var reader = new StreamReader(stream))
                {
                    var xmlstring = reader.ReadToEnd();
                    this.UpdateControlIdentifiers(xmlstring);

                    try
                    {
                        var sb = new StringBuilder();

                        var doc = XDocument.Parse(xmlstring);
                        var ribbonxmlnode = doc.Root;
                        if (ribbonxmlnode != null)
                        {
                            foreach (var node in ribbonxmlnode.Nodes())
                            {
                                sb.Append(node);
                            }

                            xmlstring = sb.ToString();
                        }

                        logger.Trace("Ribbon Part XML: {0}", xmlstring);
                        return xmlstring;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        return string.Empty;    
                    }
                }
            }
        }
    }
}
