// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationTrackParameterDetail.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2020 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Dashboard.Views.Widget
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dashboard.ViewModels.Widget;

    /// <summary>
    /// Interaction logic for IterationTrackParameterDetail.xaml
    /// </summary>
    [DialogViewExport(nameof(IterationTrackParameterDetailViewModel), "The track parameter view")]
    public partial class IterationTrackParameterDetail : IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IterationTrackParameterDetail"/> class.
        /// </summary>
        public IterationTrackParameterDetail()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationTrackParameterDetail"/> class.
        /// </summary>
        public IterationTrackParameterDetail(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
