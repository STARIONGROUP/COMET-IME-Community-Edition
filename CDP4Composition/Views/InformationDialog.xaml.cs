﻿// -------------------------------------------------------------------------------------------------
// <copyright file="InformationDialog.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Views
{
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;
    using DevExpress.Xpf.Core;

    /// <summary>
    /// Interaction logic for InformationDialog.xaml
    /// </summary>
    [DialogViewExport("InformationDialog", "The information dialog")]
    public partial class InformationDialog : DXWindow, IDialogView
    {
        public InformationDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">a value indicating whether the contained Components shall be loaded</param>
        /// <remarks>This constructor is called by the navigation service</remarks>
        public InformationDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
