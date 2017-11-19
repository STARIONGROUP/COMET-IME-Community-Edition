// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LuaScriptPanel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for LuaScriptPanel.xaml
    /// </summary>
    [PanelViewExport(RegionNames.EditorPanel)]
    public class LuaScriptPanel : ScriptPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LuaScriptPanel"/> class
        /// </summary>
        public LuaScriptPanel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LuaScriptPanel"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public LuaScriptPanel(bool initializeComponent) : base(initializeComponent)
        {
        }
    }
}