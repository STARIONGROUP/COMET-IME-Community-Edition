// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedThingDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2022 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Omar Elebiary, Antoine Théate, Jan Oratowski, Jaimer Bernar
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

namespace CDP4CommonView
{
    using System.Linq;

    using CDP4Common.CommonData;

    using CDP4CommonView;

    using CDP4Composition.Mvvm;

    /// <summary>
    /// dialog-view-model class representing a <see cref="DefinedThing"/>
    /// this part of the class is not auto generated and will be persisted
    /// </summary>
    public abstract partial class DefinedThingDialogViewModel<T> : DialogViewModelBase<T> where T : DefinedThing
    {
        /// <summary>
        /// Gets first <see cref="Alias"/> from <see cref="AliasRowViewModel"/>
        /// and returns it and it's language code as a string
        /// </summary>
        public string FirstAlias
        {
            get
            {
                var firstAlias = this.Alias.FirstOrDefault();
                return firstAlias == null ? string.Empty : $"{firstAlias.Content} [{firstAlias.LanguageCode}]";
            }
        }

        /// <summary>
        /// Gets first <see cref="Definition"/> from <see cref="DefinitionRowViewModel"/>
        /// and returns it's content as a string
        /// </summary>
        public string FirstDefinition
        {
            get
            {
                var firstDefinition = this.Definition.FirstOrDefault();
                return firstDefinition?.Content;
            }
        }
    }
}
