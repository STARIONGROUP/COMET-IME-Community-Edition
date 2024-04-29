﻿// ------------------------------------------------------------------------------------------------
// <copyright file="BudgetConfig.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Budget.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for CategoryDialog
    /// </summary>
    [DialogViewExport("BudgetConfigViewModel", "The budget configuration view")]
    public partial class BudgetConfig : IDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetConfig"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is used by MEF to instation the view. The view is instantiated to enable navigation using the <see cref="IThingDialogNavigationService"/>
        /// </remarks>
        public BudgetConfig()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetConfig"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public BudgetConfig(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
