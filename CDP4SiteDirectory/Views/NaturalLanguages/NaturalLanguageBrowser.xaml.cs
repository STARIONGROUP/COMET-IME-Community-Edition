﻿// -------------------------------------------------------------------------------------------------
// <copyright file="NaturalLanguageBrowser.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for NaturalLanguageBrowser
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class NaturalLanguageBrowser : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalLanguageBrowser"/> class.
        /// </summary>
        public NaturalLanguageBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NaturalLanguageBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public NaturalLanguageBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
