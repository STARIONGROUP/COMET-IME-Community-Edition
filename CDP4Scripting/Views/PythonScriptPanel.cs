// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PythonScriptPanel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for PythonScriptPanel.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public class PythonScriptPanel : ScriptPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PythonScriptPanel"/> class
        /// </summary>
        public PythonScriptPanel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonScriptPanel"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public PythonScriptPanel(bool initializeComponent) : base(initializeComponent)
        {
        }
    }
}