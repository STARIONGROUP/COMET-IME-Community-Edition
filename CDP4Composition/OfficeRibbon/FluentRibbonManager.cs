// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FluentRibbonManager.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Schema;
    using NLog;

    /// <summary>
    /// The <see cref="FluentRibbonManager"/> registers the Fluent Ribbon XML declared in the CDP4 plugins that is used to customize the
    /// Office Ribbon.
    /// </summary>
    [Export(typeof(IFluentRibbonManager))]
    public class FluentRibbonManager : IFluentRibbonManager
    {
        /// <summary>
        /// The Nlog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The list of Ribbon Parts that have been registered with the <see cref="FluentRibbonManager"/>
        /// </summary>
        private readonly List<RibbonPart> ribbonParts = new List<RibbonPart>();

        /// <summary>
        /// The office ribbon <code>customui14</code> schema
        /// </summary>
        private XmlSchema customui14Schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentRibbonManager"/> class.
        /// </summary>
        public FluentRibbonManager()
        {
            this.customui14Schema = this.GetRibbonXmlSchema();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IFluentRibbonManager"/> is active or not
        /// </summary>
        /// <remarks>
        /// The <see cref="RibbonPart"/>s shall only be active if the orchestrating <see cref="IFluentRibbonManager"/> is active.
        /// </remarks>
        public bool IsActive { get; set; }

        /// <summary>
        /// Registers the <see cref="RibbonPart"/> that contains fluent Ribbon XML with the <see cref="FluentRibbonManager"/>
        /// </summary>
        /// <param name="ribbonPart">
        /// The <see cref="RibbonPart"/> that is to be registered
        /// </param>
        public void RegisterRibbonPart(RibbonPart ribbonPart)
        {
            ribbonPart.FluentRibbonManager = this;
            this.ribbonParts.Add(ribbonPart);
        }

        /// <summary>
        /// Gets the Fluent Ribbon XML that is used to customize the Office Ribbon
        /// </summary>
        /// <returns>
        /// a string of valid Fluent Ribbon XML
        /// </returns>
        public string GetFluentXml()
        {
            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.Append("<customUI xmlns=\"http://schemas.microsoft.com/office/2006/01/customui\" onLoad=\"CustomUI_OnLoad\" >");
            sb.Append("<ribbon>");
            sb.Append("<tabs>");
            sb.Append("<tab id=\"COMET\" label=\"COMET\">");

            foreach (var ribbonPart in this.ribbonParts.OrderBy(x => x.Order))
            {
                try
                {
                    this.ValidateRibbonXml(ribbonPart.RibbonXml);
                    sb.Append(ribbonPart.RibbonXml);
                }
                catch (Exception ex)
                {
                    logger.Warn("RibbonPart XML ignored: {0} {1}", ribbonPart.RibbonXml, ex.Message);               
                }
            }

            sb.Append("</tab>");
            sb.Append("</tabs>");
            sb.Append("</ribbon>");
            sb.Append("</customUI>");

            var ribbonXml = sb.ToString();

            try
            {
                this.ValidateRibbonXml(ribbonXml, false);
            }
            catch (Exception ex)
            {
                logger.Warn("RibbonPart XML ignored: {0} {1}", ex.Message);
            }

            return ribbonXml;
        }

        /// <summary>
        /// Validates the XML of the provided <see cref="RibbonPart"/>.
        /// </summary>
        /// <param name="ribbonXml">
        /// The ribbon Xml that is to be validated
        /// </param>
        /// <param name="fragment">
        /// a value indicating whether validation happens on an XML fragment or a complete ribbon XML document.
        /// </param>
        private void  ValidateRibbonXml(string ribbonXml, bool fragment = true)
        {
            var settings = new XmlReaderSettings();
            if (fragment)
            {
                settings.ConformanceLevel = ConformanceLevel.Fragment;
            }
            
            settings.Schemas.Add(this.customui14Schema);
            settings.ValidationType = ValidationType.Schema;

            using (var stringReader = new StringReader(ribbonXml))
            {
                using (var reader = XmlReader.Create(stringReader, settings))
                {
                    while (reader.Read())
                    {
                        logger.Trace("reading ribbon XML");
                    }
                }
            }
        }

        /// <summary>
        /// Gets the ribbon xml schema from the embedded resources
        /// </summary>
        /// <returns>
        /// an instance of <see cref="XmlSchema"/>
        /// </returns>
        private XmlSchema GetRibbonXmlSchema()
        {
            var a = Assembly.GetExecutingAssembly();
            var type = this.GetType();
            var @namespace = type.Namespace;
            var ribbonXmlResource = string.Format("{0}.Resources.RibbonXml.{1}", @namespace, "customui14.xsd");

            var stream = a.GetManifestResourceStream(ribbonXmlResource);

            return XmlSchema.Read(stream,
                (sender, args) =>
                    {
                        logger.Debug("{0} - {1}", args.Severity, args.Message);
                    }
                );
        }


        /// <summary>
        /// Executes the action associated to the control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        public async Task OnAction(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                await ribbonPart.OnAction(ribbonControlId, ribbonControlTag);
            }
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
        public string GetDescription(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                var description = ribbonPart.GetDescription(ribbonControlId, ribbonControlTag);
                if (string.IsNullOrEmpty(description))
                {
                    logger.Debug("The Description property of RibbonPart {0}:{1} is null", ribbonControlTag, ribbonControlId);
                    return string.Empty;
                }

                if (description.Length > 4096)
                {
                    logger.Debug("The Description property of RibbonPart {0}:{1} is longer than 4096 characters: {2}", ribbonControlTag, ribbonControlId, description);
                    return description.Substring(0, 4096);
                }

                return description;
            }

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
        public string GetKeytip(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                var keyptip = ribbonPart.GetKeytip(ribbonControlId, ribbonControlTag);
                if (string.IsNullOrEmpty(keyptip))
                {
                    logger.Debug("The KeyTip property of RibbonPart {0}:{1} is null", ribbonControlTag, ribbonControlId);
                    return string.Empty;
                }

                if (keyptip.Length > 3)
                {
                    logger.Debug("The KeyTip property of RibbonPart {0}:{1} is longer than 3 characters: {2}", ribbonControlTag, ribbonControlId, keyptip);
                    return keyptip.Substring(0, 3);
                }
                
                return keyptip;                                    
            }

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
        public string GetScreentip(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                var screentip = ribbonPart.GetScreentip(ribbonControlId, ribbonControlTag);
                if (string.IsNullOrEmpty(screentip))
                {
                    logger.Debug("The Screentip property of RibbonPart {0}:{1} is null", ribbonControlTag, ribbonControlId);
                    return string.Empty;
                }

                if (screentip.Length > 1024)
                {
                    logger.Debug("The Screentip property of RibbonPart {0}:{1} is longer than 1024 characters: {2}", ribbonControlTag, ribbonControlId, screentip);
                    return screentip.Substring(0, 1024);
                }

                return screentip;
            }

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
        public string GetContent(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                var content = ribbonPart.GetContent(ribbonControlId, ribbonControlTag);
                if (string.IsNullOrEmpty(content))
                {
                    logger.Debug("The Content property of RibbonPart {0}:{1} is null", ribbonControlTag, ribbonControlId);
                    return string.Empty;
                }

                if (this.IsWelformedXml(content))
                {
                    return content;
                }

                return string.Empty;
            }

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
        /// an instance of <see cref="Image"/> that will be used for the association Ribbon Control
        /// </returns>
        /// <remarks>
        /// minimum length of 1 character, maximum length of 1024 characters
        /// </remarks>
        public string GetLabel(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                var label = ribbonPart.GetLabel(ribbonControlId, ribbonControlTag);
                if (string.IsNullOrEmpty(label))
                {
                    logger.Debug("The Label property of RibbonPart {0}:{1} is null", ribbonControlTag, ribbonControlId);
                    return string.Empty;
                }

                if (label.Length > 1024)
                {
                    logger.Debug("The Label property of RibbonPart {0}:{1} is longer than 1024 characters: {2}", ribbonControlTag, ribbonControlId, label);
                    return label.Substring(0, 1024);
                }

                return label;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the tooltip size for the control
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
        public string GetSize(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                var size = ribbonPart.GetSize(ribbonControlId, ribbonControlTag);

                if ((size == "normal") || (size == "large"))
                {
                    return size;
                }

                return "normal";
            }

            return "normal";
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
        public string GetSupertip(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                var supertip = ribbonPart.GetSupertip(ribbonControlId, ribbonControlTag);
                if (string.IsNullOrEmpty(supertip))
                {
                    logger.Debug("The Supertip property of RibbonPart {0}:{1} is null", ribbonControlTag, ribbonControlId);
                    return string.Empty;
                }

                if (supertip.Length > 1024)
                {
                    logger.Debug("The ScrSupertipeentip property of RibbonPart {0}:{1} is longer than 1024 characters: {2}", ribbonControlTag, ribbonControlId, supertip);
                    return supertip.Substring(0, 1024);
                }

                return supertip;
            }

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
        public Image GetImage(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetImage(ribbonControlId, ribbonControlTag);
            }

            return null;
        }

        /// <summary>
        /// Gets a value indicating whether a control shall be enabled or disabled
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
        public bool GetEnabled(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetEnabled(ribbonControlId, ribbonControlTag);         
            }

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
        public bool GetPressed(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetPressed(ribbonControlId, ribbonControlTag);
            }

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
        public bool GetVisible(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetVisible(ribbonControlId, ribbonControlTag);
            }

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
        public bool GetShowImage(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetShowImage(ribbonControlId, ribbonControlTag);
            }

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
        public bool GetShowLabel(string ribbonControlId, string ribbonControlTag = "")
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetShowLabel(ribbonControlId, ribbonControlTag);
            }

            return true;
        }

        /// <summary>
        /// Gets the RibbonPart that contains the Ribbon XML control with the specified ribbonControlId.
        /// </summary>
        /// <param name="ribbonControlId">
        /// The unique Id of the Ribbon XML control that needs to be retrieved.
        /// </param>
        /// <returns>
        /// The <see cref="RibbonPart"/> that contains a Ribbon XML control with the associated Id.
        /// </returns>
        /// <remarks>
        /// If none of the <see cref="RibbonPart"/>s contain such a XML Ribbon control, then null is returned.
        /// </remarks>>
        private RibbonPart GetRibbonPartBase(string ribbonControlId)
        {
            return this.ribbonParts.FirstOrDefault(ribbonPartBase => ribbonPartBase.ControlIdentifiers.Contains(ribbonControlId));
        }

        /// <summary>
        /// Checks whether an XML string is a well formed XML string
        /// </summary>
        /// <param name="xml">
        /// The XML string that needs to be checked
        /// </param>
        /// <returns>
        /// true if well formed, false otherwise
        /// </returns>
        private bool IsWelformedXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return false;
            }

            using (var xr = XmlReader.Create(new StringReader(xml)))
            {
                try
                {
                    while (xr.Read())
                    {
                        logger.Trace("xml is well formed");
                    }
                }
                catch (Exception ex)
                {
                    logger.Debug("xml is not wel l formed", ex);
                    return false;
                }
            }

            return true;
        }
    }
}