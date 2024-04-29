// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataGridColumn.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2021 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
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

namespace CDP4ReferenceDataMapper.GridColumns
{
    using CDP4ReferenceDataMapper.Managers;

    using ReactiveUI;

    public class DataGridColumn : ReactiveObject
    {
        /// <summary>
        /// Backing field for the <see cref="Visible"/> property
        /// </summary>
        private bool visible = true;

        /// <summary>
        /// Backing field for the <see cref="FieldName"/> property
        /// </summary>
        private string fieldName;

        /// <summary>
        /// Backing field for the <see cref="AllowEditing"/> property
        /// </summary>
        private bool allowEditing;

        /// <summary>
        /// Gets or sets the field name of the column
        /// </summary>
        public string FieldName
        {
            get => this.fieldName;
            set => this.RaiseAndSetIfChanged(ref this.fieldName, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the column
        /// </summary>
        public bool Visible
        {
            get => this.visible;
            set => this.RaiseAndSetIfChanged(ref this.visible, value);
        }

        /// <summary>
        /// Gets or sets the AllowEditing functionality of the column
        /// </summary>
        public bool AllowEditing
        {
            get => this.allowEditing;
            set => this.RaiseAndSetIfChanged(ref this.allowEditing, value);
        }

        /// <summary>
        /// The <see cref="Managers.DataSourceManager"/> where this column belongs to.
        /// </summary>
        public DataSourceManager DataSourceManager { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DataGridColumn"/> class.
        /// </summary>
        /// <param name="dataSourceManager">The associated <see cref="Managers.DataSourceManager"/></param>
        public DataGridColumn(DataSourceManager dataSourceManager)
        {
            this.DataSourceManager = dataSourceManager;
        }
    }
}
