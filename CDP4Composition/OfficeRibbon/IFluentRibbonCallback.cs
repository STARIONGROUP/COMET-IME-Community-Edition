// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IFluentRibbonCallback.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.OfficeRibbon
{
    using System.Drawing;
    using System.Threading.Tasks;

    /// <summary>
    /// Definition of the methods that can be invoked through the fluent ribbon xml callback mechanism
    /// </summary>
    public interface IFluentRibbonCallback
    {
        /// <summary>
        /// Executes the action associated to the control
        /// </summary>
        /// <param name="ribbonControlId">
        /// The Id property of the associated RibbonControl
        /// </param>
        /// <param name="ribbonControlTag">
        /// The Tag property of the associated RibbonControl
        /// </param>        
        Task OnAction(string ribbonControlId, string ribbonControlTag = "");

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
        string GetDescription(string ribbonControlId, string ribbonControlTag = "");

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
        string GetKeytip(string ribbonControlId, string ribbonControlTag = "");

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
        string GetScreentip(string ribbonControlId, string ribbonControlTag = "");

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
        string GetContent(string ribbonControlId, string ribbonControlTag = "");

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
        string GetLabel(string ribbonControlId, string ribbonControlTag = "");

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
        string GetSize(string ribbonControlId, string ribbonControlTag = "");

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
        string GetSupertip(string ribbonControlId, string ribbonControlTag = "");

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
        Image GetImage(string ribbonControlId, string ribbonControlTag = "");

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
        bool GetEnabled(string ribbonControlId, string ribbonControlTag = "");

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
        bool GetPressed(string ribbonControlId, string ribbonControlTag = "");

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
        bool GetVisible(string ribbonControlId, string ribbonControlTag = "");

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
        bool GetShowImage(string ribbonControlId, string ribbonControlTag = "");

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
        bool GetShowLabel(string ribbonControlId, string ribbonControlTag = "");
    }
}
