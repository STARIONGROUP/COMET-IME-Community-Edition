﻿// ------------------------------------------------------------------------------------------------
// <copyright file="PersonRoleDialog.xaml.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.Views
{
    using CDP4Common.CommonData;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation.Interfaces;

    /// <summary>
    /// Interaction logic for PersonRoleDialog
    /// </summary>
    [ThingDialogViewExport(ClassKind.PersonRole)]
    public partial class PersonRoleDialog : IThingDialogView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRoleDialog"/> class.
        /// </summary>
        public PersonRoleDialog()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonRoleDialog"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the <see cref="IThingDialogNavigationService"/>.
        /// </remarks>
        public PersonRoleDialog(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }
    }
}
