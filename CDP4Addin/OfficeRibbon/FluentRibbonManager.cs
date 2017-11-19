// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FluentRibbonManager.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Addin
{
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using CDP4Composition;

    /// <summary>
    /// The <see cref="FluentRibbonManager"/> registers the Fluent Ribbon XML declared in the CDP4 plugins that is used to customize the
    /// Office Ribbon.
    /// </summary>
    [Export(typeof(IFluentRibbonManager))]
    public sealed class FluentRibbonManager : IFluentRibbonManager
    {
        /// <summary>
        /// The list of Ribbon Parts that have been registered with the <see cref="FluentRibbonManager"/>
        /// </summary>
        private readonly List<RibbonPart> ribbonParts = new List<RibbonPart>();

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
            sb.Append("<customUI xmlns=\"http://schemas.microsoft.com/office/2006/01/customui\" onLoad=\"OnLoadRibonUI\" >");
            sb.Append("<ribbon>");
            sb.Append("<tabs>");
            sb.Append("<tab id=\"CDP4\" label=\"CDP4\">");

            foreach (var ribbonPart in this.ribbonParts.OrderBy(x => x.Order))
            {
                sb.Append(ribbonPart.RibbonXml);
            }

            sb.Append("</tab>");
            sb.Append("</tabs>");
            sb.Append("</ribbon>");
            sb.Append("</customUI>");
            return sb.ToString();
        }

        /// <summary>
        /// Executes the action associated to the control
        /// </summary>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        public void OnAction(string ribbonControlTag, string ribbonControlId)
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                ribbonPart.OnAction(ribbonControlTag, ribbonControlId);
            }
        }

        /// <summary>
        /// Gets the <see cref="Image"/> to decorate the control
        /// </summary>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// an instance of <see cref="Image"/> that will be used for the association Ribbon Control
        /// </returns>
        public Image GetImage(string ribbonControlTag, string ribbonControlId)
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetImage(ribbonControlTag, ribbonControlId);
            }
            
            return null;            
        }

        /// <summary>
        /// Gets the label as a <see cref="string"/> for the control
        /// </summary>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// an instance of <see cref="Image"/> that will be used for the association Ribbon Control
        /// </returns>
        public string GetLabel(string ribbonControlTag, string ribbonControlId)
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetLabel(ribbonControlTag, ribbonControlId);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether a control shall be enabled or disabled
        /// </summary>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// true if enabled, false if not enabled
        /// </returns>
        public bool GetEnabled(string ribbonControlTag, string ribbonControlId)
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetEnabled(ribbonControlTag, ribbonControlId);
            }

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether a control is pressed or not
        /// </summary>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// true if pressed, false if not pressed
        /// </returns>
        public bool GetPressed(string ribbonControlTag, string ribbonControlId)
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetPressed(ribbonControlTag, ribbonControlId);
            }

            return false;
        }

        /// <summary>
        /// Gets the tooltip as a <see cref="string"/> for the control
        /// </summary>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// an instance of <see cref="Image"/> that will be used for the association Ribbon Control
        /// </returns>
        public string GetToolTip(string ribbonControlTag, string ribbonControlId)
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetToolTip(ribbonControlTag, ribbonControlId);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether a control is visible or not
        /// </summary>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <returns>
        /// true if visible, false is not
        /// </returns>
        public bool GetVisible(string ribbonControlTag, string ribbonControlId)
        {
            var ribbonPart = this.GetRibbonPartBase(ribbonControlId);
            if (ribbonPart != null)
            {
                return ribbonPart.GetVisible(ribbonControlTag, ribbonControlId);
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
            return this.ribbonParts.FirstOrDefault(ribbonPartBase => ribbonPartBase.ControlItentifiers.Contains(ribbonControlId));
        }
    }
}
