﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlossaryDialogViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Alexander van Delft, Nathanael Smiechowski
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

namespace BasicRdl.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using CDP4Common;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Attributes;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    
    using CDP4Dal;
    using CDP4Dal.Operations;

    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="GlossaryDialogViewModel"/> is to allow a <see cref="Glossary"/> to
    /// be created or updated.
    /// </summary>
    /// <remarks>
    /// The creation of an <see cref="Glossary"/> will result in an <see cref="Glossary"/> being created by
    /// the connected data-source
    /// </remarks>
    [ThingDialogViewModelExport(ClassKind.Glossary)]
    public class GlossaryDialogViewModel : CDP4CommonView.GlossaryDialogViewModel, IThingDialogViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GlossaryDialogViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// The default constructor is required by MEF
        /// </remarks>
        public GlossaryDialogViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlossaryDialogViewModel"/> class.
        /// </summary>
        /// <param name="glossary">
        /// The <see cref="Glossary"/> that is the subject of the current view-model. This is the object
        /// that will be either created, or edited.
        /// </param>
        /// <param name="transaction">
        /// The <see cref="ThingTransaction"/> that contains the log of recorded changes.
        /// </param>
        /// <param name="session">
        /// The <see cref="ISession"/> in which the current <see cref="Thing"/> is to be added or updated
        /// </param>
        /// <param name="isRoot">
        /// Assert if this <see cref="CategoryDialogViewModel"/> is the root of all <see cref="IThingDialogViewModel"/>
        /// </param>
        /// <param name="dialogKind">
        /// The kind of operation this <see cref="CategoryDialogViewModel"/> performs
        /// </param>
        /// <param name="thingDialogNavigationService">
        /// The <see cref="IThingDialogNavigationService"/>
        /// </param>
        /// <param name="container">
        /// The Container <see cref="Thing"/> of the created <see cref="Thing"/>
        /// </param>
        /// <param name="chainOfContainers">
        /// The optional chain of containers that contains the <paramref name="container"/> argument
        /// </param>
        public GlossaryDialogViewModel(Glossary glossary, IThingTransaction transaction, ISession session, bool isRoot, ThingDialogKind dialogKind, IThingDialogNavigationService thingDialogNavigationService, Thing container = null, IEnumerable<Thing> chainOfContainers = null)
            : base(glossary, transaction, session, isRoot, dialogKind, thingDialogNavigationService, container, chainOfContainers)
        {
            this.WhenAnyValue(x => x.Container).Subscribe(_ => this.PopulateCategory());
        }

        /// <summary>
        /// Populate the terms
        /// </summary>
        protected override void PopulateTerm()
        {
            this.Term.Clear();
            foreach (var thing in this.Thing.Term.Where(t => t.ChangeKind != ChangeKind.Delete))
            {
                var row = new TermRowViewModel(thing, this.Session, this);
                row.DefinitionValue = thing.Definition.Any() ? thing.Definition.First().Content : string.Empty;
                this.Term.Add(row);
            }
        }

        /// <summary>
        /// Populate the Categories
        /// </summary>
        protected override void PopulateCategory()
        {
            this.PopulatePossibleCategory();
            base.PopulateCategory();
        }

        /// <summary>
        /// populate the possible <see cref="Category"/>
        /// </summary>
        private void PopulatePossibleCategory()
        {
            this.PossibleCategory.Clear();
            var container = this.Container as ReferenceDataLibrary;
            if (container == null)
            {
                return;
            }

            var allowedCategories = new List<Category>(container.DefinedCategory.Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));
            allowedCategories.AddRange(container.GetRequiredRdls().SelectMany(rdl => rdl.DefinedCategory)
                        .Where(c => c.PermissibleClass.Contains(this.Thing.ClassKind)));

            this.PossibleCategory.AddRange(allowedCategories.OrderBy(c => c.ShortName));
        }
    }
}
