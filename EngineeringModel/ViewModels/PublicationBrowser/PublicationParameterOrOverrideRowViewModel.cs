// -------------------------------------------------------------------------------------------------
// <copyright file="PublicationParameterOrOverrideRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System;
    using System.Globalization;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Mvvm;
    using CDP4Dal;
    using PublicationBrowser;
    using ReactiveUI;

    /// <summary>
    /// The row representing a <see cref="ParameterOrOverrideBase"/> in the <see cref="PublicationBrowserViewModel"/>
    /// </summary>
    public class PublicationParameterOrOverrideRowViewModel : ParameterOrOverrideBaseRowViewModel, IPublishableRow
    {
        /// <summary>
        /// Backing field for the <see cref="ToBePublished"/> property.
        /// </summary>
        private bool toBePublished;

        /// <summary>
        /// Backing field for <see cref="PercentageChange"/>
        /// </summary>
        private string percentageChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublicationParameterOrOverrideRowViewModel"/> class
        /// </summary>
        /// <param name="parameterOrOverrideBase">
        /// The associated <see cref="ParameterOrOverrideBase"/>
        /// </param>
        /// <param name="session">
        /// The associated <see cref="ISession"/>
        /// </param>
        /// <param name="containerViewModel">
        /// The container Row.
        /// </param>
        public PublicationParameterOrOverrideRowViewModel(ParameterOrOverrideBase parameterOrOverrideBase, ISession session, IRowViewModelBase<Thing> containerViewModel)
            : base(parameterOrOverrideBase, session, containerViewModel, false)
        {
            this.WhenAnyValue(vm => vm.ToBePublished).Subscribe(_ => this.ToBePublishedChanged());
            this.IsCheckable = true;
            this.SetProperties();
        }

        /// <summary>
        /// Execute change of status on whether the row is to be published.
        /// </summary>
        private void ToBePublishedChanged()
        {
            this.Thing.ToBePublished = this.ToBePublished;
        }

        /// <summary>
        /// Sets the row values.
        /// </summary>
        public override void SetProperties()
        {
            base.SetProperties();
            this.ToBePublished = this.Thing.ToBePublished;

            // Set percentage change
            if (!(this.Thing.ParameterType is QuantityKind))
            {
                 this.PercentageChange = "-";
                return;
            }

            double oldValue, newValue;

            var newParsed = double.TryParse(this.Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out newValue);
            var oldParsed = double.TryParse(this.Published, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out oldValue);

            if (!oldParsed || !newParsed)
            {
                this.PercentageChange = "-";
                return;
            }

            var percentageChage = (newValue - oldValue) / oldValue;
            this.PercentageChange = string.Format(CultureInfo.InvariantCulture, "{0:0.0%}", percentageChage);
        }

        /// <summary>
        /// Gets the Model Code
        /// </summary>
        public string ModelCode
        {
            get
            {
                return this.Thing is Parameter
                    ? ((Parameter)this.Thing).ModelCode()
                    : ((ParameterOverride)this.Thing).ModelCode();
            }
        }

        /// <summary>
        /// Gets the percentage change representation of the difference between
        /// <see cref="Value"/> and <see cref="Published"/> value.
        /// </summary>
        public string PercentageChange
        {
            get { return this.percentageChange; }
            private set { this.RaiseAndSetIfChanged(ref this.percentageChange, value); }
        }

        /// <summary>
        /// Gets or sets whether this row is checkable.
        /// </summary>
        public bool IsCheckable { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ParameterOrOverrideBase"/> is to be published in the next publication.
        /// </summary>
        public bool ToBePublished
        {
            get
            {
                return this.toBePublished;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.toBePublished, value);
            }
        }
    }
}
