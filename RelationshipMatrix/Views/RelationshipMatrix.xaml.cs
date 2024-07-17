﻿// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrix.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2019 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;
    
    /// <summary>
    /// Interaction logic for RelationshipMatrix view
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class RelationshipMatrix : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrix"/> class
        /// </summary>
        public RelationshipMatrix()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationshipMatrix"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public RelationshipMatrix(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}