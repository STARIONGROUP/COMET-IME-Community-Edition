// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabbedGroupAdapter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Adapters
{
    using System.Collections.Specialized;
    using System.ComponentModel.Composition;
    using DevExpress.Xpf.Docking;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// A region adaptor for the <see cref="TabbedGroup"/>
    /// </summary>
    [Export(typeof(TabbedGroupAdapter)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class TabbedGroupAdapter : RegionAdapterBase<TabbedGroup>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TabbedGroupAdapter"/> class.
        /// </summary>
        /// <param name="behaviorFactory">
        /// The behavior factory.
        /// </param>
        [ImportingConstructor]
        public TabbedGroupAdapter(IRegionBehaviorFactory behaviorFactory) :
            base(behaviorFactory)
        {
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
        /// Adapts the <see cref="TabbedGroup"/> in the association <see cref="IRegion"/>
        /// </summary>
        /// <param name="region">
        /// The associated <see cref="IRegion"/>
        /// </param>
        /// <param name="regionTarget">
        /// the control to adapt
        /// </param>
        protected override void Adapt(IRegion region, TabbedGroup regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                OnViewsCollectionChanged(region, regionTarget, s, e);
            };
        }

        void OnViewsCollectionChanged(IRegion region, TabbedGroup regionTarget, object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object view in e.NewItems)
                {
                    LayoutPanel panel = new LayoutPanel();
                    panel.Content = view;
                    panel.Caption = "new Page";
                    regionTarget.Items.Add(panel);
                    regionTarget.SelectedTabIndex = regionTarget.Items.Count - 1;
                }
            }
        }
    }
}
