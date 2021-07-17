// -------------------------------------------------------------------------------------------------
// <copyright file="RelationshipMatrix.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition;
    using CDP4Composition.Attributes;    
    
    /// <summary>
    /// Interaction logic for RelationshipMatrix view
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class RelationshipMatrix : IPanelView
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