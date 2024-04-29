// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterBaseRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using ReactiveUI;

    /// <summary>
    /// Extended hand-coded part for the auto-generated <see cref="ParameterBaseRowViewModel{T}"/>
    /// </summary>
    public abstract partial class ParameterBaseRowViewModel<T>
    {
        #region Fields
        /// <summary>
        /// The <see cref="Option"/> being used
        /// </summary>
        private Option option;

        /// <summary>
        /// The backing field for the <see cref="Value"/> property.
        /// </summary>
        private string value;

        /// <summary>
        /// The backing field for the <see cref="Published"/> property.
        /// </summary>
        private string published;

        /// <summary>
        /// The backing field for the <see cref="State"/> property.
        /// </summary>
        private string state;

        /// <summary>
        /// The backing field for the <see cref="Computed"/> property.
        /// </summary>
        private string computed;

        /// <summary>
        /// The backing field for the <see cref="Manual"/> property.
        /// </summary>
        private object manual;

        /// <summary>
        /// The backing field for the <see cref="Reference"/> property.
        /// </summary>
        private object reference;

        /// <summary>
        /// The backing field for the <see cref="Switch"/> property.
        /// </summary>
        private ParameterSwitchKind? switchValue;

        /// <summary>
        /// The backing field for the <see cref="Name"/> property
        /// </summary>
        private string name;
        
        #endregion

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets the ShortName
        /// </summary>
        public string ShortName
        {
            get { return this.Name; }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set { this.RaiseAndSetIfChanged(ref this.value, value); }
        }

        /// <summary>
        /// Gets or sets the published value.
        /// </summary>
        public string Published
        {
            get { return this.published; }
            set { this.RaiseAndSetIfChanged(ref this.published, value); }
        }

        /// <summary>
        /// Gets or sets the switch.
        /// </summary>
        public ParameterSwitchKind? Switch
        {
            get { return this.switchValue; }
            set { this.RaiseAndSetIfChanged(ref this.switchValue, value); }
        }

        /// <summary>
        /// Gets or sets the computed value.
        /// </summary>
        public string Computed
        {
            get { return this.computed; }
            set { this.RaiseAndSetIfChanged(ref this.computed, value); }
        }

        /// <summary>
        /// Gets or sets the manual value.
        /// </summary>
        public object Manual
        {
            get { return this.manual; }
            set { this.RaiseAndSetIfChanged(ref this.manual, value); }
        }

        /// <summary>
        /// Gets or sets the reference value.
        /// </summary>
        public object Reference
        {
            get { return this.reference; }
            set { this.RaiseAndSetIfChanged(ref this.reference, value); }
        }

        /// <summary>
        /// Gets or sets the reference value.
        /// </summary>
        public Option Option
        {
            get { return this.option; }
            protected set { this.RaiseAndSetIfChanged(ref this.option, value); }
        }

        /// <summary>
        /// Gets or sets the reference value.
        /// </summary>
        public string State
        {
            get { return this.state; }
            protected set { this.RaiseAndSetIfChanged(ref this.state, value); }
        }

        /// <summary>
        /// Computes the entire row or specific property of the row is editable based on the
        /// result of the <see cref="PermissionService.CanWrite"/> method and potential
        /// conditions of the property of the Row that is being edited.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property for which the value is computed. This allows to include the
        /// specific property of the row-view-model in the computation. If the propertyname is empty
        /// then the whole row is taken into account. If a property is specified only that property
        /// is taken into account.
        /// </param>
        /// <returns>
        /// True if the row or more specific the property is editable or not.
        /// </returns>
        public override bool IsEditable(string propertyName = "")
        {
            if (this.ContainedRows.Any())
            {
                return false;
            }

            return base.IsEditable(propertyName);            
        }
    }
}