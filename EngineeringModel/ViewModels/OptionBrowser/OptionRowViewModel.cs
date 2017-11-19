// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The extended row class representing an <see cref="Option"/>
    /// </summary>
    public class OptionRowViewModel : CDP4CommonView.OptionRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="IsDefaultOption"/>
        /// </summary>
        private bool isDefaultOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionRowViewModel"/> class
        /// </summary>
        /// <param name="option">The <see cref="Option"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public OptionRowViewModel(Option option, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(option, session, containerViewModel)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is the default <see cref="Option"/>
        /// </summary>
        public bool IsDefaultOption
        {
            get { return this.isDefaultOption; }
            set { this.RaiseAndSetIfChanged(ref this.isDefaultOption, value); }
        }
    }
}