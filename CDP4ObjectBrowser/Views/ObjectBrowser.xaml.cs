﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectBrowser.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ObjectBrowser
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;
    
    /// <summary>
    /// Interaction logic for ObjectBrowserView
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class ObjectBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectBrowser"/> class.
        /// </summary>
        public ObjectBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ObjectBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}