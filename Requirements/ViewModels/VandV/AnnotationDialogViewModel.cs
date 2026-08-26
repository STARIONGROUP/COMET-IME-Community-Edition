// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnnotationDialogViewModel.cs" company="Starion Group S.A.">
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
    using System.ComponentModel;
    using System.Reactive;
    using System.Reactive.Linq;

    using CDP4Requirements.Services;

    using CDP4Common.CommonData;
    using CDP4Common.ReportingData;

    using CDP4Composition.Mvvm;
    using CDP4Composition.Navigation;

    using ReactiveUI;

    /// <summary>
    /// The dialog for raising a review annotation, a Review Item Discrepancy (the ECSS non-conformance mechanism), a
    /// Request for Deviation or a Request for Waiver, against the thing selected in the V&amp;V browser.
    /// </summary>
    /// <remarks>
    /// The IME registers no <c>ThingDialog</c> for these classes, so the stock create-annotation commands cannot open
    /// anything; this dialog plus <see cref="Services.AnnotationCreator"/> replace that dead path.
    /// </remarks>
    public class AnnotationDialogViewModel : DialogViewModelBase, IDataErrorInfo
    {
        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        private string title;

        /// <summary>
        /// Backing field for <see cref="ShortName"/>
        /// </summary>
        private string shortName;

        /// <summary>
        /// Backing field for <see cref="Content"/>
        /// </summary>
        private string content;

        /// <summary>
        /// Backing field for <see cref="Classification"/>
        /// </summary>
        private AnnotationClassificationKind classification = AnnotationClassificationKind.MINOR;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationDialogViewModel"/> class.
        /// </summary>
        /// <param name="kind">The kind of annotation being raised.</param>
        /// <param name="annotatedThing">The <see cref="Thing"/> the annotation is raised against.</param>
        public AnnotationDialogViewModel(AnnotationKind kind, Thing annotatedThing)
        {
            this.HasIdentification = kind.HasIdentification;
            this.DialogTitle = $"Create {kind.Name}";
            this.AnnotatedThingCaption = $"{annotatedThing.UserFriendlyShortName}: {annotatedThing.UserFriendlyName}";

            var canOk = this.WhenAnyValue(
                x => x.Title,
                x => x.ShortName,
                x => x.Content,
                (dialogTitle, name, body) =>
                    !string.IsNullOrWhiteSpace(body)
                    && (!this.HasIdentification
                        || (!string.IsNullOrWhiteSpace(dialogTitle) && !string.IsNullOrWhiteSpace(name))));

            this.OkCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(true); }, canOk);
            this.CancelCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(false); });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationDialogViewModel"/> class in reply mode, where
        /// only the message body is asked for.
        /// </summary>
        /// <param name="annotation">The review request being replied to.</param>
        public AnnotationDialogViewModel(EngineeringModelDataAnnotation annotation)
        {
            this.IsReply = true;
            this.HasIdentification = false;
            var identifier = AnnotationKind.QueryShortName(annotation);

            this.DialogTitle = $"Reply to {identifier}";
            this.AnnotatedThingCaption = $"{identifier}: {AnnotationKind.Describe(annotation)}";

            var canOk = this.WhenAnyValue(x => x.Content).Select(body => !string.IsNullOrWhiteSpace(body));

            this.OkCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(true); }, canOk);
            this.CancelCommand = ReactiveCommandCreator.Create(() => { this.DialogResult = new BaseDialogResult(false); });
        }

        /// <summary>
        /// Gets a value indicating whether the dialog is collecting a reply rather than a new review request. In reply
        /// mode the identification fields are hidden: a reply only has a body.
        /// </summary>
        public bool IsReply { get; }

        /// <summary>
        /// Gets a value indicating whether the kind being raised carries the identification fields. A model note has
        /// no short-name, title or classification, and a reply has none either.
        /// </summary>
        public bool HasIdentification { get; }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        public string DialogTitle { get; }

        /// <summary>
        /// Gets the caption identifying the thing being annotated.
        /// </summary>
        public string AnnotatedThingCaption { get; }

        /// <summary>
        /// Gets the selectable <see cref="AnnotationClassificationKind"/>s.
        /// </summary>
        public AnnotationClassificationKind[] PossibleClassifications { get; } =
        {
            AnnotationClassificationKind.MINOR,
            AnnotationClassificationKind.MAJOR
        };

        /// <summary>
        /// Gets or sets the annotation title.
        /// </summary>
        public string Title
        {
            get => this.title;
            set => this.RaiseAndSetIfChanged(ref this.title, value);
        }

        /// <summary>
        /// Gets or sets the annotation short-name.
        /// </summary>
        public string ShortName
        {
            get => this.shortName;
            set => this.RaiseAndSetIfChanged(ref this.shortName, value);
        }

        /// <summary>
        /// Gets or sets the annotation body text.
        /// </summary>
        public string Content
        {
            get => this.content;
            set => this.RaiseAndSetIfChanged(ref this.content, value);
        }

        /// <summary>
        /// Gets or sets the annotation classification.
        /// </summary>
        public AnnotationClassificationKind Classification
        {
            get => this.classification;
            set => this.RaiseAndSetIfChanged(ref this.classification, value);
        }

        /// <summary>
        /// Gets the command that accepts the dialog.
        /// </summary>
        public ReactiveCommand<Unit, Unit> OkCommand { get; }

        /// <summary>
        /// Gets the command that cancels the dialog.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        /// <summary>
        /// Gets the validation error summary. Always null: per-field errors are enough here.
        /// </summary>
        public string Error => null;

        /// <summary>
        /// Gets the validation error for a field.
        /// </summary>
        /// <param name="columnName">The field name.</param>
        /// <returns>The error text, or null when the field is valid.</returns>
        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.Title):
                        return this.HasIdentification && string.IsNullOrWhiteSpace(this.Title) ? "The title is mandatory." : null;
                    case nameof(this.ShortName):
                        return this.HasIdentification && string.IsNullOrWhiteSpace(this.ShortName) ? "The short name is mandatory." : null;
                    case nameof(this.Content):
                        return string.IsNullOrWhiteSpace(this.Content) ? "The content is mandatory." : null;
                    default:
                        return null;
                }
            }
        }
    }
}
