// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeAssignmentRowViewModel.cs" company="RHEA System S.A.">
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
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels.Dialogs.Rows
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;

    using CDP4Dal;
    using CDP4Dal.Events;

    using ReactiveUI;

    /// <summary>
    /// A row view model for <see cref="IParameterTypeAssignment" />
    /// </summary>
    /// <typeparam name="T">The <see cref="IParameterTypeAssignment" /> type</typeparam>
    public abstract class ParameterTypeAssignmentRowViewModel<T> : RowViewModelBase<T> where T : Thing, IParameterTypeAssignment
    {
        /// <summary>
        /// All possible ParameterTypes that are used to populate list of filtered ParameterType.
        /// </summary>
        private readonly ReactiveList<ParameterType> possibleParameterTypes;

        /// <summary>
        /// Backing field for <see cref="ParameterType" />
        /// </summary>
        private ParameterType parameterType;

        /// <summary>
        /// Backing field for <see cref="ParameterTypeName" />
        /// </summary>
        private string parameterTypeName;

        /// <summary>
        /// Backing field for <see cref="ParameterTypeShortName" />
        /// </summary>
        private string parameterTypeShortName;

        /// <summary>
        /// Backing field for <see cref="Scale" />
        /// </summary>
        private MeasurementScale scale;

        /// <summary>
        /// Backing field for <see cref="ScaleName" />
        /// </summary>
        private string scaleName;

        /// <summary>
        /// Backing field for <see cref="ScaleShortName" />
        /// </summary>
        private string scaleShortName;

        /// <summary>
        /// Backing field for the <see cref="SelectedFilter" /> property.
        /// </summary>
        private string selectedFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeAssignmentRowViewModel{T}" /> class.
        /// </summary>
        /// <param name="thing">The <see cref="Thing" /> represented by the row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The parent Row</param>
        protected ParameterTypeAssignmentRowViewModel(IParameterTypeAssignment thing, ISession session, SampledFunctionParameterTypeDialogViewModel containerViewModel) : base((T)thing, session, containerViewModel)
        {
            this.PossibleScale = new ReactiveList<MeasurementScale>();
            this.FilteringOptions = new Dictionary<string, Type>();
            this.FilteringOptions.Add(string.Empty, null);

            foreach (var type in TypeResolver.GetDerivedTypes(typeof(ParameterType), typeof(ParameterType).Assembly).Where(x => !x.IsAbstract))
            {
                this.FilteringOptions.Add(type.Name, type);
            }

            this.possibleParameterTypes = containerViewModel.PossibleParameterTypes;
            this.PossibleParameterType = new ReactiveList<ParameterType>();

            this.PossibleParameterType.AddRange(this.possibleParameterTypes);

            this.Disposables.Add(
                this.WhenAnyValue(x => x.ParameterType).Subscribe(
                    _ =>
                    {
                        this.PopulatePossibleScale();
                        containerViewModel.UpdateOkCanExecuteStatus();
                    }));

            this.IsReadOnly = containerViewModel.IsReadOnly || !containerViewModel.IsCreateDialog();

            this.SelectedFilter = null;
            this.Disposables.Add(this.WhenAnyValue(x => x.SelectedFilter).Subscribe(_ => this.FilterPossibleParameterType()));
            this.UpdateProperties();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this row is read-only
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets the SelectedFilter
        /// </summary>
        public string SelectedFilter
        {
            get { return this.selectedFilter; }
            set { this.RaiseAndSetIfChanged(ref this.selectedFilter, value); }
        }

        /// <summary>
        /// Gets categories by which PossibleParameterTypes can be filtered
        /// </summary>
        public Dictionary<string, Type> FilteringOptions { get; set; }

        /// <summary>
        /// Gets all the possible <see cref="ParameterType" /> for this <see cref="ParameterTypeComponent" />
        /// </summary>
        public ReactiveList<ParameterType> PossibleParameterType { get; private set; }

        /// <summary>
        /// Gets the possible <see cref="MeasurementScale" />
        /// </summary>
        public ReactiveList<MeasurementScale> PossibleScale { get; private set; }

        /// <summary>
        /// Gets or sets the ParameterType
        /// </summary>
        public ParameterType ParameterType
        {
            get { return this.parameterType; }
            set { this.RaiseAndSetIfChanged(ref this.parameterType, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="ParameterType" />
        /// </summary>
        public string ParameterTypeShortName
        {
            get { return this.parameterTypeShortName; }
            set { this.RaiseAndSetIfChanged(ref this.parameterTypeShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="ParameterType" />
        /// </summary>
        public string ParameterTypeName
        {
            get { return this.parameterTypeName; }
            set { this.RaiseAndSetIfChanged(ref this.parameterTypeName, value); }
        }

        /// <summary>
        /// Gets or sets the Scale
        /// </summary>
        public MeasurementScale Scale
        {
            get { return this.scale; }
            set { this.RaiseAndSetIfChanged(ref this.scale, value); }
        }

        /// <summary>
        /// Gets or set the ShortName of <see cref="Scale" />
        /// </summary>
        public string ScaleShortName
        {
            get { return this.scaleShortName; }
            set { this.RaiseAndSetIfChanged(ref this.scaleShortName, value); }
        }

        /// <summary>
        /// Gets or set the Name of <see cref="Scale" />
        /// </summary>
        public string ScaleName
        {
            get { return this.scaleName; }
            set { this.RaiseAndSetIfChanged(ref this.scaleName, value); }
        }

        /// <summary>
        /// The event-handler that is invoked by the subscription that listens for updates
        /// on the <see cref="Thing" /> that is being represented by the view-model
        /// </summary>
        /// <param name="objectChange">
        /// The payload of the event that is being handled
        /// </param>
        protected override void ObjectChangeEventHandler(ObjectChangedEvent objectChange)
        {
            base.ObjectChangeEventHandler(objectChange);
            this.UpdateProperties();
        }

        /// <summary>
        /// Updates the properties of this row
        /// </summary>
        private void UpdateProperties()
        {
            this.ModifiedOn = this.Thing.ModifiedOn;

            if (this.Thing.ParameterType != null)
            {
                this.ParameterTypeShortName = this.Thing.ParameterType.ShortName;
                this.ParameterTypeName = this.Thing.ParameterType.Name;
            }

            this.ParameterType = this.Thing.ParameterType;

            if (this.Thing.MeasurementScale != null)
            {
                this.ScaleShortName = this.Thing.MeasurementScale.ShortName;
                this.ScaleName = this.Thing.MeasurementScale.Name;
            }

            this.Scale = this.Thing.MeasurementScale;
        }

        /// <summary>
        /// Apply selected filter to PossibleParameterType
        /// </summary>
        private void FilterPossibleParameterType()
        {
            if (!string.IsNullOrEmpty(this.SelectedFilter))
            {
                this.ParameterType = null;

                this.PossibleParameterType.Clear();
                var filteredList = this.possibleParameterTypes.Where(x => x.GetType() == this.FilteringOptions[this.SelectedFilter]);
                this.PossibleParameterType.AddRange(filteredList);
            }
            else
            {
                this.PossibleParameterType.Clear();
                this.PossibleParameterType.AddRange(this.possibleParameterTypes);
            }
        }

        /// <summary>
        /// Populate the possible scale for this <see cref="ParameterTypeComponent" />
        /// </summary>
        private void PopulatePossibleScale()
        {
            this.PossibleScale.Clear();

            var qt = this.ParameterType as QuantityKind;

            if (qt == null)
            {
                this.Scale = null;
                return;
            }

            this.PossibleScale.AddRange(qt.AllPossibleScale);

            if (this.PossibleScale.Count > 0)
            {
                this.Scale = qt.DefaultScale ?? this.PossibleScale.First();
            }
        }
    }
}
