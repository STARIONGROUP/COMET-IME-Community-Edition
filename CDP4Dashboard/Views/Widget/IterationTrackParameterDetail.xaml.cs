// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationTrackParameterDetail.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2020 RHEA System S.A.
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
        public IterationTrackParameterDetail()
        {
            this.InitializeComponent();
        }

        public IterationTrackParameterDetail(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
