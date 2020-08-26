// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValueSetTemplates.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4CommonView.Resources
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using CDP4Common.CommonData;
    using CDP4Composition.ViewModels;
    using DevExpress.Xpf.Editors;
    using DevExpress.Xpf.Grid;

    /// <summary>
    /// Interaction logic for ValueSetTemplates.xaml
    /// </summary>
    public partial class ValueSetTemplates : ResourceDictionary
    {
        public ValueSetTemplates()
        {
            EditorLocalizer.Active = new Cdp4EditorLocalizer();
        }
    }

    public class Cdp4EditorLocalizer : EditorLocalizer
    {
        protected override void PopulateStringTable()
        {
            base.PopulateStringTable();
            this.AddString(EditorStringId.EmptyItem, "-");
        }
    }

    /// <summary>
    /// The <see cref="DataTemplateSelector"/> for the manual cell
    /// </summary>
    public class ValueCellTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// returns a System.Windows.DataTemplate based on custom logic.
        /// </summary>
        /// <param name="item">
        /// The data object for which to select the template.
        /// </param>
        /// <param name="container">
        ///  The data-bound object.
        /// </param>
        /// <returns>
        /// Returns a System.Windows.DataTemplate or null. The default value is null.
        /// </returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var cellData = item as GridCellData;
            if (cellData == null)
            {
                return this.InactiveTemplate;
            }

            var rowObject = cellData.RowData.Row as IValueSetRow;
            if (rowObject == null)
            {
                return this.InactiveTemplate;
            }

            var parameterTypeKind = rowObject.ParameterTypeClassKind;
            switch (parameterTypeKind)
            {
                case ClassKind.TextParameterType:
                case ClassKind.DerivedQuantityKind:
                case ClassKind.SimpleQuantityKind:
                case ClassKind.SpecializedQuantityKind:
                    return this.TextEditTemplate;
                case ClassKind.EnumerationParameterType:
                    return rowObject.IsMultiSelect ? this.MultiEnumTemplate : this.SingleEnumTemplate;
                case ClassKind.BooleanParameterType:
                    return this.BooleanTemplate;
                case ClassKind.DateTimeParameterType:
                    return this.DateTimeTemplate;
                case ClassKind.DateParameterType:
                    return this.DateTemplate;
                case ClassKind.TimeOfDayParameterType:
                    return this.TimeOfDayTemplate;
                default:
                    return this.TextEditTemplate;
            }
        }

        /// <summary>
        /// Gets or set an inactive template
        /// </summary>
        public DataTemplate InactiveTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for text item
        /// </summary>
        public DataTemplate TextEditTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for boolean values
        /// </summary>
        public DataTemplate BooleanTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for multi enumeration
        /// </summary>
        public DataTemplate MultiEnumTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> for single enumeration
        /// </summary>
        public DataTemplate SingleEnumTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to edit a <see cref="DateTime"/>
        /// </summary>
        public DataTemplate DateTimeTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to edit a date
        /// </summary>
        public DataTemplate DateTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> to edit a time
        /// </summary>
        public DataTemplate TimeOfDayTemplate { get; set; }
    }
}