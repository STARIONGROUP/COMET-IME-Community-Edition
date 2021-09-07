// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptKindViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
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
//    along with this program. If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.CommonView.ViewModels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// View model that represents a script <see cref="BehavioralModelKind"/> 
    /// </summary>
    public class ScriptKindViewModel : ReactiveObject, IBehavioralModelKindViewModel
    {
        /// <summary>
        /// The <see cref="ISession"/>
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The <see cref="BehaviorDialogViewModel" container of this view model/>
        /// </summary>
        private readonly BehaviorDialogViewModel containerViewModel;

        /// <summary>
        /// Backing field for <see cref="Script"/>
        /// </summary>
        private string script;

        /// <summary>
        /// Backing field for <see cref="SelectedBehaviorParameter"/>
        /// </summary>
        private BehavioralParameterRowViewModel selectedBehaviorParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptKindViewModel"/> class
        /// </summary>
        /// <param name="behavioralParameters">The <see cref="BehavioralParameter"/>s for this script</param>
        /// <param name="script"></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="containerViewModel">The <see cref="BehaviorDialogViewModel" container of this view model/></param>
        /// <param name="parameter">The selectable <see cref="Parameter"/>s</param>
        public ScriptKindViewModel(IEnumerable<BehavioralParameter> behavioralParameters, string script, ISession session, BehaviorDialogViewModel containerViewModel, ContainerList<Parameter> parameter)
        {
            this.BehaviorParameter = new ReactiveList<BehavioralParameterRowViewModel>();
            this.BehaviorParameter.ChangeTrackingEnabled = true;

            this.Script = script;
            this.session = session;
            this.containerViewModel = containerViewModel;
            this.Parameters = parameter;

            this.BehaviorParameter.AddRange(behavioralParameters
                .Select(p => new BehavioralParameterRowViewModel(p, session, containerViewModel)));

            var canExecuteAddParameterCommand = this.WhenAnyValue(vm => vm.IsReadOnly, vm => vm.Parameters, (r, p) => !r && p.Any());
            this.AddParameterCommand = ReactiveCommand.Create(canExecuteAddParameterCommand);
            this.AddParameterCommand.Subscribe(_ => this.AddParameter());

            var canExecuteDeleteParameterCommand = this.WhenAnyValue(vm => vm.IsReadOnly, vm => vm.Parameters, vm => vm.SelectedBehaviorParameter, (r, p, s) => !r && p.Any() && s is not null);
            this.DeleteParameterCommand = ReactiveCommand.Create(canExecuteDeleteParameterCommand);
            this.DeleteParameterCommand.Subscribe(_ => this.DeleteParameter());
        }

        /// <summary>
        /// Gets a value indicating whether the associated view is read-only
        /// </summary>
        public bool IsReadOnly => this.containerViewModel.IsReadOnly;

        /// <summary>
        /// The selectable <see cref="Parameter"/>s
        /// </summary>
        public IEnumerable<Parameter> Parameters { get; }

        /// <summary>
        /// The available <see cref="BehavioralParameterKind"/>s
        /// </summary>
        public IEnumerable ParameterKinds => Enum.GetValues(typeof(BehavioralParameterKind));

        /// <summary>
        /// Removes the selected <see cref="BehavioralParameter"/> from the <see cref="BehaviorParameter"/> collection
        /// </summary>
        private void DeleteParameter()
        {
            this.BehaviorParameter.Remove(this.selectedBehaviorParameter);
        }

        /// <summary>
        /// Adds a new <see cref="BehavioralParameter"/> to the <see cref="BehaviorParameter"/> collection
        /// </summary>
        private void AddParameter()
        {
            this.BehaviorParameter.Add(new BehavioralParameterRowViewModel(new BehavioralParameter(), this.session, this.containerViewModel));
        }

        /// <summary>
        /// <see cref="ReactiveCommand"/> to add a new parameter 
        /// </summary>
        public ReactiveCommand<object> AddParameterCommand { get; }

        /// <summary>
        /// <see cref="ReactiveCommand"/> to delete a parameter 
        /// </summary>
        public ReactiveCommand<object> DeleteParameterCommand { get; }

        /// <summary>
        /// The text of the script
        /// </summary>
        public string Script
        {
            get => this.script;
            set => this.RaiseAndSetIfChanged(ref this.script, value);
        }

        /// <summary>
        /// Returns the if the OK command can be executed for this view model
        /// </summary>
        /// <returns>The ok status</returns>
        public bool OkCanExecute()
        {
            return !string.IsNullOrWhiteSpace(this.Script) && this.BehaviorParameter.All(p => p.IsValid);
        }

        /// <summary>
        /// Update the transaction with the <see cref="BehavioralModelKind"/> information represented by this view model
        /// </summary>
        /// <param name="transaction">The transaction for the <see cref="Thing"/></param>
        /// <param name="clone">The <see cref="Behavior"/> for which to update the <see cref="IThingTransaction"/></param>
        public void UpdateTransaction(IThingTransaction transaction, Behavior clone)
        {
            clone.Attachment.Clear();
            clone.Script = this.Script;
            clone.BehavioralParameter.Clear();
            clone.BehavioralParameter.AddRange(this.BehaviorParameter.Select(b =>
            {
                b.UpdateTransaction(transaction);
                return b.Thing;
            }));
        }

        /// <summary>
        /// The <see cref="BehavioralParameterRowViewModel"/>s for this script
        /// </summary>
        public ReactiveList<BehavioralParameterRowViewModel> BehaviorParameter { get; }

        /// <summary>
        /// Gets or sets the selected <see cref="BehaviorRowViewModel"/>
        /// </summary>
        public BehavioralParameterRowViewModel SelectedBehaviorParameter
        {
            get => this.selectedBehaviorParameter;
            set => this.RaiseAndSetIfChanged(ref this.selectedBehaviorParameter, value);
        }
    }
}
