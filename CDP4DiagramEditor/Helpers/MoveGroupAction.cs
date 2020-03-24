// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MoveGroupAction.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru, Nathanael Smiechowski.
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
    using System.Windows;
    using DevExpress.Xpf.Bars;
    using DevExpress.Xpf.Bars.Native;
    using DevExpress.Xpf.Ribbon;

    /// <summary>
    /// A custom action for moving a RibbonGroup from one RibbonPage to another
    /// </summary>
    public class MoveGroupAction : BarManagerControllerActionBase
    {
        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register("GroupName", typeof(string), typeof(MoveGroupAction), null);
        public static readonly DependencyProperty TargetPageNameProperty = DependencyProperty.Register("TargetPageName", typeof(string), typeof(MoveGroupAction), null);

        public string GroupName
        {
            get { return (string) this.GetValue(GroupNameProperty); }
            set { this.SetValue(GroupNameProperty, value); }
        }
        public string TargetPageName
        {
            get { return (string) this.GetValue(TargetPageNameProperty); }
            set { this.SetValue(TargetPageNameProperty, value); }
        }

        protected override void ExecuteCore(DependencyObject context)
        {
            var page = CollectionActionHelper.Instance.FindElementByName(context, this.TargetPageName, this.Container, ScopeSearchSettings.Descendants) as RibbonPage;
            var group = CollectionActionHelper.Instance.FindElementByName(context, this.GroupName, this.Container, ScopeSearchSettings.Descendants) as RibbonPageGroup;

            if (group == null || page == null || group.Page == null)
            {
                return;
            }

            group.Page.Groups.Remove(group);
            page.Groups.Add(group);
        }

        public override object GetObjectCore()
        {
            return null;
        }
    }
}
