﻿// ------------------------------------------------------------------------------------------------
// <copyright file="EngineeringModelSetupDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for EngineeringModelSetupDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.EngineeringModelSetup)]
    public partial class EngineeringModelSetupDialog : DXWindow, IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupDialog"/> class.
        /// </summary>
        public EngineeringModelSetupDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModelSetupDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public EngineeringModelSetupDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
