// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LayoutPanelAdapter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Adapters
{
    using System.ComponentModel.Composition;
    using DevExpress.Xpf.Docking;
    using Microsoft.Practices.Prism.Regions;

    /// <summary>
    /// A region adaptor for the <see cref="LayoutPanel"/>
    /// </summary>
    [Export(typeof(LayoutPanelAdapter)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class LayoutPanelAdapter : RegionAdapterBase<LayoutPanel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LayoutPanelAdapter"/> class.
        /// </summary>
        /// <param name="behaviorFactory">
        /// The behavior factory.
        /// </param>
        [ImportingConstructor]
        public LayoutPanelAdapter(IRegionBehaviorFactory behaviorFactory) :
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
        /// Adapts the <see cref="LayoutPanel"/> in the association <see cref="IRegion"/>
        /// </summary>
        /// <param name="region">
        /// The associated <see cref="IRegion"/>
        /// </param>
        /// <param name="regionTarget">
        /// the control to adapt
        /// </param>
        protected override void Adapt(IRegion region, LayoutPanel regionTarget)
        {
            region.Views.CollectionChanged += (d, e) =>
            {
                if (e.NewItems != null)
                {
                    regionTarget.Content = e.NewItems[0];
                }                
            };
        }
    }
}
