// -------------------------------------------------------------------------------------------------
// <copyright file="DomainFileStoreBrowser.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System.Windows.Controls;

    using CDP4Composition;
    using CDP4Composition.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.ComponentModel.Composition;

    /// <summary>
    /// Interaction logic for CommonFileStoreBrowser
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class DomainFileStoreBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreBrowser"/> class.
        /// </summary>
        public DomainFileStoreBrowser()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommonFileStoreBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public DomainFileStoreBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
