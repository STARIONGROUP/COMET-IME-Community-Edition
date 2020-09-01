// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterRowControlViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Composition.ViewModels
{
    using System.Linq;

    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Views;

    using ReactiveUI;

    /// <summary>
    /// Represents an element parameters to use within the <see cref="ElementParameterRowControl"/>
    /// </summary>
    public class ParameterRowControlViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the parameter name
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Gets or sets the parameter name
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Backing field for <see cref="Value"/>
        /// </summary>
        private string value;

        /// <summary>
        /// Gets or sets the parameter name
        /// </summary>
        public string Value
        {
            get => this.value;
            set => this.RaiseAndSetIfChanged(ref this.value, value);
        }

        /// <summary>
        /// Backing field for <see cref="OwnerShortName"/>
        /// </summary>
        private string ownerShortName;

        /// <summary>
        /// Gets or sets the parameter Owner Short Name
        /// </summary>
        public string OwnerShortName
        {
            get => this.ownerShortName;
            set => this.RaiseAndSetIfChanged(ref this.ownerShortName, value);
        }

        /// <summary>
        /// Backing field for <see cref="Switch"/>
        /// </summary>
        private string switchField;

        /// <summary>
        /// Gets or sets the parameter Switch
        /// </summary>
        public string Switch
        {
            get => this.switchField;
            set => this.RaiseAndSetIfChanged(ref this.switchField, value);
        }

        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        private string description;

        /// <summary>
        /// Gets or sets the parameter Description
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.RaiseAndSetIfChanged(ref this.description, value);
        }

        /// <summary>
        /// Backing field for <see cref="ModelCode"/>
        /// </summary>
        private string modelCode;

        /// <summary>
        /// Gets or sets the parameter ModelCode
        /// </summary>
        public string ModelCode
        {
            get => this.modelCode;
            set => this.RaiseAndSetIfChanged(ref this.modelCode, value);
        }

        /// <summary>
        /// Backing field for <see cref="RowType"/>
        /// </summary>
        private string rowType;

        /// <summary>
        /// Gets or sets the parameter Row Type
        /// </summary>
        public string RowType
        {
            get => this.rowType;
            set => this.RaiseAndSetIfChanged(ref this.rowType, value);
        }

        /// <summary>
        /// Gets the <see cref="Parameter"/> this view model represents
        /// </summary>
        public ParameterOrOverrideBase Parameter { get; private set; }

        /// <summary>
        /// Gets the Actual Option
        /// </summary>
        public Option ActualOption { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ParameterRowControlViewModel"/>
        /// </summary>
        /// <param name="parameter">The <see cref="Parameter"/> this view model represents</param>
        /// <param name="actualOption">The actual Option</param>
        public ParameterRowControlViewModel(ParameterOrOverrideBase parameter, Option actualOption)
        {
            this.Parameter = parameter;
            this.ActualOption = actualOption;
            this.UpdateProperties();
        }

        /// <summary>
        /// Update and set all this view model properties
        /// </summary>
        private void UpdateProperties()
        {
            var valueSet = this.GetValueSet();

            this.Name = this.Parameter.ParameterType.Name;
            this.ShortName = this.Parameter.ParameterType.ShortName;
            this.Value = $"{valueSet?.Published.FirstOrDefault()} [{this.Parameter.Scale.ShortName}]";
            this.OwnerShortName = this.Parameter.Owner.ShortName;
            this.Switch = valueSet?.ValueSwitch.ToString();
            this.Description = "-";
            this.ModelCode = this.Parameter.ModelCode();
            this.RowType = this.Parameter.ClassKind.ToString();
        }

        /// <summary>
        /// Gets the <see cref="ParameterValueSetBase"/> for an <see cref="Option"/> (if this <see cref="ParameterOrOverrideBase"/> is option dependent) and a <see cref="ActualFiniteState"/> (if it is state dependent)
        /// </summary>
        /// <returns>The <see cref="ParameterValueSetBase"/> if a value is defined for the <see cref="Option"/></returns>
        protected ParameterValueSetBase GetValueSet()
        {
            ParameterValueSetBase valueSet = null;

            if (this.Parameter is ParameterOverride parameterOverride)
            {
                valueSet = parameterOverride.ValueSet.SingleOrDefault(x => (!this.Parameter.IsOptionDependent || (x.ActualOption == this.ActualOption)));
            }
            else if (this.Parameter is Parameter parameter)
            {
                valueSet = parameter.ValueSet.SingleOrDefault(x => (!this.Parameter.IsOptionDependent || (x.ActualOption == this.ActualOption)));
            }

            return valueSet;
        }
    }
}
