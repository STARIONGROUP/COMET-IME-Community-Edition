// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IterationSetupRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4SiteDirectory.ViewModels
{
    using System.Globalization;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;

    /// <summary>
    /// the view model for <see cref="IterationSetup"/> displayed in the Tree
    /// </summary>
    public class IterationSetupRowViewModel : CDP4CommonView.IterationSetupRowViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IterationSetupRowViewModel"/> class
        /// </summary>
        /// <param name="iterationSetup">The <see cref="IterationSetup"/> this is associated to</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public IterationSetupRowViewModel(IterationSetup iterationSetup, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(iterationSetup, session, containerViewModel)
        {
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Updates the Column values
        /// </summary>
        private void UpdateProperties()
        {
            this.Name = "iteration_" + this.Thing.IterationNumber.ToString(CultureInfo.InvariantCulture);
            this.RowStatus = this.Thing.IsDeleted ? RowStatusKind.Inactive : RowStatusKind.Active;
        }

        /// <summary>
        /// The object changed event handler
        /// </summary>
        /// <param name="objectChange">The <see cref="ObjectChangedEvent"/></param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }
    }
}