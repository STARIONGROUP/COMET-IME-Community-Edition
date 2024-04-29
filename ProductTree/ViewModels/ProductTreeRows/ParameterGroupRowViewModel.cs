// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterGroupRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using System.Collections.Generic;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using CDP4Dal.Events;
    using CDP4ProductTree.Comparers;

    /// <summary>
    /// The row-view-model representing a <see cref="ParameterGroup"/>
    /// </summary>
    public class ParameterGroupRowViewModel : CDP4CommonView.ParameterGroupRowViewModel, IHaveContainedModelCodes
    {
        /// <summary>
        /// The current <see cref="ParameterGroup"/>
        /// </summary>
        private ParameterGroup currentGroup;

        /// <summary>
        /// The <see cref="IComparer{T}"/>
        /// </summary>
        public static readonly IComparer<IRowViewModelBase<Thing>> ChildRowComparer = new ParameterGroupChildRowComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterGroupRowViewModel"/> class
        /// </summary>
        /// <param name="parameterGroup">The <see cref="ParameterGroup"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParameterGroupRowViewModel(ParameterGroup parameterGroup, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(parameterGroup, session, containerViewModel)
        {
            this.currentGroup = this.Thing.ContainingGroup;
            this.UpdateProperties();
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

        /// <summary>
        /// Update the <see cref="ThingStatus"/> property
        /// </summary>
        protected override void UpdateThingStatus()
        {
            this.ThingStatus = new ThingStatus(this.Thing);
        }

        /// <summary>
        /// Update the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.UpdateThingStatus();
            if (this.Thing.ContainingGroup != this.currentGroup)
            {
                // update the position of this row in the parameter-group hierarchy
                ((IParameterRowContainer)this.ContainerViewModel).UpdateParameterGroupPosition(this.Thing);
                this.currentGroup = this.Thing.ContainingGroup;
            }
        }

        /// <summary>
        /// Gets the ShortName
        /// </summary>
        /// <remarks>
        /// A <see cref="ParameterGroup"/> does not have a ShortName property, therefore the <see cref="Name"/>
        /// property is returned.
        /// </remarks>
        public string ShortName
        {
            get { return this.Name; }
        }

        /// <summary>
        /// Update the model code property of all contained rows recursively
        /// </summary>
        public void UpdateModelCode()
        {
            foreach (var containedRow in this.ContainedRows)
            {
                var modelCodeRow = containedRow as IHavePath;
                modelCodeRow?.UpdateModelCode();

                if (containedRow is IHaveContainedModelCodes groupRow)
                {
                    groupRow.UpdateModelCode();
                }
            }
        }
    }
}