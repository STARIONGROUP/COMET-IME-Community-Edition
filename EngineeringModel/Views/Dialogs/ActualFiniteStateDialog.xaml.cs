﻿// ------------------------------------------------------------------------------------------------
// <copyright file="ActualFiniteStateDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for ActualFiniteStateDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.ActualFiniteState)]
    public partial class ActualFiniteStateDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateDialog"/> class.
        /// </summary>
        public ActualFiniteStateDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActualFiniteStateDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public ActualFiniteStateDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
