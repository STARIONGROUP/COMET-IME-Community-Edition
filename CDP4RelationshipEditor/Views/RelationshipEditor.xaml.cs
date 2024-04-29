﻿// ------------------------------------------------------------------------------------------------
// <copyright file="RelationshipEditor.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4RelationshipEditor.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for RelationshipEditor.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class RelationshipEditor : UserControl, IPanelView
    {
        public RelationshipEditor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipEditor"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public RelationshipEditor(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
