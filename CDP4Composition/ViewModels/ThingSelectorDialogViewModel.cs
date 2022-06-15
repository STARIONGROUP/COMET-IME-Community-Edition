// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThingSelectorDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive;

    using CDP4Common.CommonData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ThingSelectorDialogViewModel{T}"/> is to select a specific <see cref="Thing"/> from a list of <see cref="Thing"/>s.
    /// </summary>
    /// <typeparam name="T">The specific <see cref="Type"/> of the list of <see cref="Thing"/>s </typeparam>
    public class ThingSelectorDialogViewModel<T> : ReactiveObject where T : Thing
    {
        private IEnumerable<T> things;
        private IEnumerable<dynamic> columns;
        private T selectedThing;
        private string title;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThingSelectorDialogViewModel{T}"/> class.
        /// </summary>
        /// <param name="things">A list of <see cref="Thing"/>s from where the user can select a <see cref="Thing"/></param>
        /// <param name="fieldNames">The names of the fields in the <see cref="Thing"/>s that will be shown as columns in the UI.</param>
        public ThingSelectorDialogViewModel(IEnumerable<T>things, IEnumerable<string> fieldNames)
        {
            this.Things = things;
            this.Columns = fieldNames.Select(x => new { FieldName = x });

            this.Title = $"Please Select A {typeof(T).Name}";

            var canOk = this.WhenAnyValue(vm => vm.SelectedThing).Select(thing => thing != null);
            this.OkCommand = ReactiveCommandCreator.Create(this.ExecuteOk, canOk);

            this.CancelCommand = ReactiveCommandCreator.Create(this.ExecuteCancel);
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
        /// Gets or sets the Columns property
        /// </summary>
        public IEnumerable<dynamic> Columns
        {
            get { return this.columns; }
            set { this.RaiseAndSetIfChanged(ref this.columns, value); }
        }

        /// <summary>
        /// Gets or sets the Title property
        /// </summary>
        public string Title
        {
            get { return this.title; }
            set { this.RaiseAndSetIfChanged(ref this.title, value); }
        }

        /// <summary>
        /// Gets the Ok Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; private set; }

        /// <summary>
        /// Gets the Cancel Command
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

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
