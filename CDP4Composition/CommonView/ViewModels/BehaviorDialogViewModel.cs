// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BehaviorDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2021 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Simon Wood
//
//    This file is part of CDP4-IME Community Edition.
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software{colon} you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation{colon} either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY{colon} without even the implied warranty of
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4CommonView;
    using CDP4Composition.Attributes;

    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="BehaviorDialogViewModel"/> is to allow an <see cref="Behavior"/> to
    /// be created or updated.
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.Behavior)]
    public class BehaviorDialogViewModel : DefinedThingDialogViewModel<Behavior>, IThingDialogViewModel
    {
        /// <summary>
        /// Backing field for the <see cref="SelectedBehavioralModelKindViewModel"/> property
        /// </summary>
        private IBehavioralModelKindViewModel selectedBehavioralModelKindViewModel;

        /// <summary>
        /// Backing field for the <see cref="SelectedBehavioralModelKind"/> property
        /// </summary>
        private BehavioralModelKind selectedBehavioralModelKind;

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public BehaviorDialogViewModel()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BehaviorDialogViewModel"/> class
        /// </summary>
        /// <param name="behavior">The <see cref="Behavior"/> this dialog view model represents</param>
        /// <param name="transaction">The <see cref="ThingTransaction"/> that contains the log of recorded changes.</param>
        /// <param name="session">The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated</param>
        /// <param name="isRoot">Assert if this <see cref="BehaviorDialogViewModel"/> is the root of all <see cref="BehaviorDialogViewModel"/></param>
        /// <param name="dialogKind">The kind of operation this <see cref="BehaviorDialogViewModel"/> performs</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/>.</param>
        /// <param name="container">The <see cref="Behavior"/> that contains the created <see cref="Behavior"/> in this Dialog</param>
        /// <param name="chainOfContainers">The optional chain of containers that contains the <paramref name="container"/> argument</param>
        public BehaviorDialogViewModel(
            Behavior behavior,
            IThingTransaction transaction,
            ISession session,
            bool isRoot,
            ThingDialogKind dialogKind,
            IThingDialogNavigationService thingDialogNavigationService,
            Thing container = null,
            IEnumerable<Thing> chainOfContainers = null)
            : base(behavior, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            if (container is not null and not DefinedThing)
            {
                throw new ArgumentException($"The container parameter is of type {container.GetType()}, it must be of type {typeof(DefinedThing)}", nameof(container));
            }

            this.Disposables.Add(
                this.WhenAnyValue(d => d.SelectedBehavioralModelKind).Subscribe(_ =>
                {
                    if (this.SelectedBehavioralModelKindViewModel is IDisposable disposable)
                    {
                        disposable?.Dispose();
                    }

                    this.SelectedBehavioralModelKindViewModel = this.CreateBehavioralModelKindViewModel(this.selectedBehavioralModelKind);
                })
            );

            this.BehavioralModelKinds = new ReactiveList<BehavioralModelKind>(Enum.GetValues(typeof(BehavioralModelKind)) as BehavioralModelKind[]);
            this.UpdateProperties();
        }

        /// <summary>
        /// List of all <see cref="BehavioralModelKind"/>s
        /// </summary>
        public ReactiveList<BehavioralModelKind> BehavioralModelKinds { get; }

        /// <summary>
        /// The currently selected <see cref="IBehavioralModelKindViewModel"/>
        /// </summary>
        public IBehavioralModelKindViewModel SelectedBehavioralModelKindViewModel
        {
            get => this.selectedBehavioralModelKindViewModel;
            set => this.RaiseAndSetIfChanged(ref this.selectedBehavioralModelKindViewModel, value);
        }

        /// <summary>
        /// The currently selected <see cref="BehavioralModelKind"/>
        /// </summary>
        public BehavioralModelKind SelectedBehavioralModelKind
        {
            get => this.selectedBehavioralModelKind;
            set => this.RaiseAndSetIfChanged(ref this.selectedBehavioralModelKind, value);
        }

        /// <summary>
        /// Facrtory method for creating a <see cref="IBehavioralModelKindViewModel"/> of the required type for a <see cref="BehavioralModelKind"/>
        /// </summary>
        /// <param name="behavioralModelKind">The <see cref="BehavioralModelKind"/> for which to create a <see cref="IBehavioralModelKindViewModel"/></param>
        /// <returns>A <see cref="IBehavioralModelKindViewModel"/></returns>
        private IBehavioralModelKindViewModel CreateBehavioralModelKindViewModel(BehavioralModelKind behavioralModelKind)
        {
            switch (behavioralModelKind)
            {
                case BehavioralModelKind.Lua:
                case BehavioralModelKind.Python:
                case BehavioralModelKind.CSharp:
                case BehavioralModelKind.Other:
                    {
                        var elementDefinition = this.Container as ElementDefinition;
                        var vm = new ScriptKindViewModel(this.Thing.BehavioralParameter, this.Thing.Script, this.Session, this, elementDefinition?.Parameter);

                        this.Disposables.Add(vm.Changed.Subscribe(_ => this.UpdateOkCanExecute()));
                        this.Disposables.Add(vm.BehaviorParameter.Changed.Subscribe(_ => this.UpdateOkCanExecute()));
                        this.Disposables.Add(vm.BehaviorParameter.ItemChanged
                                                                 .Where(i => i.PropertyName == nameof(BehavioralParameterRowViewModel.IsValid))
                                                                 .Subscribe(_ => this.UpdateOkCanExecute()));
                        return vm;
                    }
                case BehavioralModelKind.File:
                    {
                        var attachment = this.Thing.Attachment.FirstOrDefault() ?? new Attachment() { ChangeKind = this.Thing.ChangeKind };

                        var vm = new AttachmentViewModel(attachment, this.Session, this.ChainOfContainer, this.dialogKind);

                        this.Disposables.Add(vm.Changed.Subscribe(_ => this.UpdateOkCanExecute()));
                        this.Disposables.Add(vm.FileType.Changed.Subscribe(_ => this.UpdateOkCanExecute()));
                        return vm;
                    }
                default:
                    throw new ArgumentException($"Value {behavioralModelKind} is not currently supported.", nameof(behavioralModelKind));
            }
        }

        /// <summary>
        /// Update the transaction with the Thing represented by this Dialog
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = this.Thing;

            clone.Name = this.Name;
            clone.ShortName = this.Name;

            clone.BehavioralModelKind = this.SelectedBehavioralModelKind;

            this.SelectedBehavioralModelKindViewModel.UpdateTransaction(this.transaction, clone);
        }

        /// <summary>
        /// Updates the property indicating whether it is possible to close the current dialog by clicking the OK button
        /// </summary>
        /// <remarks>
        /// The <see cref="Container"/> may not be null and there may not be any Validation Errors
        /// </remarks>
        protected override void UpdateOkCanExecute()
        {
            base.UpdateOkCanExecute();

            this.OkCanExecute = this.OkCanExecute &&
                                this.SelectedBehavioralModelKindViewModel?.OkCanExecute() is true;
        }

        /// <summary>
        /// Update the properties
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            this.Name = this.Thing.Name;
            this.SelectedBehavioralModelKind = this.Thing.BehavioralModelKind;
            this.SelectedBehavioralModelKindViewModel = this.CreateBehavioralModelKindViewModel(this.Thing.BehavioralModelKind);
        }
    }
}
