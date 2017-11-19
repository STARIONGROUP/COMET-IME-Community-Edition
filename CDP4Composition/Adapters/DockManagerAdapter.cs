// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DockManagerAdapter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Adapters
{
    using System.ComponentModel.Composition;
    using System.Windows.Controls;
    using DevExpress.Xpf.Docking;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// A region adaptor for the <see cref="DockLayoutManager"/>
    /// </summary>
    [Export(typeof(DockManagerAdapter)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class DockManagerAdapter : RegionAdapterBase<DockLayoutManager>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DockManagerAdapter"/> class.
        /// </summary>
        /// <param name="behaviorFactory">
        /// The behavior factory.
        /// </param>
        [ImportingConstructor]
        public DockManagerAdapter(IRegionBehaviorFactory behaviorFactory) :
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
            return new SingleActiveRegion();
        }

        /// <summary>
        /// Adapts the <see cref="DockLayoutManager"/> in the association <see cref="IRegion"/>
        /// </summary>
        /// <param name="region">
        /// The associated <see cref="IRegion"/>
        /// </param>
        /// <param name="regionTarget">
        /// the control to adapt
        /// </param>
        protected override void Adapt(IRegion region, DockLayoutManager regionTarget)
        {
            BaseLayoutItem[] items = regionTarget.GetItems();
            foreach (BaseLayoutItem item in items)
            {
                string regionName = RegionManager.GetRegionName(item);
                
                if (!string.IsNullOrEmpty(regionName))
                {
                    var panel = item as LayoutPanel;
                    if (panel != null && panel.Content == null)
                    {
                        var control = new ContentControl();
                        RegionManager.SetRegionName(control, regionName);
                        panel.Content = control;
                    }
                }
            }
        }
    }
}
