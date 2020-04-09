// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingSelectorDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
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

namespace CDP4Composition.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ThingSelectorDialogViewModel{T}"/> is to allow a <see cref="File"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="File"/> will result in an <see cref="File"/> being created by
    /// the connected data-source
    /// </remarks>
    public class ThingSelectorDialogViewModel<T> : ReactiveObject where T : Thing
    {
        private IEnumerable<T> things;
        private IEnumerable<dynamic> columns;
        private T selectedThing;
        private string title;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingSelectorDialogViewModel{T}"/> class.
        /// </summary>
        public ThingSelectorDialogViewModel(IEnumerable<T>things, IEnumerable<string> fieldNames)
        {
            this.Things = things;
            this.Columns = fieldNames.Select(x => new { FieldName = x });

            this.Title = $"Please Select A {typeof(T).Name}";

            var canOk = this.WhenAnyValue(vm => vm.SelectedThing).Select(thing => thing != null);
            this.OkCommand = ReactiveCommand.Create(canOk);
            this.OkCommand.Subscribe(_ => this.ExecuteOk());

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancel());
        }

        /// <summary>
        /// Gets or sets the <see cref="SelectedThing"/> property
        /// </summary>
        public T SelectedThing
        {
            get { return this.selectedThing; }
            set { this.RaiseAndSetIfChanged(ref this.selectedThing, value); }
        }

        public bool? DialogResult { get; private set; }

        /// <summary>
        /// Gets or sets the Things property
        /// </summary>
        public IEnumerable<T> Things
        {
            get { return this.things; }
            set { this.RaiseAndSetIfChanged(ref this.things, value); }
        }

        /// <summary>
        /// Gets or sets the IsLocked property
        /// </summary>
        public IEnumerable<dynamic> Columns
        {
            get { return this.columns; }
            set { this.RaiseAndSetIfChanged(ref this.columns, value); }
        }

        public string Title
        {
            get { return this.title; }
            set { this.RaiseAndSetIfChanged(ref this.title, value); }
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<object> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; private set; }

        /// <summary>
        /// Executes the Cancel Command
        /// </summary>
        private void ExecuteCancel()
        {
            this.DialogResult = false;
        }

        /// <summary>
        /// Executes the Ok Command
        /// </summary>
        private void ExecuteOk()
        {
            this.DialogResult = true;
        }
    }
}
