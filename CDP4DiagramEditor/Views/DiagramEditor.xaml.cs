// ------------------------------------------------------------------------------------------------
// <copyright file="CDP4DiagramEditor.xaml.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Views
{
    using CDP4Composition;
    using CDP4Composition.Attributes;

    /// <summary>
    /// Interaction logic for CDP4DiagramEditor.xaml
    /// </summary>
    [PanelViewExport(RegionNames.EditorPanel)]
    public partial class DiagramEditor : IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramEditor"/> class
        /// </summary>
        /// <remarks>
        /// Called by MEF
        /// </remarks>
        public DiagramEditor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramEditor"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public DiagramEditor(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}