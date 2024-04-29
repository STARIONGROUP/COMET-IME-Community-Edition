// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleRowViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2020 Starion Group S.A.
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

namespace CDP4BuiltInRules.ViewModels
{
    using System;
    using System.Windows;

    using CDP4Composition.DragDrop;
    using CDP4Composition.Services;
    
    /// <summary>
    /// The purpose of <see cref="BuiltInRuleRowViewModel"/> is to represent rows of <see cref="BuiltInRule"/>s in the
    /// <see cref="BuiltInRulesBrowserViewModel"/>.
    /// </summary>
    public class BuiltInRuleRowViewModel : IDragSource
    {
        /// <summary>
        /// The meta data of the <see cref="BuiltInRule"/> that is represented by the current row view-model.
        /// </summary>
        private readonly IBuiltInRuleMetaData metaData;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRuleRowViewModel"/> class.
        /// </summary>
        /// <param name="rule">
        /// The <see cref="BuiltInRule"/> that is represented by the current row view-model.
        /// </param>
        /// <param name="metaData">
        /// The <see cref="IBuiltInRuleMetaData"/> that provides the meta-data for the <see cref="BuiltInRule"/> 
        /// that is represented by the current row view-model.
        /// </param>
        public BuiltInRuleRowViewModel(IBuiltInRule rule, IBuiltInRuleMetaData metaData)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule), "The rule shall not be null");
            }

            if (metaData == null)
            {
                throw new ArgumentNullException(nameof(metaData), "The metaData shall not be null");
            }

            this.Rule = rule;
            this.metaData = metaData;
        }

        /// <summary>
        /// Gets the <see cref="BuiltInRule"/> that is being represented by the current row view-model.
        /// </summary>
        public IBuiltInRule Rule { get; private set; }

        /// <summary>
        /// Gets the author of the <see cref="BuiltInRule"/>.
        /// </summary>
        public string Author => this.metaData.Author;

        /// <summary>
        /// Gets the human readable name of the <see cref="BuiltInRule"/>.
        /// </summary>
        public string Name => this.metaData.Name;

        /// <summary>
        /// Gets the human readable description of the <see cref="BuiltInRule"/>.
        /// </summary>
        public string Description => this.metaData.Description;

        /// <summary>
        /// Queries whether a drag can be started
        /// </summary>
        /// <param name="dragInfo">
        /// Information about the drag.
        /// </param>
        /// <remarks>
        /// To allow a drag to be started, the <see cref="IDragInfo.Effects"/> property on <paramref name="dragInfo"/> 
        /// should be set to a value other than <see cref="DragDropEffects.None"/>. 
        /// </remarks>
        public void StartDrag(IDragInfo dragInfo)
        {
            dragInfo.Payload = this.metaData;
            dragInfo.Effects = DragDropEffects.Copy;
        }
    }
}
