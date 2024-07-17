// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParameterTypeMappingDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Windows.Input;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;

    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;
    
    using ReqIFSharp;

    [DialogViewModelExport("ParameterTypeMappingDialogViewModel", "The dialog used to map the Reqif DatatypeDefinition to ParameterType.")]
    public class ParameterTypeMappingDialogViewModel : ReqIfMappingDialogViewModelBase
    {
        /// <summary>
        /// The result of the parameter-type mapping
        /// </summary>
        private Dictionary<DatatypeDefinition, DatatypeDefinitionMap> map;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeMappingDialogViewModel"/> class.
        /// Used by MEF.
        /// </summary>
        public ParameterTypeMappingDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterTypeMappingDialogViewModel"/> class.
        /// </summary>
        public ParameterTypeMappingDialogViewModel(string lang, IEnumerable<DatatypeDefinition> datatypeDefinitions, IDictionary<DatatypeDefinition, DatatypeDefinitionMap> map, Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService)
            : base(iteration, session, thingDialogNavigationService, lang)
        {
            this.NextCommand = ReactiveCommandCreator.Create(this.ExecuteNextCommand);

            this.CreateParameterTypeCommands = new ReactiveList<ContextMenuItemViewModel>();
            this.ParameterTypes = new ReactiveList<ParameterType>();
            this.PopulateParameterTypes();

            this.PopulateCreateParameterTypeCommands();
            this.PopulateCreateMeasurementScaleCommands();

            this.WhenAnyValue(x => x.SelectedRow).Subscribe(_ => this.PopulateCreateParameterTypeCommands());

            this.MappingRows = new ReactiveList<DatatypeDefinitionMappingRowViewModel>();

            this.PopulateRows(datatypeDefinitions);

            // set current parameter type mapping if any
            if (map != null)
            {
                this.SetCurrentMapping(map);
            }
        }

        /// <summary>
        /// Populates the <see cref="MappingRows"/>
        /// </summary>
        /// <param name="datatypeDefinitions"></param>
        private void PopulateRows(IEnumerable<DatatypeDefinition> datatypeDefinitions)
        {
            foreach (var datatypeDefinition in datatypeDefinitions)
            {
                var row = new DatatypeDefinitionMappingRowViewModel(datatypeDefinition, this.ParameterTypes);
                this.MappingRows.Add(row);
            }
        }

        /// <summary>
        /// Sets the current mapping
        /// </summary>
        /// <param name="map">The mapping configuration</param>
        private void SetCurrentMapping(IDictionary<DatatypeDefinition, DatatypeDefinitionMap> map)
        {
            foreach (var pair in map)
            {
                var row = this.MappingRows.SingleOrDefault(x => x.Identifiable.Identifier == pair.Key.Identifier);

                if (row is null)
                {
                    continue;
                }

                row.MappedThing = pair.Value.ParameterType;

                // set current enumvalue mapping
                if (!(row.Identifiable is DatatypeDefinitionEnumeration))
                {
                    continue;
                }

                this.PopulateEnumRow(pair.Value, row);
            }
        }

        /// <summary>
        /// Sets the Mapped Thing on enum values of the provided row
        /// </summary>
        /// <param name="dataTypeDefinitionMap"></param>
        /// <param name="row"></param>
        private void PopulateEnumRow(DatatypeDefinitionMap dataTypeDefinitionMap, DatatypeDefinitionMappingRowViewModel row)
        {
            foreach (var enumRow in row.EnumValue)
            {
                if (dataTypeDefinitionMap.EnumValueMap.TryGetValue(enumRow.Identifiable, out var enumValueDef))
                {
                    enumRow.MappedThing = enumRow.PossibleThings.Single(x => x.Iid == enumValueDef.Iid);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/>s to create a <see cref="ParameterType"/>
        /// </summary>
        public ReactiveList<ContextMenuItemViewModel> CreateParameterTypeCommands { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/>s to create a <see cref="MeasurementScale"/>
        /// </summary>
        public ReactiveList<ContextMenuItemViewModel> CreateMeasurementScaleCommands { get; private set; }

        /// <summary>
        /// Gets the <see cref="ParameterType"/> available for mapping
        /// </summary>
        public ReactiveList<ParameterType> ParameterTypes { get; private set; }

        /// <summary>
        /// Gets the mapping rows
        /// </summary>
        public ReactiveList<DatatypeDefinitionMappingRowViewModel> MappingRows { get; private set; } 

        /// <summary>
        /// Gets the "next" <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<Unit, Unit> NextCommand { get; private set; }

        /// <summary>
        /// Gets the Cast selected row
        /// </summary>
        public DatatypeDefinitionMappingRowViewModel CastSelectedRow
        {
            get { return (DatatypeDefinitionMappingRowViewModel)this.SelectedRow; }
        }

        /// <summary>
        /// Executes the <see cref="NextCommand"/>
        /// </summary>
        private void ExecuteNextCommand()
        {
            this.PopulateMap();
            this.DialogResult = new ParameterTypeMappingDialogResult(this.map, true);
        }

        /// <summary>
        /// Populate the possible <see cref="ParameterType"/>s
        /// </summary>
        private void PopulateParameterTypes()
        {
            var model = (EngineeringModel)this.IterationClone.Container;
            var rdls = new List<ReferenceDataLibrary>();

            var requiredRdl = model.EngineeringModelSetup.RequiredRdl.Single();
            rdls.Add(requiredRdl);
            rdls.AddRange(requiredRdl.GetRequiredRdls());

            this.ParameterTypes.Clear();
            this.ParameterTypes.AddRange(rdls.SelectMany(x => x.ParameterType).OrderBy(x => x.Name));
        }

        /// <summary>
        /// Populate the result of the mapping from the row data
        /// </summary>
        private void PopulateMap()
        {
            this.map = new Dictionary<DatatypeDefinition, DatatypeDefinitionMap>();
            foreach (var row in this.MappingRows)
            {
                var enumMap = new Dictionary<EnumValue, EnumerationValueDefinition>();
                foreach (var enumValueRow in row.EnumValue)
                {
                    enumMap.Add(enumValueRow.Identifiable, enumValueRow.MappedThing);
                }

                var defMap = new DatatypeDefinitionMap(row.Identifiable, row.MappedThing, enumMap);
                this.map.Add(row.Identifiable, defMap);
            }
        }

        /// <summary>
        /// Populate the Create <see cref="ParameterType"/> <see cref="ICommand"/>s
        /// </summary>
        private void PopulateCreateParameterTypeCommands()
        {
            this.CreateParameterTypeCommands.Clear();

            if (this.SelectedRow == null)
            {
                return;
            }

            if (this.SelectedRow.Identifiable is DatatypeDefinitionEnumeration)
            {
                var enumPtCommand = ReactiveCommandCreator.Create(this.ExecuteCreateGenericParameterTypeCommand<EnumerationParameterType>);
                this.CreateParameterTypeCommands.Add(new ContextMenuItemViewModel("Create an Enumeration Parameter Type", "", enumPtCommand));
                return;
            }

            if (this.SelectedRow.Identifiable is DatatypeDefinitionDate)
            {
                var datePtCommand = ReactiveCommandCreator.Create(this.ExecuteCreateGenericParameterTypeCommand<DateParameterType>);
                this.CreateParameterTypeCommands.Add(new ContextMenuItemViewModel("Create a Date Parameter Type", "", datePtCommand));

                var dateTimePtCommand = ReactiveCommandCreator.Create(this.ExecuteCreateGenericParameterTypeCommand<DateTimeParameterType>);
                this.CreateParameterTypeCommands.Add(new ContextMenuItemViewModel("Create a Date-Time Parameter Type", "", dateTimePtCommand));

                var timeOfDayPtCommand = ReactiveCommandCreator.Create(this.ExecuteCreateGenericParameterTypeCommand<TimeOfDayParameterType>);
                this.CreateParameterTypeCommands.Add(new ContextMenuItemViewModel("Create a Time of Day Parameter Type", "", timeOfDayPtCommand));
                return;
            }

            if (this.SelectedRow.Identifiable is DatatypeDefinitionBoolean)
            {
                var booleanPtCommand = ReactiveCommandCreator.Create(this.ExecuteCreateGenericParameterTypeCommand<BooleanParameterType>);
                this.CreateParameterTypeCommands.Add(new ContextMenuItemViewModel("Create a Boolean Parameter Type", "", booleanPtCommand));
                return;
            }

            var textPtCommand = ReactiveCommandCreator.Create(this.ExecuteCreateGenericParameterTypeCommand<TextParameterType>);
            this.CreateParameterTypeCommands.Add(new ContextMenuItemViewModel("Create a Text Parameter Type", "", textPtCommand));

            var simpleQkCommand = ReactiveCommandCreator.Create(this.ExecuteCreateGenericParameterTypeCommand<SimpleQuantityKind>);
            this.CreateParameterTypeCommands.Add(new ContextMenuItemViewModel("Create a Simple Quantity Kind", "", simpleQkCommand));

            var derivedQtCommand = ReactiveCommandCreator.Create(this.ExecuteCreateGenericParameterTypeCommand<DerivedQuantityKind>);
            this.CreateParameterTypeCommands.Add(new ContextMenuItemViewModel("Create a Derived Quantity Kind", "", derivedQtCommand));

            var specializedQtCommand = ReactiveCommandCreator.Create(this.ExecuteCreateGenericParameterTypeCommand<SpecializedQuantityKind>);
            this.CreateParameterTypeCommands.Add(new ContextMenuItemViewModel("Create a Specialized Quantity Kind", "", specializedQtCommand));
        }

        /// <summary>
        /// Populate the Create <see cref="MeasurementScale"/> <see cref="ICommand"/>s
        /// </summary>
        private void PopulateCreateMeasurementScaleCommands()
        {
            this.CreateMeasurementScaleCommands = new ReactiveList<ContextMenuItemViewModel>();
            var ratioScaleCommand = ReactiveCommandCreator.Create(this.ExecuteCreateScaleCommand<RatioScale>);
            this.CreateMeasurementScaleCommands.Add(new ContextMenuItemViewModel("Create a Ratio Scale", "", ratioScaleCommand));

            var cyclicRatioScaleCommand = ReactiveCommandCreator.Create(this.ExecuteCreateScaleCommand<CyclicRatioScale>);
            this.CreateMeasurementScaleCommands.Add(new ContextMenuItemViewModel("Create a Cyclic-Ratio Scale", "", cyclicRatioScaleCommand));

            var ordinalScaleCommand = ReactiveCommandCreator.Create(this.ExecuteCreateScaleCommand<OrdinalScale>);
            this.CreateMeasurementScaleCommands.Add(new ContextMenuItemViewModel("Create an Ordinal Scale", "", ordinalScaleCommand));

            var logarithmicScaleCommand = ReactiveCommandCreator.Create(this.ExecuteCreateScaleCommand<LogarithmicScale>);
            this.CreateMeasurementScaleCommands.Add(new ContextMenuItemViewModel("Create a Logarithmic Scale", "", logarithmicScaleCommand));

            var intervalScaleCommand = ReactiveCommandCreator.Create(this.ExecuteCreateScaleCommand<IntervalScale>);
            this.CreateMeasurementScaleCommands.Add(new ContextMenuItemViewModel("Create a Interval Scale", "", intervalScaleCommand));
        }

        /// <summary>
        /// Execute the <see cref="ICommand"/> to create a <see cref="ParameterType"/>
        /// </summary>
        /// <typeparam name="TParameterType">A kind of <see cref="ParameterType"/></typeparam>
        /// <remarks>
        /// A new transaction is created to allow a <see cref="ParameterType"/> to be created "on the fly"
        /// </remarks>
        private void ExecuteCreateGenericParameterTypeCommand<TParameterType>() where TParameterType : ParameterType, new()
        {
            ParameterType parameterType;

            parameterType = new TParameterType();
            parameterType.Name = this.SelectedRow.Identifiable.LongName;
            
            var enumDatatype = this.SelectedRow.Identifiable as DatatypeDefinitionEnumeration;
            if (enumDatatype != null)
            {
                var enumParameterType = new EnumerationParameterType { Name = enumDatatype.LongName };
                foreach (var specifiedValue in enumDatatype.SpecifiedValues.OrderBy(x => x.Properties.Key))
                {
                    var enumerationDefinition = new EnumerationValueDefinition
                    {
                        Name = specifiedValue.LongName,
                        ShortName = specifiedValue.Properties.OtherContent
                    };

                    enumParameterType.ValueDefinition.Add(enumerationDefinition);
                }

                parameterType = enumParameterType;
            }

            var siteDirectory = this.Session.RetrieveSiteDirectory();
            var transactionContext = TransactionContextResolver.ResolveContext(siteDirectory);
            var parameterTypeTransaction = new ThingTransaction(transactionContext);

            this.AddContainedThingToTransaction(parameterTypeTransaction, parameterType);
            
            var result = this.ThingDialogNavigationService.Navigate(parameterType, parameterTypeTransaction, this.Session, true, ThingDialogKind.Create, this.ThingDialogNavigationService);

            if (!result.HasValue || !result.Value)
            {
                return;
            }
            
            // refresh parameter type list and set the mapping to the new parameter type
            this.PopulateParameterTypes();
            ((DatatypeDefinitionMappingRowViewModel)this.SelectedRow).FilterPossibleParameterTypes();
            this.CastSelectedRow.MappedThing = this.ParameterTypes.Single(x => x.Iid == parameterType.Iid);
        }

        /// <summary>
        /// Execute the <see cref="ICommand"/> to create a <see cref="MeasurementScale"/>
        /// </summary>
        /// <typeparam name="TScale">A kinf of <see cref="MeasurementScale"/></typeparam>
        /// <remarks>
        /// A new transaction is created to allow a <see cref="MeasurementScale"/> to be created "on the fly"
        /// </remarks>
        private void ExecuteCreateScaleCommand<TScale>() where TScale : MeasurementScale, new()
        {
            var scale = new TScale();

            var siteDirectory = this.Session.RetrieveSiteDirectory();
            var transactionContext = TransactionContextResolver.ResolveContext(siteDirectory);
            var scaleTransaction = new ThingTransaction(transactionContext);

            this.ThingDialogNavigationService.Navigate(scale, scaleTransaction, this.Session, true, ThingDialogKind.Create, this.ThingDialogNavigationService);
        }

        /// <summary>
        /// Executes the cancel <see cref="ICommand"/>
        /// </summary>
        protected override void ExecuteCancelCommand()
        {
            this.DialogResult = new ParameterTypeMappingDialogResult(null, false);
        }
    }
}