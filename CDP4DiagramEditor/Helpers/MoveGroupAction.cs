// ------------------------------------------------------------------------------------------------
// <copyright file="MoveGroupAction.cs" company="RHEA S.A.">
//   Copyright (c) 2015 RHEA S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

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
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }
        public string TargetPageName
        {
            get { return (string)GetValue(TargetPageNameProperty); }
            set { SetValue(TargetPageNameProperty, value); }
        }

        protected override void ExecuteCore(DependencyObject context)
        {
            var page = CollectionActionHelper.Instance.FindElementByName(context, TargetPageName, this.Container, ScopeSearchSettings.Descendants) as RibbonPage;
            var group = CollectionActionHelper.Instance.FindElementByName(context, GroupName, this.Container, ScopeSearchSettings.Descendants) as RibbonPageGroup;

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
