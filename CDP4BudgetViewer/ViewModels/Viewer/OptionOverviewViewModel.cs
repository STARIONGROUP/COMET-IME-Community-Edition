// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionOverviewViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Budget.ViewModels
{
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;

    using CDP4Composition.Mvvm;

    using ReactiveUI;
    using Services;

    /// <summary>
    /// The view-model for an option overview of the budget
    /// </summary>
    public class OptionOverviewViewModel : ReactiveObject
    {
        /// <summary>
        /// The option-name column name
        /// </summary>
        private const string OPTION_NAME_FIELDNAME = "OptionName";

        /// <summary>
        /// The option-name column name
        /// </summary>
        private const string OPTION_NAME_HEADER = "Option Name";

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionOverviewViewModel"/> class
        /// </summary>
        public OptionOverviewViewModel()
        {
            this.Records = new ReactiveList<ExpandoObject>();
            this.Columns = new ReactiveList<ColumnDefinition>();
        }

        /// <summary>
        /// Gets the element-definition overview
        /// </summary>
        public ReactiveList<ExpandoObject> Records { get; private set; }

        /// <summary>
        /// Gets the <see cref="Option"/> name
        /// </summary>
        public ReactiveList<ColumnDefinition> Columns { get; private set; }

        /// <summary>
        /// Clear the view
        /// </summary>
        public void ClearView()
        {
            this.Columns.Clear();
            this.Records.Clear();
        }

        /// <summary>
        /// Generates the columns of the view
        /// </summary>
        /// <param name="elements">The <see cref="ElementDefinition"/></param>
        public void GenerateColumn(IEnumerable<ElementDefinition> elements)
        {
            this.Columns.Add(new ColumnDefinition(OPTION_NAME_HEADER, OPTION_NAME_FIELDNAME));
            this.Columns.AddRange(elements.Select(x => new ColumnDefinition(x.Name, x.ShortName)));
        }

        /// <summary>
        /// Add a row with all totals of an <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="option">The <see cref="ElementDefinition"/></param>
        /// <param name="elementTotals">The total value given an option for all computed <see cref="ElementDefinition"/></param>
        public void AddRecord(Option option, IReadOnlyDictionary<ElementDefinition, float> elementTotals)
        {
            var record = new ExpandoObject();
            var dic = (IDictionary<string, object>)record;
            dic.Add(OPTION_NAME_FIELDNAME, option.Name);
            foreach (var keyValuePair in elementTotals)
            {
                dic.Add(keyValuePair.Key.ShortName, keyValuePair.Value);
            }

            this.Records.Add((ExpandoObject)dic);
        }
    }
}
