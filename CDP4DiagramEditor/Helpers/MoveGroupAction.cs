// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoveGroupAction.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski, Kamil Wojnowski.
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4DiagramEditor.Helpers
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    using CDP4Composition;
    using CDP4Composition.Ribbon;
    
    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Bars.Native;
    using DevExpress.Xpf.Ribbon;

    using Microsoft.Practices.Prism.Regions;
    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// A custom action for moving a RibbonGroup from one RibbonPage to another
    /// </summary>
    public class MoveGroupAction : BarManagerControllerActionBase
    {
        /// <summary>
        /// <see cref="DependencyProperty"/> that can be set in XAML to specify what group to move
        /// </summary>
        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(MoveGroupAction), null);

        /// <summary>
        /// <see cref="DependencyProperty"/> that can be set in XAML to specify the target page caption
        /// </summary>
        public static readonly DependencyProperty TargetPageCaptionProperty = DependencyProperty.Register("TargetPageCaption", typeof(string), typeof(MoveGroupAction), null);

        /// <summary>
        /// <para>Gets or sets the target <see cref="DevExpress.Xpf.Ribbon.RibbonPageGroup.Name"/> to be moved</para>
        /// <para>This is a <see cref="DependencyProperty"/>.</para>
        /// </summary>
        public string GroupName
        {
            get => (string) this.GetValue(GroupNameProperty);
            set => this.SetValue(GroupNameProperty, value);
        }

        /// <summary>
        /// <para>Gets or sets the target <see cref="DevExpress.Xpf.Ribbon.RibbonPage.Caption"/> to be moved</para>
        /// <para>This is a <see cref="DependencyProperty"/>.</para>
        /// </summary>
        public string TargetPageCaption
        {
            get => (string) this.GetValue(TargetPageCaptionProperty);
            set => this.SetValue(TargetPageCaptionProperty, value);
        }

        /// <summary>
        /// the <see cref="IRegion"/> property that is used to get the <see cref="RibbonPage"/> to move
        /// </summary>
        public IRegion RibbonRegion { get; set; }

        /// <summary>
        /// the <see cref="IRegionManager"/> property that is used to get the <see cref="RibbonRegion"/>
        /// </summary>
        public IRegionManager RegionManager { get; set; }
        
        /// <summary>
        /// Constructor of <see cref="MoveGroupAction"/>
        /// Resolve <see cref="IRegionManager"/>
        /// And Sets the <see cref="RibbonRegion"/> Property
        /// </summary>
        public MoveGroupAction()
        {
            this.RegionManager = ServiceLocator.Current.GetInstance<IRegionManager>();
            this.RibbonRegion = this.RegionManager?.Regions.FirstOrDefault(region => region.Name == RegionNames.RibbonRegion);
        }

        /// <summary>
        /// The main logic that is being executed when this <see cref="MoveGroupAction"/> action is used
        /// </summary>
        /// <param name="context"></param>
        protected override void ExecuteCore(DependencyObject context)
        {
            var page = this.RibbonRegion?.Views.OfType<ExtendedRibbonPage>().FirstOrDefault(rb => (string)rb.Caption == this.TargetPageCaption);
            var group = CollectionActionHelper.Instance.FindElementByName(VisualTreeHelper.GetParent(context), this.GroupName, this.Container, ScopeSearchSettings.Descendants) as RibbonPageGroup;
            
            if (group == null || page == null || group.Page == null || group.Page.Caption == page.Caption)
            {
                return;
            }

            group.Page.Groups.Remove(group);

            if (page.Groups.All(gr => gr.Caption != group.Caption))
            {
                page.Groups.Add(group);
            }
        }

        /// <summary>
        /// Returns an object that is manipulated by the <see cref="MoveGroupAction"/>
        /// </summary>
        /// <returns></returns>
        public override object GetObjectCore()
        {
            return null;
        }
    }
}
