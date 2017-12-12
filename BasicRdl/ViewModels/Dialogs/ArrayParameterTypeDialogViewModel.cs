// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ArrayParameterTypeDialogViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2016 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Operations;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="ArrayParameterTypeDialogViewModel"/> is to provide a dialog view model
    /// for a <see cref="ArrayParameterType"/>
    /// </summary>
    [ThingDialogViewModelExport(ClassKind.ArrayParameterType)]
    public class ArrayParameterTypeDialogViewModel : CompoundParameterTypeDialogViewModel
    {
        #region Fields

        /// <summary>
        /// Backing field for <see cref="IsTensor"/>
        /// </summary>
        private bool isTensor;

        /// <summary>
        /// Backing field for <see cref="DimensionString"/>
        /// </summary>
        private string dimensionString;

        /// <summary>
        /// The number of components for this array parameter type
        /// </summary>
        private int numberOfComponents;

        /// <summary>
        /// The initial dimensions of this array parameter type
        /// </summary>
        private ValueArray<int> initialDimension;

        /// <summary>
        /// The current dimensions defined by <see cref="DimensionString"/>
        /// </summary>
        private ValueArray<int> currentDimension;

        /// <summary>
        /// The array of denominators used to compute the indexes of a component
        /// </summary>
        private int[] indexDenominator;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayParameterTypeDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public ArrayParameterTypeDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayParameterTypeDialogViewModel"/> class.
        /// </summary>
        /// <param name="arrayParameterType">The <see cref="ArrayParameterType"/></param>
        /// <param name="transaction">The <see cref="ThingTransaction"/> that contains the log of recorded changes</param>
        /// <param name="session">The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated</param>
        /// <param name="isRoot">Assert if this <see cref="BooleanParameterTypeDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/></param>
        /// <param name="dialogKind">The kind of operation this <see cref="BooleanParameterTypeDialogViewModel"/> performs</param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/> that is used to navigate to a dialog of a specific <see cref="Thing"/></param>
        /// <param name="container">The <see cref="Thing"/> that contains the created <see cref="Thing"/> in this Dialog</param>
        /// <param name="chainOfContainers">The optional chain of containers that contains the <paramref name="container"/> argument</param>
        /// <exception cref="ArgumentException">
        /// The container must be of type <see cref="ReferenceDataLibrary"/>.
        /// </exception>
        public ArrayParameterTypeDialogViewModel(ArrayParameterType arrayParameterType, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(arrayParameterType, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the IsTensor
        /// </summary>
        public bool IsTensor
        {
            get { return this.isTensor; }
            set { this.RaiseAndSetIfChanged(ref this.isTensor, value); }
        }

        /// <summary>
        /// Gets or sets the dimension
        /// </summary>
        public string DimensionString
        {
            get { return this.dimensionString; }
            set { this.RaiseAndSetIfChanged(ref this.dimensionString, value); }
        }

        /// <summary>
        /// Gets or sets the number of components
        /// </summary>
        public int NumberOfComponents
        {
            get { return this.numberOfComponents; }
            private set { this.RaiseAndSetIfChanged(ref this.numberOfComponents, value); }
        }

        #endregion

        #region Dialog Base
        
        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        /// <remarks>
        /// Used by the view through the IDataErrorInfo interface to validate a field
        /// </remarks>
        public override string this[string columnName]
        {
            get
            {
                if (columnName == "DimensionString")
                {
                    string error;

                    var number = 0;

                    if (this.DimensionString.TryParseToIntValueArray(out this.currentDimension, out error))
                    {
                        number = 1;
                        foreach (var dimension in this.currentDimension)
                        {
                            if (dimension > 0)
                            {
                                number *= dimension;
                            }
                            else
                            {
                                number = 0;
                                error = "The array dimesions must be positive.";
                                break;
                            }
                        }
                    }

                    this.NumberOfComponents = number;
                    return error;
                }

                return base[columnName];
            }
        }

        /// <summary>
        /// Initialize the dialog
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            var arrayPt = (ArrayParameterType)this.Thing;
            this.initialDimension = new ValueArray<int>(arrayPt.Dimension);
            // set defaut dimension to 1
            this.DimensionString = (this.initialDimension.Any()) ? this.initialDimension.ToString() : "{1;1;1}";
            var error = this["DimensionString"];
        }

        /// <summary>
        /// Initializes the commands and listeners
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            this.WhenAnyValue(x => x.NumberOfComponents).Subscribe(_ => this.PopulateComponent());
        }

        /// <summary>
        /// Update the properties of this dialog
        /// </summary>
        protected override void UpdateProperties()
        {
            base.UpdateProperties();
            var arrayPt = (ArrayParameterType)this.Thing;
            this.IsTensor = arrayPt.IsTensor;
        }

        /// <summary>
        /// Populate the components
        /// </summary>
        protected override void PopulateComponent()
        {
            this.Component.Clear();

            if (this.NumberOfComponents != 0)
            {
                this.ComputeComponentCoordinateDenominator();
            }

            for (var i = 0; i < this.NumberOfComponents; i++)
            {
                ParameterTypeComponentRowViewModel row;
                var coordinates = this.ComputeComponentCoordinates(i + 1);

                if (this.Thing.Component.Count > i)
                {
                    var component = this.Thing.Component[i];
                    row = new ParameterTypeComponentRowViewModel(component, this.Session, this);
                }
                else
                {
                    // create new ParameterTypeComponent
                    var component = new ParameterTypeComponent();
                    row = new ParameterTypeComponentRowViewModel(component, this.Session, this);
                    row.ShortName = coordinates;
                }

                row.Coordinates = coordinates;
                this.Component.Add(row);
            }
        }

        /// <summary>
        /// Update the transaction
        /// </summary>
        protected override void UpdateTransaction()
        {
            base.UpdateTransaction();
            var clone = ((ArrayParameterType) this.Thing);

            clone.IsTensor = this.IsTensor;

            if (this.initialDimension.SequenceEqual(this.currentDimension))
            {
                return;
            }

            clone.Dimension = new OrderedItemList<int>(clone);
            clone.Dimension.AddRange(this.currentDimension);
        }

        #endregion

        #region Compute Coordinate methods
        /// <summary>
        /// Compute the coordinates of a component from its index in the array
        /// </summary>
        /// <param name="n">the flatten index of the component</param>
        /// <returns>A string containing the coordinates</returns>
        /// <remarks>
        /// Assuming N components in the dimension {d_1, d_2, ... , d_dmax},
        /// the coordinates c_i i=1..dmax of the component of index n=1..N are given by the following formula:
        /// c_i = (n - offset) / denominator,
        /// where offset = Sum(d_j*(c_j - 1)), j=1..i
        ///       denominator = Product(d_j), j=i+1...dmax
        /// </remarks>
        private string ComputeComponentCoordinates(int n)
        {
            var rank = this.currentDimension.Count();
            var indexList = new int[rank];

            for (var i = 0; i < rank; i++)
            {
                var offset = this.ComputeOffset(indexList, i);
                var ni = n - offset;

                indexList[i] = (int)Math.Ceiling(ni/(double)this.indexDenominator[i]);
            }

            return "{" + string.Join(";", indexList.Select(x => x.ToString(CultureInfo.InvariantCulture))) + "}";
        }

        /// <summary>
        /// Compute the denominators used to compute the coordinates of the components on an axis
        /// </summary>
        private void ComputeComponentCoordinateDenominator()
        {
            var rank = this.currentDimension.Count();
            this.indexDenominator = new int[rank];
            var reversedDimension = this.currentDimension.ToList();
            reversedDimension.Reverse();
            for (var i = 0; i < rank - 1; i++)
            {
                this.indexDenominator[i] = reversedDimension.GetRange(0, rank - 1 - i).Aggregate((a, b) => a * b);
            }

            this.indexDenominator[rank - 1] = 1;
        }

        /// <summary>
        /// Compute the offset to compute the coordinate of a component on a specific axis
        /// </summary>
        /// <param name="coordinates">The coordinates containing the values for the previous axes</param>
        /// <param name="axisNumber">The axis for which the offset is computed</param>
        /// <returns>The offset</returns>
        private int ComputeOffset(IEnumerable<int> coordinates, int axisNumber)
        {
            if (axisNumber == 0)
            {
                return 0;
            }

            var computesIndexes = coordinates.ToList();
            var offsetSum = 0;
            for (var j = 0; j < axisNumber; j++)
            {
                offsetSum += (computesIndexes[j] - 1) * this.indexDenominator[j];
            }

            return offsetSum;
        }
        #endregion
    }
}