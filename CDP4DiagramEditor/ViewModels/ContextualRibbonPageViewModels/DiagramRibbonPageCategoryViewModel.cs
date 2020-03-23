// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramBrowserViewModel.cs" company="RHEA S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.ViewModels.ContextualRibbonPageViewModels
{
    using System;
    using System.Reactive.Linq;
    using System.Windows;

    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;

    using CDP4DiagramEditor.Events;
    using CDP4DiagramEditor.Views;

    using DevExpress.Xpf.Charts;

    using Microsoft.Practices.Prism.Regions;

    using ReactiveUI;

    using EventKind = Events.EventKind;

    /// <summary>
    /// The purpose of the <see cref="DiagramRibbonPageCategoryViewModel"/> is to represent the view-model for <see cref="Diagram"/>s
    /// </summary>
    public class DiagramRibbonPageCategoryViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="ShouldItBeVisible"/>
        /// </summary>
        private bool shouldItBeVisible;

        /// <summary>
        /// Gets a value indicating whether the Diagram page category is showing up or not
        /// </summary>
        /// <remarks>
        /// Sets Whether the Diagram page category should be visible or not based on if any DiagramEditor are opened
        /// </remarks>

        public bool ShouldItBeVisible
        {
            get => this.shouldItBeVisible;
            private set => this.RaiseAndSetIfChanged(ref this.shouldItBeVisible, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramRibbonPageCategoryViewModel"/> class.
        /// </summary>
        public DiagramRibbonPageCategoryViewModel()
        {
            this.ShouldItBeVisible = false;
            
            this.AddSubscriptions();

            //regionManager.Regions.WhenAny(region => region.)
        }

        /// <summary>
        /// Add the necessary subscriptions for this view model.
        /// </summary>
        private void AddSubscriptions()
        {
            var subscribeOn =
                CDPMessageBus.Current.Listen<ViewChangedEvent>(this.GetType())
                    //.Where(objectChange => objectChange.EventKind == EventKind.Showing)
                    //.Select(x => x.ChangedView as DiagramEditorViewModel)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(value => this.ShouldItBeVisible = value.EventKind == EventKind.Showing);

            //this.Disposables.Add(subscribeOn);
        }
    }
}
