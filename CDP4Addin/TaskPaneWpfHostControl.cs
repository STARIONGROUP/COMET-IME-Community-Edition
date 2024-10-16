// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskPaneWpfHostControl.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4AddinCE
{
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;

    /// <summary>
    /// The purpose of the <see cref="TaskPaneWpfHostControl"/> is to act as container or host for
    /// the WPF controls of the CDP4 plugins
    /// </summary>
    [Guid("029DF98D-39FB-4DD7-A562-E2D49261E97B")]
    [ProgId("CDP4-COMETCE.TaskPaneWpfHostControl")]
    [ComVisible(true)]
    public partial class TaskPaneWpfHostControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskPaneWpfHostControl"/> class.
        /// </summary>
        public TaskPaneWpfHostControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the WPF Element Host
        /// </summary>
        public ElementHost WpfElementHost
        {
            get
            {
                return this.wpfElementHost;
            }
        }

        /// <summary>
        /// Sets the provided <see cref="UIElement"/> as content of the current Host Control
        /// </summary>
        /// <param name="view">
        /// The <see cref="UIElement"/> that needs to be added to the host control
        /// </param>
        public void SetContent(UIElement view)
        {
            this.WpfElementHost.HostContainer.Children.Add(view);
        }
    }
}
