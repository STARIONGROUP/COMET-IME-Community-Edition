// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptPanelEvent.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Events
{
    using Interfaces;

    /// <summary>
    /// The script panel status.
    /// </summary>
    public enum ScriptPanelStatus
    {
        /// <summary>
        /// Saved status, a script has been saved.
        /// </summary>
        Saved
    }

    /// <summary>
    /// The script panel event.
    /// </summary>
    public class ScriptPanelEvent
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptPanelEvent"/> class.
        /// </summary>
        /// <param name="scriptPanelViewModel">The script panel view model.</param>
        /// <param name="status">The status.</param>
        public ScriptPanelEvent(IScriptPanelViewModel scriptPanelViewModel, ScriptPanelStatus status)
        {
            this.ScriptPanelViewModel = scriptPanelViewModel;
            this.Status = status;
        }

        /// <summary>
        /// Gets or sets the ScriptPanelViewModel.
        /// </summary>
        public IScriptPanelViewModel ScriptPanelViewModel { get; set; }

        /// <summary>
        /// Gets or sets the status of the <see cref="ScriptPanelViewModel"/>.
        /// </summary>
        public ScriptPanelStatus Status { get; set; }
    }
}