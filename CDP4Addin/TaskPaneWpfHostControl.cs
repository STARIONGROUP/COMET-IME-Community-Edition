// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TaskPaneWpfHostControl.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Addin
{
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Forms.Integration;

    /// <summary>
    /// The purpose of the <see cref="TaskPaneWpfHostControl"/> is to act as container or host for
    /// the WPF controls of the CDP4 plugins
    /// </summary>
    [Guid("76CD03FA-0532-45A4-A222-5A14669D805C")]
    [ProgId("CDP4Addin.TaskPaneWpfHostControl")]
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
