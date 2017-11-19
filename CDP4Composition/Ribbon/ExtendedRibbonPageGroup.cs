// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtendedRibbonPageGroup.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Ribbon
{
    using System.Windows;
    using CDP4Composition.Adapters;
    using DevExpress.Xpf.Ribbon;

    /// <summary>
    /// Extension of the <see cref="RibbonPageGroup"/> that is used by the <see cref="RibbonAdapter"/> to manage the region behavior
    /// </summary>
    public class ExtendedRibbonPageGroup : RibbonPageGroup
    {
        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="ContainerRegionName"/> property
        /// </summary>
        /// <remarks>
        /// The default value of this <see cref="DependencyProperty"/> is <see cref="string.Empty"/>
        /// </remarks>
        public static readonly DependencyProperty ContainerRegionNameProperty = DependencyProperty.Register(
            "ContainerRegionName", 
            typeof(string), 
            typeof(ExtendedRibbonPageGroup), 
            new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the name of the container <see cref="RibbonPage"/> that the current <see cref="ExtendedRibbonPageGroup"/> needs to be added to.
        /// </summary>
        /// <remarks>
        /// The page will only be added by the <see cref="RibbonAdapter"/> if the <see cref="RibbonPage"/> with the specified <see cref="RegionName"/> 
        /// property can be found in the <see cref="RibbonControl"/>
        /// </remarks>
        public string ContainerRegionName
        {
            get
            {
                return (string)GetValue(ContainerRegionNameProperty);
            }
            set
            {
                this.SetValue(ContainerRegionNameProperty, value);
            }
        }
    }
}
