// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentGroupAdapter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using DevExpress.Xpf.Docking;
    using DevExpress.Xpf.Docking.Base;
    using DevExpress.Xpf.Layout.Core;

    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;
    using Navigation;
    using ViewModels;

    /// <summary>
    /// A region adaptor for the <see cref="DocumentGroup"/>
    /// </summary>
    [Export(typeof(DocumentGroupAdapter)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class DocumentGroupAdapter : RegionAdapterBase<DocumentGroup>
    {
        /// <summary>
        /// Associates a view to a documentPanel
        /// </summary>
        private Dictionary<IPanelView, DocumentPanel> viewPanelPair;

        /// <summary>
        /// The MEF injected <see cref="IDialogNavigationService"/>
        /// </summary>
        private IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentGroupAdapter"/> class.
        /// </summary>
        /// <param name="behaviorFactory">
        /// The behavior factory.
        /// </param>
        /// <param name="dialogNavigationService">
        /// The <see cref="IDialogNavigationService"/>
        /// </param>
        [ImportingConstructor]
        public DocumentGroupAdapter(IRegionBehaviorFactory behaviorFactory) : base(behaviorFactory)
        {
            this.viewPanelPair = new Dictionary<IPanelView, DocumentPanel>();
        }

        /// <summary>
        /// Returns a <see cref="IRegion"/> instance to be associated with the adapted control.
        /// </summary>
        /// <returns>
        /// an instance of <see cref="IRegion"/>
        /// </returns>
        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }

        /// <summary>
        /// Adapts the <see cref="DocumentGroup"/> in the association <see cref="IRegion"/>
        /// </summary>
        /// <param name="region">
        /// The associated <see cref="IRegion"/>
        /// </param>
        /// <param name="regionTarget">
        /// the control to adapt
        /// </param>
        protected override void Adapt(IRegion region, DocumentGroup regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                this.OnViewsCollectionChanged(region, regionTarget, s, e);
            };

            regionTarget.GetDockLayoutManager().DockItemClosing += (s, e) => this.OnPanelClosing(region, regionTarget, s, e);
            regionTarget.GetDockLayoutManager().DockItemClosed += (s, e) => this.OnPanelClosed(region, regionTarget, s, e);

            regionTarget.GetDockLayoutManager().DockOperationStarting += this.DocumentGroupAdapter_DockOperationStarting;
        }

        /// <summary>
        /// Handles the DockOperationStarting event
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="DockOperationStartingEventArgs"/></param>
        private void DocumentGroupAdapter_DockOperationStarting(object sender, DockOperationStartingEventArgs e)
        {
            if (e.DockOperation != DockOperation.Dock)
            {
                return;
            }

            if (!(e.DockTarget is DocumentGroup documentGroup))
            {
                return;
            }

            if (!(e.Item is TabbedGroup tabbedGroup))
            {
                return;
            }

            var panelRegionName = tabbedGroup.GetValue(RegionManager.RegionNameProperty).ToString();

            if (string.IsNullOrWhiteSpace(panelRegionName))
            {
                return;
            }

            e.Cancel = true;
            var itemsToCopy = tabbedGroup.Items.ToArray();
            tabbedGroup.Items.Clear();
            documentGroup.AddRange(itemsToCopy);

            switch (panelRegionName)
            {
                case RegionNames.LeftPanel:
                    tabbedGroup.GetDockLayoutManager().DockController.Dock(tabbedGroup, documentGroup.Parent, DockType.Left);
                    break;
                case RegionNames.RightPanel:
                    tabbedGroup.GetDockLayoutManager().DockController.Dock(tabbedGroup, documentGroup.Parent, DockType.Right);
                    break;
            }
        }

        /// <summary>
        /// Handles the Dock Panel closing event 
        /// </summary>
        /// <param name="region">The region</param>
        /// <param name="regionTarget">The region target</param>
        /// <param name="s">The sender</param>
        /// <param name="e">The <see cref="ItemCancelEventArgs"/></param>
        private void OnPanelClosing(IRegion region, DocumentGroup regionTarget, object s, ItemCancelEventArgs e)
        {
            if (!e.Item.Parent.Equals(regionTarget))
            {
                return;
            }

            var docPanel = e.Item as LayoutPanel;
            if (docPanel == null)
            {
                return;
            }

            var view = (UserControl)docPanel.Content;
            var panelViewModel = (IPanelViewModel)view.DataContext;
            if (panelViewModel == null)
            {
                return;
            }

            if (!panelViewModel.IsDirty)
            {
                return;
            }

            if (this.dialogNavigationService == null)
            {
                this.dialogNavigationService = ServiceLocator.Current.GetInstance<IDialogNavigationService>();
            }

            var confirmation = new GenericConfirmationDialogViewModel("Warning", MessageHelper.ClosingPanelConfirmation);
            var result = this.dialogNavigationService.NavigateModal(confirmation);
            if (result != null && result.Result.HasValue && result.Result.Value)
            {
                return;
            }

            e.Cancel = true;
        }

        /// <summary>
        /// Handles a closed panel event and removes the associated <see cref="IPanelView"/> from the region
        /// </summary>
        /// <param name="region">The <see cref="IRegion"/></param>
        /// <param name="regionTarget">The <see cref="DocumentGroup"/></param>
        /// <param name="s">The sender</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnPanelClosed(IRegion region, DocumentGroup regionTarget, object s, DockItemClosedEventArgs e)
        {
            foreach (var panel in e.AffectedItems)
            {
                var docPanel = panel as DocumentPanel;
                if (docPanel != null)
                {
                    var view = this.viewPanelPair.SingleOrDefault(x => x.Value == docPanel).Key;
                    if (view != null)
                    {
                        this.viewPanelPair.Remove(view);
                        
                        region.Remove(view);
                    }
                }
            }
        }

        /// <summary>
        /// Redirects to the appropriate event handler upon a collection changed
        /// </summary>
        /// <param name="region">The <see cref="IRegion"/></param>
        /// <param name="regionTarget">The <see cref="DocumentGroup"/></param>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnViewsCollectionChanged(IRegion region, DocumentGroup regionTarget, object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.OnAdd(region, regionTarget, sender, e);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                this.OnRemove(region, regionTarget, sender, e);
            }
        }

        /// <summary>
        /// Handles the CollectionChanged event when an element is added
        /// </summary>
        /// <param name="region">The <see cref="IRegion"/></param>
        /// <param name="regionTarget">The <see cref="DocumentGroup"/></param>
        /// <param name="sender">The sender</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/></param>
        private void OnAdd(IRegion region, DocumentGroup regionTarget, object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (object view in e.NewItems)
            {
                var documentGroupView = view as IPanelView;
                if (documentGroupView == null)
                {
                    throw new ArgumentException(string.Format("The added view does not respect the interface : {0}", view));
                }

                var viewModel = documentGroupView.DataContext as IPanelViewModel;
                if (viewModel == null)
                {
                    throw new ArgumentException(string.Format("The viewmodel does not respect the interface"));
                }

                var manager = regionTarget.GetDockLayoutManager();
                var panel = manager.DockController.AddDocumentPanel(regionTarget);

                panel.Content = documentGroupView;
                panel.ClosingBehavior = ClosingBehavior.ImmediatelyRemove;

                var captionProperty = new Binding();
                captionProperty.Source = viewModel;
                captionProperty.Path = new PropertyPath("Caption");
                captionProperty.Mode = BindingMode.OneWay;
                captionProperty.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(panel, DocumentPanel.CaptionProperty, captionProperty);

                var tooltipProperty = new Binding();
                tooltipProperty.Source = viewModel;
                tooltipProperty.Path = new PropertyPath("ToolTip");
                tooltipProperty.Mode = BindingMode.OneWay;
                tooltipProperty.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(panel, DocumentPanel.ToolTipProperty, tooltipProperty);

                // not sure that thing even does anything
                // without it the panels are still activated from AddDocumentPanel it seems
                manager.DockController.Activate(panel);

                this.viewPanelPair.Add(documentGroupView, panel);
            }
        }

        /// <summary>
        /// Handles the region.Views.CollectionChanged event when elements have been removed
        /// </summary>
        /// <param name="region">the region</param>
        /// <param name="regionTarget">the region target</param>
        /// <param name="s">the sender</param>
        /// <param name="e">the NotifyCollectionChangedEventArgs</param>
        private void OnRemove(IRegion region, DocumentGroup regionTarget, object s, NotifyCollectionChangedEventArgs e)
        {
            foreach (var view in e.OldItems)
            {
                var panelView = view as IPanelView;
                if (panelView != null)
                {
                    DocumentPanel documentPanel;
                    if (this.viewPanelPair.TryGetValue(panelView, out documentPanel))
                    {
                        this.viewPanelPair.Remove(panelView);
                        regionTarget.GetDockLayoutManager().DockController.RemovePanel(documentPanel);
                    }
                }
            }
        }
    }
}