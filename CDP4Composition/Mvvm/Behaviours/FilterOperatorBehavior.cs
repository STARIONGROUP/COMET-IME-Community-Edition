// -------------------------------------------------------------------------------------------------
// <copyright file="FilterOperatorBehavior.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smieckowski
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
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm.Behaviours
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    using CDP4Common.CommonData;

    using CDP4Composition.FilterOperators;

    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.Data;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// <see cref="Behavior{T}"/> for a <see cref="GridColumnBase"/> to be used to define FilterOperator behavior for that specific <see cref="GridColumnBase"/>.
    /// </summary>
    public class FilterOperatorBehavior : Behavior<GridColumnBase>
    {
        /// <summary>
        /// The <see cref="DependencyProperty"/> for the <see cref="ItemsSource"/>
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable<IRowViewModelBase<Thing>>),
                typeof(FilterOperatorBehavior),
                new PropertyMetadata(OnItemsSourceChanged));

        /// <summary>
        /// The <see cref="DependencyProperty"/> for the <see cref="CustomFilterOperatorType"/>
        /// </summary>
        public static readonly DependencyProperty CustomFilterOperatorTypeProperty =
            DependencyProperty.Register(
                "CustomFilterOperatorType",
                typeof(CustomFilterOperatorType),
                typeof(FilterOperatorBehavior),
                new PropertyMetadata());

        /// <summary>
        /// Gets the current <see cref="GridColumnBase"/>
        /// </summary>
        private GridColumnBase Column => this.AssociatedObject;

        private IHaveCustomFilterOperators customQueryFilterOperatorsViewModel;

        /// <summary>
        /// Gets or sets the <see cref="CustomFilterOperatorType"/>
        /// </summary>
        public CustomFilterOperatorType CustomFilterOperatorType
        {
            get => (CustomFilterOperatorType)this.GetValue(CustomFilterOperatorTypeProperty);
            set => this.SetValue(CustomFilterOperatorTypeProperty, value);
        }

        /// <summary>
        /// Gets or sets the <see cref="ItemsSource"/> which should be an <see cref="IEnumerable{T}"/> of type <see cref="IRowViewModelBase{Thing}"/>
        /// </summary>
        public IEnumerable<IRowViewModelBase<Thing>> ItemsSource
        {
            get => (IEnumerable<IRowViewModelBase<Thing>>)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Occurs when the <see cref="ItemsSourceProperty"/> <see cref="DependencyObject"/> changes.
        /// </summary>
        /// <param name="d">The <see cref="DependencyObject"/> </param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FilterOperatorBehavior filterOperatorBehavior)
            {
                var dataViewBase = filterOperatorBehavior.Column?.View;
                var fieldName = filterOperatorBehavior.Column?.FieldName;

                if (dataViewBase == null
                    || string.IsNullOrWhiteSpace(fieldName)
                    || filterOperatorBehavior.customQueryFilterOperatorsViewModel == null)
                {
                    return;
                }

                if (!filterOperatorBehavior.customQueryFilterOperatorsViewModel.CustomFilterOperators.ContainsKey(dataViewBase))
                {
                    filterOperatorBehavior.customQueryFilterOperatorsViewModel.CustomFilterOperators.Add(dataViewBase, new Dictionary<string, (CustomFilterOperatorType, IEnumerable<IRowViewModelBase<Thing>>)>());
                }

                var filterOperatorsPerFieldName =
                    filterOperatorBehavior.customQueryFilterOperatorsViewModel.CustomFilterOperators[dataViewBase];

                var newValue = (filterOperatorBehavior.CustomFilterOperatorType, filterOperatorBehavior.ItemsSource);

                if (filterOperatorsPerFieldName.ContainsKey(fieldName))
                {
                    filterOperatorsPerFieldName[fieldName] = newValue;
                }
                else
                {
                    filterOperatorsPerFieldName.Add(fieldName, newValue);
                }
            }
        }

        /// <summary>
        /// Executes when this behavior is detached from its <see cref="GridColumnBase"/>
        /// </summary>
        protected override void OnDetaching()
        {
            this.Column.View.DataContextChanged -= this.View_DataContextChanged;
            this.Column.View.DataControl.CustomUniqueValues -= this.DataControl_CustomUniqueValues;
            base.OnDetaching();
        }

        /// <summary>
        /// Executes when this behavior is attached to its <see cref="GridColumnBase"/>
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.Column.View.DataContextChanged += this.View_DataContextChanged;
            this.Column.View.DataControl.CustomUniqueValues += this.DataControl_CustomUniqueValues;
        }

        /// <summary>
        /// Handles the <see cref="DataControlBase.CustomUniqueValues"/> event
        /// </summary>
        /// <param name="sender">The <see cref="DataControlBase"/></param>
        /// <param name="e">The <see cref="CustomUniqueValuesEventArgs"/></param>
        private void DataControl_CustomUniqueValues(object sender, CustomUniqueValuesEventArgs e)
        {
            if (e.Column != this.Column)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(this.Column?.FieldName))
            {
                var filterOperatorHandler = CustomFilterOperatorHandlerFactory.CreateFilterOperatorHandler(this.CustomFilterOperatorType, this.ItemsSource, this.Column.FieldName);
                var values = filterOperatorHandler.GetValues().ToArray();

                e.UniqueValues = values.Distinct().ToArray();

                e.UniqueValuesAndCounts = values.GroupBy(x => x)
                    .Select(x => new ValueAndCount(x.Key, x.Count()))
                    .ToArray();

                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the <see cref="DataViewBase.DataContextChanged"/> event
        /// </summary>
        /// <param name="sender">The <see cref="DataViewBase"/></param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        private void View_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.customQueryFilterOperatorsViewModel = e.NewValue as IHaveCustomFilterOperators;
        }
    }
}
