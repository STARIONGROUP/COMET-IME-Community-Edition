// ------------------------------------------------------------------------------------------------
// <copyright file="ParticipantDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for ParticipantDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.Participant)]
    public partial class ParticipantDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantDialog"/> class.
        /// </summary>
        public ParticipantDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParticipantDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public ParticipantDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
