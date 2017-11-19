// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionsBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for ElementDefinitions view
    /// </summary>
    [PanelViewExport(RegionNames.LeftPanel)]
    public partial class ElementDefinitionsBrowser : IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionsBrowser"/> class
        /// </summary>
        public ElementDefinitionsBrowser()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionsBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ElementDefinitionsBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}