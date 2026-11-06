// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModellingThingBrowserViewModelBase.cs" company="Starion Group S.A.">
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

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    using CDP4CommonView.ViewModels;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.ReportingData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.Annotations;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Composition.PluginSettingService;
    using CDP4Composition.Services;
    using CDP4Composition.ViewModels;

    using CDP4Dal;
    using CDP4Dal.Events;

    using CommonServiceLocator;

    using ReactiveUI;

    /// <summary>
    /// The View-Model-base that shall be used by a view-model representing a Browser that displays modelling data.
    /// </summary>
    /// <remarks>
    /// Carries the whole annotation experience, raising, reading, replying to and retiring the ECSS-E-TM-10-25
    /// annotations, so every browser over model data offers it by inheriting rather than by re-implementing. It used
    /// to expose one hard-coded create command per kind which no view ever bound and which navigated to a
    /// <c>ThingDialog</c> the IME does not register, so none of them could ever open anything.
    /// </remarks>
    public abstract class ModellingThingBrowserViewModelBase : BrowserViewModelBase<Iteration>
    {
        /// <summary>
        /// The service that writes the annotations.
        /// </summary>
        private readonly IAnnotationService annotationService = new AnnotationService();

        /// <summary>
        /// Backing field for <see cref="CreatableAnnotationKinds"/>
        /// </summary>
        private IReadOnlyCollection<ClassKind> creatableAnnotationKinds = new HashSet<ClassKind>();

        /// <summary>
        /// The <see cref="IMessageBoxService"/>, resolved on first failure so a browser can be unit-tested without
        /// a composition container.
        /// </summary>
        private IMessageBoxService messageBoxService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModellingThingBrowserViewModelBase"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/> that contains the data to browse</param>
        /// <param name="session">The session</param>
        /// <param name="thingDialogNavigationService">the <see cref="IThingDialogNavigationService"/></param>
        /// <param name="panelNavigationService">the <see cref="IPanelNavigationService"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        /// <param name="pluginSettingsService">
        /// The <see cref="IPluginSettingsService"/> used to read and write plugin setting files.
        /// </param>
        protected ModellingThingBrowserViewModelBase(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService, IPluginSettingsService pluginSettingsService)
            : base(iteration, session, thingDialogNavigationService, panelNavigationService, dialogNavigationService, pluginSettingsService)
        {
            this.AddAnnotationSubscriptions();
        }

        /// <summary>
        /// Gets the context menu group that lists the annotations raised against the selected row.
        /// </summary>
        public ContextMenuItemViewModel AnnotationMenuGroup { get; private set; }

        /// <summary>
        /// Gets the context menu group that offers one create command per annotation kind.
        /// </summary>
        public ContextMenuItemViewModel CreateAnnotationMenuGroup { get; private set; }

        /// <summary>
        /// Gets the create command per <see cref="AnnotationKind"/>, so a browser offers every kind by iterating
        /// <see cref="AnnotationKind.All"/> instead of carrying a property per kind.
        /// </summary>
        public IReadOnlyDictionary<AnnotationKind, ReactiveCommand<Unit, Unit>> CreateAnnotationCommands { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClassKind"/>s of the annotations the current participant may create in this model.
        /// </summary>
        public IReadOnlyCollection<ClassKind> CreatableAnnotationKinds
        {
            get => this.creatableAnnotationKinds;
            private set => this.RaiseAndSetIfChanged(ref this.creatableAnnotationKinds, value);
        }

        /// <summary>
        /// Initializes the <see cref="ReactiveCommand{TParam,TResult}"/>s of this view-model.
        /// </summary>
        protected override void InitializeCommands()
        {
            base.InitializeCommands();

            this.CreateAnnotationCommands = AnnotationKind.Creatable.ToDictionary(
                kind => kind,
                kind => ReactiveCommandCreator.CreateAsyncTask(
                    () => this.ExecuteCreateAnnotation(kind),
                    this.WhenAnyValue(
                        x => x.SelectedThing,
                        x => x.CreatableAnnotationKinds,
                        (row, kinds) => row != null && kinds.Contains(kind.ClassKind))));
        }

        /// <summary>
        /// Computes the permissions of the current participant.
        /// </summary>
        public override void ComputePermission()
        {
            base.ComputePermission();

            if (this.Thing == null)
            {
                return;
            }

            this.CreatableAnnotationKinds = new HashSet<ClassKind>(
                AnnotationKind.All
                    .Select(kind => kind.ClassKind)
                    .Where(classKind => this.PermissionService.CanWrite(classKind, this.Thing.TopContainer)));
        }

        /// <summary>
        /// Populates the context menu.
        /// </summary>
        public override void PopulateContextMenu()
        {
            base.PopulateContextMenu();

            if (this.CreateAnnotationCommands == null)
            {
                return;
            }

            if (this.CreateAnnotationMenuGroup == null)
            {
                this.CreateAnnotationMenuGroup = new ContextMenuItemViewModel("Create an Annotation", "", null, MenuItemKind.Create, ClassKind.ModellingAnnotationItem);

                foreach (var kind in AnnotationKind.Creatable)
                {
                    this.CreateAnnotationMenuGroup.SubMenu.Add(
                        new ContextMenuItemViewModel(
                            kind.Name,
                            "",
                            this.CreateAnnotationCommands[kind],
                            MenuItemKind.Create,
                            kind.ClassKind));
                }
            }

            if (this.AnnotationMenuGroup == null)
            {
                this.AnnotationMenuGroup = new ContextMenuItemViewModel("Annotations", "", null, MenuItemKind.Navigate, ClassKind.ModellingAnnotationItem);
            }

            this.ContextMenu.Add(this.CreateAnnotationMenuGroup);
            this.ContextMenu.Add(this.AnnotationMenuGroup);
            this.PopulateAnnotationMenuItemGroup();
        }

        /// <summary>
        /// Fills the annotation group with the annotations raised against the selected row, each with the actions it
        /// supports: open it to read and reply on the discussion board, reply straight away, mark it implemented,
        /// close it, or reopen it.
        /// </summary>
        protected virtual void PopulateAnnotationMenuItemGroup()
        {
            this.AnnotationMenuGroup.SubMenu.Clear();

            if (this.SelectedThing == null)
            {
                return;
            }

            var annotations = AnnotationQuery.QueryAllFor(this.SelectedThing.Thing);

            if (annotations.Count == 0)
            {
                this.AnnotationMenuGroup.SubMenu.Add(
                    new ContextMenuItemViewModel("(none raised against this item)", "", null, MenuItemKind.None, ClassKind.NotThing));

                return;
            }

            foreach (var annotation in annotations)
            {
                this.AnnotationMenuGroup.SubMenu.Add(this.BuildAnnotationMenuItem(annotation));
            }
        }

        /// <summary>
        /// Opens the floating annotation window, the discussion board, for an annotation of any kind.
        /// </summary>
        /// <param name="annotation">The <see cref="EngineeringModelDataAnnotation"/> to open.</param>
        protected virtual void ExecuteOpenAnnotationWindow(EngineeringModelDataAnnotation annotation)
        {
            var viewModel = new AnnotationFloatingDialogViewModel(annotation, this.Session);
            this.DialogNavigationService.NavigateFloating(viewModel);
        }

        /// <summary>
        /// Re-reads the annotations of every row in this browser, so the badges and icons follow the annotations
        /// raised, replied to and closed during the session.
        /// </summary>
        protected void RefreshAnnotationStates()
        {
            foreach (var row in Descendants(this.RootRowViewModels))
            {
                row.RefreshAnnotationState();
            }

            this.PopulateContextMenu();
        }

        /// <summary>
        /// Builds the menu item for one annotation, with the actions the current participant may perform on it.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <returns>The <see cref="ContextMenuItemViewModel"/>.</returns>
        private ContextMenuItemViewModel BuildAnnotationMenuItem(EngineeringModelDataAnnotation annotation)
        {
            var status = AnnotationQuery.DescribeStatus(annotation);
            var hasStatus = !string.IsNullOrEmpty(status);
            var canWrite = this.PermissionService.CanWrite(annotation);

            var identifier = AnnotationKind.QueryShortName(annotation);

            var header = hasStatus
                ? $"{identifier}, {AnnotationKind.Describe(annotation)} ({status})"
                : $"{identifier}, {AnnotationKind.Describe(annotation)}";

            var group = new ContextMenuItemViewModel(header, "", null, MenuItemKind.None, annotation.ClassKind);

            group.SubMenu.Add(
                new ContextMenuItemViewModel(
                    "Open Discussion Board...",
                    "",
                    x => this.ExecuteOpenAnnotationWindow((EngineeringModelDataAnnotation)x),
                    annotation,
                    true,
                    MenuItemKind.Navigate));

            group.SubMenu.Add(
                new ContextMenuItemViewModel(
                    "Reply...",
                    "",
                    x => this.ReplyToAnnotation((EngineeringModelDataAnnotation)x),
                    annotation,
                    canWrite,
                    MenuItemKind.Create));

            if (hasStatus)
            {
                var statusGroup = new ContextMenuItemViewModel("Set Status", "", null, MenuItemKind.Edit, ClassKind.NotThing);
                var currentStatus = ((ModellingAnnotationItem)annotation).Status;

                foreach (var choice in AnnotationQuery.StatusChoices)
                {
                    var target = choice.Status;

                    statusGroup.SubMenu.Add(
                        new ContextMenuItemViewModel(
                            choice.Caption,
                            "",
                            x => this.SetAnnotationStatus((ModellingAnnotationItem)x, target),
                            annotation,
                            canWrite && currentStatus != target,
                            MenuItemKind.Edit));
                }

                group.SubMenu.Add(statusGroup);
            }

            group.SubMenu.Add(
                new ContextMenuItemViewModel(
                    "Delete",
                    "",
                    x => this.DeleteAnnotation((EngineeringModelDataAnnotation)x),
                    annotation,
                    this.PermissionService.CanWrite(annotation),
                    MenuItemKind.Delete));

            return group;
        }

        /// <summary>
        /// Opens the annotation dialog for the selected thing and, on OK, writes the annotation.
        /// </summary>
        /// <param name="kind">The kind of annotation to raise.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        private async Task ExecuteCreateAnnotation(AnnotationKind kind)
        {
            var annotatedThing = this.SelectedThing?.Thing;

            if (annotatedThing == null)
            {
                return;
            }

            try
            {
                var dialogViewModel = new AnnotationDialogViewModel(kind, annotatedThing);
                var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

                var participantAndDomain = this.QueryParticipantAndDomain();

                if (result?.Result != true || participantAndDomain == null)
                {
                    return;
                }

                await this.annotationService.CreateAsync(
                    this.Session,
                    kind.Create(),
                    annotatedThing,
                    participantAndDomain.Item2,
                    participantAndDomain.Item1,
                    dialogViewModel.Title,
                    dialogViewModel.ShortName,
                    dialogViewModel.Content,
                    dialogViewModel.Classification);
            }
            catch (Exception ex)
            {
                this.ReportAnnotationFailure($"The {kind.Name} could not be created", ex);
            }
        }

        /// <summary>
        /// Asks for a reply and posts it onto an annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        private async void ReplyToAnnotation(EngineeringModelDataAnnotation annotation)
        {
            try
            {
                var dialogViewModel = new AnnotationDialogViewModel(annotation);
                var result = this.DialogNavigationService.NavigateModal(dialogViewModel);

                var participantAndDomain = this.QueryParticipantAndDomain();

                if (result?.Result != true || participantAndDomain == null)
                {
                    return;
                }

                await this.annotationService.ReplyAsync(this.Session, annotation, participantAndDomain.Item2, dialogViewModel.Content);
            }
            catch (Exception ex)
            {
                this.ReportAnnotationFailure("The reply could not be posted", ex);
            }
        }

        /// <summary>
        /// Asks for confirmation and deletes an annotation.
        /// </summary>
        /// <param name="annotation">The annotation to delete.</param>
        private async void DeleteAnnotation(EngineeringModelDataAnnotation annotation)
        {
            try
            {
                // the stock delete confirmation, so deleting an annotation looks like every other delete in the IME
                var confirmation = new ConfirmationDialogViewModel(annotation);
                var dialogResult = this.DialogNavigationService.NavigateModal(confirmation);

                if (dialogResult?.Result != true)
                {
                    return;
                }

                await this.annotationService.DeleteAsync(this.Session, annotation);
            }
            catch (Exception ex)
            {
                this.ReportAnnotationFailure("The annotation could not be deleted", ex);
            }
        }

        /// <summary>
        /// Resolves the <see cref="IMessageBoxService"/>, on first use so a browser can be unit-tested without a
        /// composition container.
        /// </summary>
        /// <returns>The <see cref="IMessageBoxService"/>.</returns>
        private IMessageBoxService QueryMessageBoxService()
        {
            this.messageBoxService = this.messageBoxService ?? ServiceLocator.Current.GetInstance<IMessageBoxService>();

            return this.messageBoxService;
        }

        /// <summary>
        /// Resolves the domain and participant the current user acts as in this iteration.
        /// </summary>
        /// <returns>The pair, or null while the open-iteration map is being re-assembled.</returns>
        private Tuple<DomainOfExpertise, Participant> QueryParticipantAndDomain()
        {
            this.Session.OpenIterations.TryGetValue(this.Thing, out var participantAndDomain);

            return participantAndDomain;
        }

        /// <summary>
        /// Writes a new status onto an annotation.
        /// </summary>
        /// <param name="annotation">The annotation.</param>
        /// <param name="status">The new status.</param>
        private async void SetAnnotationStatus(ModellingAnnotationItem annotation, AnnotationStatusKind status)
        {
            try
            {
                await this.annotationService.SetStatusAsync(this.Session, annotation, status);
            }
            catch (Exception ex)
            {
                this.ReportAnnotationFailure("The annotation could not be updated", ex);
            }
        }

        /// <summary>
        /// Subscribes to the changes of every annotation kind in this model, so the row badges and the context menu
        /// follow what other participants raise and close.
        /// </summary>
        private void AddAnnotationSubscriptions()
        {
            var listeners = AnnotationKind.All
                .Select(kind => this.CDPMessageBus.Listen<ObjectChangedEvent>(kind.AnnotationType))
                .Aggregate((left, right) => left.Merge(right));

            this.Disposables.Add(
                listeners
                    .Where(x => this.Thing != null && x.ChangedThing.GetContainerOfType<EngineeringModel>() == this.Thing.GetContainerOfType<EngineeringModel>())
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(_ => this.RefreshAnnotationStates()));
        }

        /// <summary>
        /// Tells the user that an annotation write failed, and logs the exception.
        /// </summary>
        /// <param name="caption">What was being attempted.</param>
        /// <param name="exception">The exception that was raised.</param>
        private void ReportAnnotationFailure(string caption, Exception exception)
        {
            logger.Error(exception, caption);

            this.QueryMessageBoxService().Show(
                $"{caption}:{Environment.NewLine}{Environment.NewLine}{exception.Message}",
                this.Caption,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        /// <summary>
        /// Walks a row hierarchy depth-first.
        /// </summary>
        /// <param name="rows">The rows to walk.</param>
        /// <returns>The rows and all of their descendants.</returns>
        private static IEnumerable<IRowViewModelBase<Thing>> Descendants(IEnumerable<IRowViewModelBase<Thing>> rows)
        {
            foreach (var row in rows)
            {
                yield return row;

                foreach (var descendant in Descendants(row.ContainedRows))
                {
                    yield return descendant;
                }
            }
        }
    }
}
