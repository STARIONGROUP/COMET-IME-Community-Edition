// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtendedRibbonPage.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Ribbon
{
    using System.Windows;
    using CDP4Composition.Adapters;
    using DevExpress.Xpf.Ribbon;

    /// <summary>
    /// Extension of the <see cref="RibbonPage"/> that is used by the <see cref="RibbonAdapter"/> to manage the region behavior
    /// </summary>
    public class ExtendedRibbonPage : RibbonPage
    {
        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="IsInDefaultPageCategory"/>
        /// </summary>
        /// <remarks>
        /// The default value of this <see cref="DependencyProperty"/> is true
        /// </remarks>
        public static readonly DependencyProperty IsInDefaultPageCategoryProperty = DependencyProperty.Register(
            "IsInDefaultPageCategory",
            typeof(bool),
            typeof(ExtendedRibbonPage),
            new PropertyMetadata(true));

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="CustomPageCategoryName"/>
        /// </summary>
        /// <remarks>
        /// The default value of this <see cref="DependencyProperty"/> is "default"
        /// </remarks>
        public static readonly DependencyProperty CustomPageCategoryNameProperty = DependencyProperty.Register(
            "CustomPageCategoryName", 
            typeof(string), 
            typeof(ExtendedRibbonPage), 
            new PropertyMetadata("default"));

        /// <summary>
        /// The declaration of the <see cref="DependencyProperty"/> that is accessible via the <see cref="RegionName"/>
        /// </summary>
        /// <remarks>
        /// The default value of this <see cref="DependencyProperty"/> is <see cref="string.Empty"/>
        /// </remarks>
        public static readonly DependencyProperty RegionNameProperty = DependencyProperty.Register(
            "RegionName",
            typeof(string),
            typeof(ExtendedRibbonPage),
            new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets a value indicating whether the current group should be added to the <see cref="RibbonDefaultPageCategory"/>
        /// </summary>
        /// <remarks>
        /// if the property is true, then the <see cref="ExtendedRibbonPage"/> will be added to the <see cref="RibbonDefaultPageCategory"/>,
        /// otherwise the page will be added to a new <see cref="RibbonPageCategory"/>
        /// </remarks>
        public bool IsInDefaultPageCategory
        {
            get
            {
                return (bool)GetValue(IsInDefaultPageCategoryProperty);
            }

            set
            {
                this.SetValue(IsInDefaultPageCategoryProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the <see cref="RibbonPageCategory"/> that the current <see cref="ExtendedRibbonPage"/> needs to be added to.
        /// </summary>
        /// <remarks>
        /// The page will only be added by the <see cref="RibbonAdapter"/> if the <see cref="RibbonPageCategory"/> with the specified name can be found
        /// in the <see cref="RibbonControl"/>
        /// </remarks>
        public string CustomPageCategoryName
        {
            get
            {
                return (string)GetValue(CustomPageCategoryNameProperty);
            }

            set
            {
                this.SetValue(CustomPageCategoryNameProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the Region of the current <see cref="ExtendedRibbonPage"/>. Controls that need to be added to this region
        /// shall specify this name as their "ContainerRegion"
        /// </summary>
        public string RegionName
        {
            get
            {
                return (string)GetValue(RegionNameProperty);
            }

            set
            {
                this.SetValue(RegionNameProperty, value);
            }
        }
    }
}
