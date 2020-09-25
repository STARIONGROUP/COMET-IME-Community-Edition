// ------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeComponentRowViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft,
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ProductTree.ViewModels
{
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services.NestedElementTreeService;

    using CDP4Dal;
    using CDP4Dal.Events;

    using Microsoft.Practices.ServiceLocation;

    using ReactiveUI;

    /// <summary>
    /// Represents a <see cref="ParameterTypeComponent"/>
    /// </summary>
    public class ParameterTypeComponentRowViewModel : CDP4CommonView.ParameterTypeComponentRowViewModel, IModelCodeRowViewModel
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name; 

        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private string value;

        /// <summary>
        /// Backing field for <see cref="Switch"/>
        /// </summary>
        private ParameterSwitchKind switchValue;

        /// <summary>
        /// Backing field for <see cref="IsPublishable"/>
        /// </summary>
        private bool isPublishable;

        /// <summary>
        /// Backing field for the <see cref="OwnerShortName"/> property.
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// The <see cref="INestedElementTreeService"/>
        /// </summary>
        private readonly INestedElementTreeService nestedElementTreeService = ServiceLocator.Current.GetInstance<INestedElementTreeService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeComponentRowViewModel"/> class
        /// </summary>
        /// <param name="component">The <see cref="ParameterTypeComponent"/> represented</param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public ParameterTypeComponentRowViewModel(ParameterTypeComponent component, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(component, session, containerViewModel)
        {
            this.Name = this.Thing.ShortName;
            this.IsPublishable = false;

            var parameterOrOverrideBaseRowViewModel = containerViewModel as ParameterOrOverrideBaseRowViewModel;
            if (parameterOrOverrideBaseRowViewModel != null)
            {
                this.OwnerShortName = parameterOrOverrideBaseRowViewModel.OwnerShortName;
            }
        }

        /// <summary>
        /// Gets the model-code
        /// </summary>
        public string ModelCode
        {
            get { return this.modelCode; }
            private set { this.RaiseAndSetIfChanged(ref this.modelCode, value); }
        }

        /// <summary>
        /// Calculates the Path
        /// </summary>
        public string GetPath()
        {
            if (!(this.ContainerViewModel is ParameterOrOverrideBaseRowViewModel parameterOrOverrideBaseRowViewModel))
            {
                return "";
            }

            return this.nestedElementTreeService.GetNestedParameterPath(parameterOrOverrideBaseRowViewModel.Thing, parameterOrOverrideBaseRowViewModel.Option);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current represented <see cref="ParameterValueSetBase"/> is publishable
        /// </summary>
        public bool IsPublishable
        {
            get { return this.isPublishable; }
            set { this.RaiseAndSetIfChanged(ref this.isPublishable, value); }
        }

        /// <summary>
        /// Gets or sets the Switch of this row
        /// </summary>
        public ParameterSwitchKind Switch
        {
            get { return this.switchValue; }
            set { this.RaiseAndSetIfChanged(ref this.switchValue, value); }
        }

        /// <summary>
        /// Gets or sets the Value of this row
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set { this.RaiseAndSetIfChanged(ref this.value, value); }
        }

        /// <summary>
        /// Gets or sets the Name of this row
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.RaiseAndSetIfChanged(ref this.name, value); }
        }

        /// <summary>
        /// Gets or sets the short name of the <see cref="DomainOfExpertise"/> that owns the container
        /// <see cref="ParameterOrOverrideBase"/>
        /// </summary>
        public string OwnerShortName
        {
            get { return this.ownerShortName; }
            set { this.RaiseAndSetIfChanged(ref this.ownerShortName, value); }
        }

        /// <summary>
        /// Set the value for this row from the <see cref="ParameterOrOverrideBase"/> and the associated <see cref="ParameterValueSetBase"/>
        /// </summary>
        /// <param name="parameterOrOveride">The <see cref="ParameterOrOverrideBase"/></param>
        /// <param name="valueSet">The <see cref="ParameterValueSetBase"/></param>
        /// <param name="index">The Index of the Value to get</param>
        public void SetScalarValue(ParameterOrOverrideBase parameterOrOveride, ParameterValueSetBase valueSet, int? index = null)
        {
            this.Value = index.HasValue && valueSet.Published.Count() > index
                ? valueSet.Published[index.Value]
                : valueSet.Published.FirstOrDefault();

            var actualValue = index.HasValue && valueSet.Published.Count() > index 
                ? valueSet.ActualValue[index.Value] 
                : valueSet.ActualValue.FirstOrDefault();

            this.ModelCode = index.HasValue ? valueSet.ModelCode(index.Value) : valueSet.ModelCode();

            this.Switch = valueSet.ValueSwitch;
            if (this.Value == null || actualValue == null)
            {
                return;
            }

            if (this.Value != actualValue)
            {
                this.IsPublishable = true;
            }

            if (this.Scale != null)
            {
                this.Value += " [" + this.Scale.ShortName + "]";
            }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing"/> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.Name = this.Thing.ShortName;
        }
    }
}