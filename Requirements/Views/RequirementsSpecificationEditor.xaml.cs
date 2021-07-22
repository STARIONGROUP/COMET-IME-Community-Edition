// ------------------------------------------------------------------------------------------------
// <copyright file="RequirementsSpecificationEditor.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.Views
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;

    using CDP4Composition;

    /// <summary>
    /// Interaction logic for RequirementsSpecificationEditor.xaml
    /// </summary>
    [Export(typeof(IPanelView))]
    public partial class RequirementsSpecificationEditor : UserControl, IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationEditor"/> class
        /// </summary>
        public RequirementsSpecificationEditor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequirementsSpecificationEditor"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public RequirementsSpecificationEditor(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
