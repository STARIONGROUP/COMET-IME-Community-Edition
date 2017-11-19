// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextScriptPanel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for TextScriptPanel.xaml
    /// </summary>
    [PanelViewExport(RegionNames.EditorPanel)]
    public class TextScriptPanel : ScriptPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextScriptPanel"/> class
        /// </summary>
        public TextScriptPanel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextScriptPanel"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public TextScriptPanel(bool initializeComponent) : base(initializeComponent)
        {
        }
    }
}