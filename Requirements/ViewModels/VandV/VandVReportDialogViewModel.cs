// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VandVReportDialogViewModel.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2026 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Rowan de Voogt
//
//    This file is part of CDP4-COMET IME Community Edition.
//    The CDP4-COMET IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-COMET IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-COMET IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Text.RegularExpressions;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using CDP4Dal;

    using ReactiveUI;

    /// <summary>
    /// The dialog for creating a report (deliverable): the document a set of V&amp;V activities is recorded in, such
    /// as a FAT report or a mass budget. A report is a <see cref="RequirementsSpecification"/> of its own, so it is
    /// created deliberately, exactly like a requirements specification, and the activities recorded in it live
    /// inside it.
    /// </summary>
    public class VandVReportDialogViewModel : DialogViewModelBase, IDataErrorInfo
    {
        /// <summary>
        /// Matches a valid short-name: letters, digits or underscores, exactly as the other V&amp;V dialogs accept.
        /// </summary>
        private static readonly Regex ShortNamePattern = new Regex(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

        /// <summary>
        /// The specification short-names already used in the iteration, so a duplicate is rejected before the server
        /// does.
        /// </summary>
        private readonly HashSet<string> usedShortNames;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Backing field for <see cref="Owner"/>
        /// </summary>
        private DomainOfExpertise owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="VandVReportDialogViewModel"/> class.
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> the report is created in.</param>
        /// <param name="session">The <see cref="ISession"/>.</param>
        public VandVReportDialogViewModel(Iteration iteration, ISession session)
        {
            var model = (EngineeringModel)iteration.Container;

            this.PossibleOwners = model.EngineeringModelSetup.ActiveDomain.OrderBy(x => x.Name).ToList();
            this.Owner = session.OpenIterations.TryGetValue(iteration, out var tuple) ? tuple?.Item1 : null;

            this.usedShortNames = new HashSet<string>(iteration.RequirementsSpecification.Select(x => x.ShortName));

            var canOk = this.WhenAnyValue(x => x.ShortName, x => x.Name, x => x.Owner)
                .Select(_ => !this.HasValidationErrors());

            this.OkCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(true); }, canOk);
            this.CancelCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(false); });
        }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string Title => "Create V&V Report";

        /// <summary>Gets the possible owning <see cref="DomainOfExpertise"/>s.</summary>
        public IReadOnlyList<DomainOfExpertise> PossibleOwners { get; }

        /// <summary>
        /// Gets or sets the document reference, which is the report's short-name. Required.
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets or sets the report title. Required.
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.RaiseAndSetIfChanged(ref this.name, value);
        }

        /// <summary>Gets or sets the owning <see cref="DomainOfExpertise"/>. Required.</summary>
        public DomainOfExpertise Owner
        {
            get => this.owner;
            set => this.RaiseAndSetIfChanged(ref this.owner, value);
        }

        /// <summary>
        /// Gets the command that accepts the dialog. Enabled once every required field is valid.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        /// <summary>
        /// Gets the command that cancels the dialog.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Gets an error message for the object as a whole. Not used; per-property validation is used instead.
        /// </summary>
        public string Error => string.Empty;

        /// <summary>
        /// Validates a single property, so the editors show the standard error adornment.
        /// </summary>
        /// <param name="columnName">The property name.</param>
        /// <returns>The validation message, or an empty string when valid.</returns>
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.ShortName):
                        if (string.IsNullOrWhiteSpace(this.ShortName))
                        {
                            return "The document reference is mandatory.";
                        }

                        if (!ShortNamePattern.IsMatch(this.ShortName))
                        {
                            return "The document reference may contain only letters, digits and underscores (no spaces or punctuation).";
                        }

                        return this.usedShortNames.Contains(this.ShortName)
                            ? "Another specification or report already uses this reference."
                            : string.Empty;

                    case nameof(this.Name):
                        return string.IsNullOrWhiteSpace(this.Name) ? "The title is mandatory." : string.Empty;

                    case nameof(this.Owner):
                        return this.Owner == null ? "The owner is mandatory." : string.Empty;

                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Asserts whether any required field currently fails validation.
        /// </summary>
        /// <returns>true when the dialog cannot be accepted.</returns>
        private bool HasValidationErrors()
        {
            var validated = new[] { nameof(this.ShortName), nameof(this.Name), nameof(this.Owner) };

            return validated.Any(property => !string.IsNullOrEmpty(this[property]));
        }
    }
}
