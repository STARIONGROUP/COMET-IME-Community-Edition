// -------------------------------------------------------------------------------------------------
// <copyright file="ParameterConfigViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------


namespace CDP4Budget.ViewModels
{
    using System;
    using System.Collections.Generic;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// The view model to select a parameter-type with its optional margin
    /// </summary>
    public class ParameterConfigViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="SelectedParameterType"/>
        /// </summary>
        private QuantityKind selectedParameterType;

        /// <summary>
        /// Backing field for <see cref="SelectedMarginParameterType"/>
        /// </summary>
        private QuantityKind selectedMarginParameterType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterConfigViewModel"/> class
        /// </summary>
        /// <param name="possibleParameterTypes">the possible <see cref="QuantityKind"/> that may be used</param>
        /// <param name="validateMainForm">The action to validate the main form</param>
        public ParameterConfigViewModel(IReadOnlyList<QuantityKind> possibleParameterTypes, Action validateMainForm)
        {
            this.PossibleParameterTypes = new ReactiveList<QuantityKind>(possibleParameterTypes);
            this.WhenAnyValue(x => x.SelectedParameterType).Subscribe(_ => validateMainForm());
        }

        /// <summary>
        /// Gets the <see cref="QuantityKind"/>
        /// </summary>
        public ReactiveList<QuantityKind> PossibleParameterTypes { get; private set; }

        /// <summary>
        /// Gets or sets the selected <see cref="QuantityKind"/>
        /// </summary>
        public QuantityKind SelectedParameterType
        {
            get { return this.selectedParameterType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedParameterType, value); }
        }

        /// <summary>
        /// GEts or sets the Margin parameter-type
        /// </summary>
        public QuantityKind SelectedMarginParameterType
        {
            get { return this.selectedMarginParameterType; }
            set { this.RaiseAndSetIfChanged(ref this.selectedMarginParameterType, value); }
        }
    }
}
