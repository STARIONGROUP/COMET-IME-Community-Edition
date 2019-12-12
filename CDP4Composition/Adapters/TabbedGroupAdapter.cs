// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabbedGroupAdapter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
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
    using Attributes;
    using DevExpress.Xpf.Docking;
    using DevExpress.Xpf.Docking.Base;
    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;
    using Navigation;
    using ViewModels;

    /// <summary>
    /// A region adaptor for the <see cref="TabbedGroup"/>
    /// </summary>
    [Export(typeof(TabbedGroupAdapter)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class TabbedGroupAdapter : RegionAdapterBase<TabbedGroup>
    {
        /// <summary>
        /// Associates a view to a documentPanel
        /// </summary>
        private Dictionary<IPanelView, LayoutPanel> viewPanelPair;

        /// <summary>
        /// The MEF injected <see cref="IDialogNavigationService"/>
        /// </summary>
        private IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabbedGroupAdapter"/> class.
        /// </summary>
        /// <param name="behaviorFactory">
        /// The behavior factory.
        /// </param>
        /// <param name="dialogNavigationService">
        /// The MEF injected <see cref="IDialogNavigationService"/>
        /// </param>
        [ImportingConstructor]
        public TabbedGroupAdapter(IRegionBehaviorFactory behaviorFactory) : base(behaviorFactory)
        {
            this.viewPanelPair = new Dictionary<IPanelView, LayoutPanel>();
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
        /// Adapts the <see cref="LayoutGroup"/> in the association <see cref="IRegion"/>
        /// </summary>
        /// <param name="region">
        /// The associated <see cref="IRegion"/>
        /// </param>
        /// <param name="regionTarget">
        /// the control to adapt
        /// </param>
        protected override void Adapt(IRegion region, TabbedGroup regionTarget)
        {
            region.Views.CollectionChanged += (s, e) => this.OnViewsCollectionChanged(region, regionTarget, s, e);
            regionTarget.GetDockLayoutManager().DockItemClosed += (s, e) => this.OnPanelClosed(region, regionTarget, s, e);
            regionTarget.GetDockLayoutManager().DockItemClosing += (s, e) => this.OnPanelClosing(region, regionTarget, s, e);
        }

        /// <summary>
        /// Handles the region.Views.CollectionChanged event
        /// </summary>
        /// <param name="region">the region</param>
        /// <param name="regionTarget">the region target</param>
        /// <param name="sender">the sender</param>
        /// <param name="e">the NotifyCollectionChangedEventArgs</param>
        private void OnViewsCollectionChanged(IRegion region, LayoutGroup regionTarget, object sender, NotifyCollectionChangedEventArgs e)
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
        /// Handles the region.Views.CollectionChanged event when elements have been added
        /// </summary>
        /// <param name="region">the region</param>
        /// <param name="regionTarget">the region target</param>
        /// <param name="s">the sender</param>
        /// <param name="e">the NotifyCollectionChangedEventArgs</param>
        private void OnAdd(IRegion region, LayoutGroup regionTarget, object s, NotifyCollectionChangedEventArgs e)
        {
            foreach (var view in e.NewItems)
            {
                var panel = new LayoutPanel { Content = view };
                var panelview = view as IPanelView;
                if (panelview == null)
                {
                    throw new ArgumentException(string.Format("The added view does not respect the interface IPanelView: {0}", view));
                }

                var viewModel = panelview.DataContext as IPanelViewModel;
                if (viewModel == null)
                {
                    throw new ArgumentException(string.Format("The added view-model does not respect the interface IPanelViewModel: {0}", view));
                }

                var captionProperty = new Binding();
                captionProperty.Source = viewModel;
                captionProperty.Path = new PropertyPath("Caption");
                captionProperty.Mode = BindingMode.OneWay;
                captionProperty.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(panel, LayoutPanel.CaptionProperty, captionProperty);

                var tooltipProperty = new Binding();
                tooltipProperty.Source = viewModel;
                tooltipProperty.Path = new PropertyPath("ToolTip");
                tooltipProperty.Mode = BindingMode.OneWay;
                tooltipProperty.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                BindingOperations.SetBinding(panel, LayoutPanel.ToolTipProperty, tooltipProperty);

                regionTarget.Items.Add(panel);
                this.viewPanelPair.Add(panelview, panel);

                regionTarget.SelectedTabIndex = regionTarget.Items.Count - 1;
            }
        }

        /// <summary>
        /// Handles the region.Views.CollectionChanged event when elements have been removed
        /// </summary>
        /// <param name="region">the region</param>
        /// <param name="regionTarget">the region target</param>
        /// <param name="s">the sender</param>
        /// <param name="e">the NotifyCollectionChangedEventArgs</param>
        private void OnRemove(IRegion region, LayoutGroup regionTarget, object s, NotifyCollectionChangedEventArgs e)
        {
            foreach (var view in e.OldItems)
            {
                var panelView = view as IPanelView;
                if (panelView != null)
                {
                    LayoutPanel layoutPanel;
                    if (this.viewPanelPair.TryGetValue(panelView, out layoutPanel))
                    {
                        this.viewPanelPair.Remove(panelView);
                        regionTarget.GetDockLayoutManager().DockController.RemovePanel(layoutPanel);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Dock Panel closing event 
        /// </summary>
        /// <param name="region">The region</param>
        /// <param name="regionTarget">The region target</param>
        /// <param name="s">The sender</param>
        /// <param name="e">The <see cref="ItemCancelEventArgs"/></param>
        private void OnPanelClosing(IRegion region, LayoutGroup regionTarget, object s, ItemCancelEventArgs e)
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
        /// Handles the Dock Panel close event 
        /// </summary>
        /// <param name="region">the region</param>
        /// <param name="regionTarget">the region target</param>
        /// <param name="s">the sender</param>
        /// <param name="e">the NotifyCollectionChangedEventArgs</param>
        private void OnPanelClosed(IRegion region, LayoutGroup regionTarget, object s, DockItemClosedEventArgs e)
        {
            foreach (var panel in e.AffectedItems)
            {
                var docPanel = panel as LayoutPanel;
                if (docPanel == null)
                {
                    continue;
                }

                var view = this.viewPanelPair.SingleOrDefault(x => x.Value == docPanel).Key;
                if (view == null)
                {
                    continue;
                }

                var regionAttribute = view.GetType().GetCustomAttributes(typeof(PanelViewExportAttribute), true).SingleOrDefault() as PanelViewExportAttribute;
                if (regionAttribute != null && regionAttribute.Region == region.Name)
                {
                    this.viewPanelPair.Remove(view);
                    region.Remove(view);
                }
            }
        }
    }
}
