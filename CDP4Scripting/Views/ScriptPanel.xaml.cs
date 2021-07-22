// -------------------------------------------------------------------------------------------------
// <copyright file="ScriptPanel.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for ScriptPanel.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class ScriptPanel : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptPanel"/> class
        /// </summary>
        public ScriptPanel()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptPanel"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ScriptPanel(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
