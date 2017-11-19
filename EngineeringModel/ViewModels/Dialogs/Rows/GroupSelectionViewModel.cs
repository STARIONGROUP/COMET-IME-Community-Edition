// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupSelectionViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using ReactiveUI;

    /// <summary>
    /// This class is used as the row view-model for group selection in a combobox
    /// </summary>
    public class GroupSelectionViewModel : ReactiveObject
    { 
        /// <summary>
        /// Backing field for <see cref="DisplayedName"/>
        /// </summary>
        private string displayedName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupSelectionViewModel"/> class
        /// </summary>
        /// <param name="group">The <see cref="ParameterGroup"/></param>
        public GroupSelectionViewModel(ParameterGroup group)
        {
            this.ContainedGroups = new ReactiveList<GroupSelectionViewModel>();
            this.Thing = group;
            this.Parent = this.Thing.ContainingGroup;
            this.DisplayedName = this.Thing.Name;
        }

        /// <summary>
        /// Gets the contained group view-model
        /// </summary>
        public ReactiveList<GroupSelectionViewModel> ContainedGroups { get; private set; }

        /// <summary>
        /// Gets the represented <see cref="ParameterGroup"/>
        /// </summary>
        public ParameterGroup Thing { get; private set; }

        /// <summary>
        /// Gets the containing <see cref="ParameterGroup"/>
        /// </summary>
        public ParameterGroup Parent { get; private set; }

        /// <summary>
        /// Gets the displayed name
        /// </summary>
        public string DisplayedName
        {
            get { return this.displayedName; }
            private set { this.RaiseAndSetIfChanged(ref this.displayedName, value);}
        }
    }
}