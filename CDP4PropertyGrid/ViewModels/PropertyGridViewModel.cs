// -------------------------------------------------------------------------------------------------
// <copyright file="PropertyGridViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4PropertyGrid.ViewModels
{
    using System;
    using System.ComponentModel.Composition;

    using CDP4Common.CommonData;
    using CDP4Composition;
    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4PropertyGrid.Views;

    /// <summary>
    /// The view-model for the <see cref="PropertyGrid"/> that displays the properties of a <see cref="Thing"/>
    /// </summary>
    [Export(typeof(IPanelViewModel))]
    public class PropertyGridViewModel : ViewModelBase<Thing>, IPanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridViewModel"/> class
        /// </summary>
        public PropertyGridViewModel()
        {
            this.Identifier = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGridViewModel"/> class
        /// </summary>
        /// <param name="thing">
        /// The <see cref="Thing"/> to display
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public PropertyGridViewModel(Thing thing, ISession session)
            : base(thing, session)
        {
        }

        /// <summary>
        /// Gets the Caption
        /// </summary>
        public string Caption
        {
            get { return "Properties"; }
        }

        /// <summary>
        /// Gets a value indicating whether this is dirty
        /// </summary>
        public bool IsDirty
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the unique identifier of the view-model
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// Gets the Tooltip
        /// </summary>
        public string ToolTip
        {
            get { return "Displays the properties of the selected thing"; }
        }

        /// <summary>
        /// Gets the data-source
        /// </summary>
        public string DataSource
        {
            get { return this.Session.DataSourceUri; }
        }
    }
}