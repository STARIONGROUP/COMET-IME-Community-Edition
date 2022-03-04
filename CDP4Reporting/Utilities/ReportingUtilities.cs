// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessedValueSetGenerator.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Cozmin Velciu, Adrian Chivu
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

namespace CDP4Reporting.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;

    using CDP4Common.EngineeringModelData;

    using CDP4Dal;

    using CDP4Reporting.Events;
    using CDP4Reporting.ViewModels;

    using DevExpress.Xpf.Core;

    /// <summary>
    /// Static class that contains individual utility methods that can be used in reports
    /// </summary>
    public static class ReportingUtilities
    {
        /// <summary>
        /// Gets a path and converts it to be usefull for the applicable <see cref="Option"/>.
        /// </summary>
        /// <param name="path">The <see cref="NestedParameter.Path"/></param>
        /// <param name="option">The <see cref="Option"/> for which to convert the <paramref name="path"/> param to.</param>
        /// <returns>The converted path as a <see cref="string"/></returns>
        public static string ConvertToOptionPath(string path, Option option)
        {
            var pathArray = path.Split('\\');

            if (pathArray.Length == 3)
            {
                return $"{string.Join(@"\", pathArray)}\\{option.ShortName}";
            }

            if (pathArray.Length == 4)
            {
                pathArray[3] = option.ShortName;
                return string.Join(@"\", pathArray);
            }

            return path;
        }

        /// <summary>
        /// Shows the data in a <see cref="DataTable"/> in a model <see cref="DXDialog"/>
        /// </summary>
        /// <param name="table">
        /// The <see cref="DataTable"/>
        /// </param>
        /// <param name="allowEdit">
        /// Indicates if editting data in the table is allowed
        /// </param>
        public static void ShowDataTable(DataTable table, bool allowEdit = false)
        {
            var dialog = new DXDialog(table.TableName);
            var dataGrid = new DataGrid();
            dataGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            dataGrid.VerticalAlignment = VerticalAlignment.Stretch;
            dataGrid.ItemsSource = table.DefaultView;
            dataGrid.IsReadOnly = !allowEdit;
            dialog.Content = dataGrid;
            dialog.ShowDialog();
        }

        /// <summary>
        /// Sends output from a report as a text to message bus.
        /// Typically a listener is added to the report <see cref="ReportDesignerViewModel"/> that handles showing this output in the UI.
        /// </summary>
        /// <param name="output">
        /// The output as a <see cref="string"/>
        /// </param>
        public static void AddOutput(string output)
        {
            CDPMessageBus.Current.SendMessage(new ReportOutputEvent(output));
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> out af an <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">The generic type</typeparam>
        /// <param name="source">The <see cref="IEnumerable{T}"/></param>
        /// <returns>the <see cref="DataTable"/></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            return new ObjectShredder<T>().Shred(source, null, null);
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> out af an <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <typeparam name="T">The generic type</typeparam>
        /// <param name="source">The <see cref="IEnumerable{T}"/></param>
        /// <param name="table">An existing <see cref="DataTable"/> to copy data to</param>
        /// <param name="options"><see cref="LoadOption"/> to be used when loading data to <paramref name="table"/></param>
        /// <returns>the <see cref="DataTable"/></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source,
            DataTable table, LoadOption? options)
        {
            return new ObjectShredder<T>().Shred(source, table, options);
        }

        /// <summary>
        /// Helper class to load data from an <see cref="IEnumerable{T}"/> to a <see cref="DataTable"/>
        /// </summary>
        /// <typeparam name="T">The generic <see cref="Type"/> of the <see cref="IEnumerable{T}"/></typeparam>
        private class ObjectShredder<T>
        {
            /// <summary>
            /// Array of <see cref="FieldInfo"/> objects to be used to convert as <see cref="DataColumn"/>s in the result <see cref="DataTable"/>
            /// </summary>
            private readonly FieldInfo[] fieldInfos;

            /// <summary>
            /// Array of <see cref="PropertyInfo"/> objects to be used
            /// </summary>
            private readonly PropertyInfo[] propInfos;

            /// <summary>
            /// <see cref="Dictionary{TKey,TValue}"/> of type <see cref="string"/> and <see cref="int"/> that stores
            /// </summary>
            private readonly Dictionary<string, int> ordinalMap;
            
            /// <summary>
            /// The <see cref="Type"/> of object to be used for the conversion
            /// </summary>
            private readonly Type type;

            /// <summary>
            /// Creates a new instance of the <see cref="ObjectShredder{T}"/> class
            /// </summary>
            public ObjectShredder()
            {
                this.type = typeof(T);
                this.fieldInfos = this.type.GetFields();
                this.propInfos = this.type.GetProperties();
                this.ordinalMap = new Dictionary<string, int>();
            }

            /// <summary>
            /// Loads a DataTable from a sequence of objects.
            /// </summary>
            /// <param name="source">The sequence of objects to load into the DataTable.</param>
            /// <param name="table">The input table. The schema of the table must match that
            /// the type T.  If the table is null, a new table is created with a schema
            /// created from the public properties and fields of the type T.</param>
            /// <param name="options">Specifies how values from the source sequence will be applied to
            /// existing rows in the table.</param>
            /// <returns>A DataTable created from the source sequence.</returns>
            public DataTable Shred(IEnumerable<T> source, DataTable table, LoadOption? options)
            {
                // Load the table from the scalar sequence if T is a primitive type.
                if (typeof(T).IsPrimitive)
                {
                    return this.ShredPrimitive(source, table, options);
                }

                // Create a new table if the input table is null.
                table ??= new DataTable(typeof(T).Name);

                // Initialize the ordinal map and extend the table schema based on type T.
                this.ExtendTable(table, typeof(T));

                // Enumerate the source sequence and load the object values into rows.
                table.BeginLoadData();

                using (var obj = source.GetEnumerator())
                {
                    while (obj.MoveNext())
                    {
                        if (options != null)
                        {
                            table.LoadDataRow(this.ShredObject(table, obj.Current), (LoadOption)options);
                        }
                        else
                        {
                            table.LoadDataRow(this.ShredObject(table, obj.Current), true);
                        }
                    }
                }

                table.EndLoadData();

                // Return the table.
                return table;
            }

            /// <summary>
            /// Loads data from an <see cref="IEnumerable{T}"/> to a <see cref="DataTable"/> where <see cref="T"/> is a primitive type
            /// </summary>
            /// <param name="source">The <see cref="IEnumerable{T}"/></param>
            /// <param name="table">An existing <see cref="DataTable"/> to copy data to</param>
            /// <param name="options"><see cref="LoadOption"/> to be used when loading data to <paramref name="table"/></param>
            /// <returns>The <see cref="DataTable"/></returns>
            private DataTable ShredPrimitive(IEnumerable<T> source, DataTable table, LoadOption? options)
            {
                // Create a new table if the input table is null.
                table ??= new DataTable(typeof(T).Name);

                if (!table.Columns.Contains("Value"))
                {
                    table.Columns.Add("Value", typeof(T));
                }

                // Enumerate the source sequence and load the scalar values into rows.
                table.BeginLoadData();

                using (var obj = source.GetEnumerator())
                {
                    var values = new object[table.Columns.Count];

                    while (obj.MoveNext())
                    {
                        values[table.Columns["Value"].Ordinal] = obj.Current;

                        if (options != null)
                        {
                            table.LoadDataRow(values, (LoadOption)options);
                        }
                        else
                        {
                            table.LoadDataRow(values, true);
                        }
                    }
                }

                table.EndLoadData();

                // Return the table.
                return table;
            }

            /// <summary>
            /// Gets the property and field values of an instance of <see cref="T"/>
            /// </summary>
            /// <param name="table">The <see cref="DataTable"/></param>
            /// <param name="instance">The instance of <see cref="T"/></param>
            /// <returns>An array of objects</returns>
            private object[] ShredObject(DataTable table, T instance)
            {
                var fieldInfos = this.fieldInfos;
                var propInfos = this.propInfos;

                if (instance.GetType() != typeof(T))
                {
                    // If the instance is derived from T, extend the table schema
                    // and get the properties and fields.
                    this.ExtendTable(table, instance.GetType());
                    fieldInfos = instance.GetType().GetFields();
                    propInfos = instance.GetType().GetProperties();
                }

                // Add the property and field values of the instance to an array.
                var values = new object[table.Columns.Count];

                foreach (var fieldInfo in fieldInfos)
                {
                    values[this.ordinalMap[fieldInfo.Name]] = fieldInfo.GetValue(instance);
                }

                foreach (var propertyInfo in propInfos)
                {
                    values[this.ordinalMap[propertyInfo.Name]] = propertyInfo.GetValue(instance, null);
                }

                // Return the property and field values of the instance.
                return values;
            }

            /// <summary>
            /// Extends a <see cref="DataTable"/> with fields from a specific <see cref="Type"/>
            /// </summary>
            /// <param name="table">The <see cref="DataTable"/></param>
            /// <param name="type">The <see cref="Type"/></param>
            private void ExtendTable(DataTable table, Type type)
            {
                // Extend the table schema if the input table was null or if the value
                // in the sequence is derived from type T.
                foreach (var fieldInfo in type.GetFields())
                {
                    if (!this.ordinalMap.ContainsKey(fieldInfo.Name))
                    {
                        // Add the field as a column in the table if it doesn't exist
                        // already.
                        var dataColumn = table.Columns.Contains(fieldInfo.Name) ? table.Columns[fieldInfo.Name] : table.Columns.Add(fieldInfo.Name, Nullable.GetUnderlyingType(fieldInfo.FieldType) ?? fieldInfo.FieldType);

                        // Add the field to the ordinal map.
                        this.ordinalMap.Add(fieldInfo.Name, dataColumn.Ordinal);
                    }
                }

                foreach (var propertyInfo in type.GetProperties())
                {
                    if (!this.ordinalMap.ContainsKey(propertyInfo.Name))
                    {
                        // Add the property as a column in the table if it doesn't exist
                        // already.
                        var dataColumn = table.Columns.Contains(propertyInfo.Name) ? table.Columns[propertyInfo.Name] : table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);

                        // Add the property to the ordinal map.
                        this.ordinalMap.Add(propertyInfo.Name, dataColumn.Ordinal);
                    }
                }
            }
        }
    }
}
