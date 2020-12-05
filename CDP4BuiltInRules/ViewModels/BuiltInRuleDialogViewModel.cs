// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski, Ahmed Abulwafa Ahmed
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

namespace CDP4BuiltInRules.ViewModels
{
    using System;
    using System.Linq;

    using CDP4Composition.Navigation;
    using CDP4Composition.Services;

    using ReactiveUI;

    /// <summary>
    /// Represents a dialog view-model for the <see cref="IBuiltInRuleMetaData"/> interface
    /// </summary>
    public class BuiltInRuleDialogViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRuleDialogViewModel"/> class.
        /// </summary>
        /// <param name="builtInRule">
        /// The <see cref="IBuiltInRule"/> that is represented by the current <see cref="BuiltInRuleDialogViewModel"/>
        /// </param>
        public BuiltInRuleDialogViewModel(IBuiltInRule builtInRule)
        {
            this.SetProperties(builtInRule);

            this.CloseCommand = ReactiveCommand.Create();
            this.CloseCommand.Subscribe(_ => this.ExecuteClose());
        }

        /// <summary>
        /// Gets the Close Command
        /// </summary>
        public ReactiveCommand<object> CloseCommand { get; private set; }

        /// <summary>
        /// Gets the author of the <see cref="BuiltInRule"/>.
        /// </summary>
        public string Author { get; private set; }

        /// <summary>
        /// Gets the human readable name of the <see cref="BuiltInRule"/>.
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// Gets the human readable description of the <see cref="BuiltInRule"/>.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the name of the library the <see cref="IBuiltIn"/> is loaded from
        /// </summary>
        public string LibraryName { get; private set; }

        /// <summary>
        /// Gets the version of the library the <see cref="IBuiltIn"/> is loaded from
        /// </summary>
        public string LibraryVersion { get; private set; }

        /// <summary>
        /// Queries the <see cref="IBuiltInRuleMetaData"/> from the provided <see cref="IBuiltInRule"/>
        /// </summary>
        /// <param name="rule">
        /// The <see cref="IBuiltInRule"/> that is queried
        /// </param>
        /// <remarks>
        /// The <paramref name="rule"/> must be a class that derives from the abstract <see cref="BuiltInRule"/> class.
        /// </remarks>
        private void SetProperties(IBuiltInRule rule)
        {
            var builtInRule = rule as BuiltInRule;

            if (builtInRule == null)
            {
                return;
            }

            this.SetLibraryProperties(builtInRule);

            var t = builtInRule.GetType();
            var attributes = Attribute.GetCustomAttributes(t).OfType<IBuiltInRuleMetaData>();

            var builtInRuleMetaData = attributes.SingleOrDefault();
            if (builtInRuleMetaData != null)
            {
                this.SetMetaDataProperties(builtInRuleMetaData);
            }            
        }

        /// <summary>
        /// Set the library properties
        /// </summary>
        /// <param name="rule">
        /// The <see cref="BuiltInRule"/> from which the library name and version are queried.
        /// </param>
        private void SetLibraryProperties(BuiltInRule rule)
        {
            var assembly = rule.GetType().Assembly;
            this.LibraryName = assembly.GetName().Name;
            this.LibraryVersion = assembly.GetName().Version.ToString();
        }

        /// <summary>
        /// Set IBuiltInRuleMetaData properties
        /// </summary>
        /// <param name="metaData">
        /// The <see cref="IBuiltInRuleMetaData"/> from which the properties are set
        /// </param>
        private void SetMetaDataProperties(IBuiltInRuleMetaData metaData)
        {
            this.Author = metaData.Author;
            this.Name = metaData.Name;
            this.Description = metaData.Description;
        }

        /// <summary>
        /// Executes the Close Command
        /// </summary>
        private void ExecuteClose()
        {
            this.DialogResult = new BaseDialogResult(false);
        }
    }
}
