// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DiagramControlContextMenuViewModel.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Kamil Wojnowski
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

namespace CDP4Grapher.ViewModels
{
    using System.Collections.Generic;
    using System;
    using System.Reactive.Linq;
    using System.Windows.Input;

    using CDP4Grapher.Behaviors;
    using CDP4Grapher.Utilities;

    using DevExpress.Diagram.Core;
    using DevExpress.Diagram.Core.Layout;
    using DevExpress.Xpf.Bars;

    using ReactiveUI;

    using Direction = DevExpress.Diagram.Core.Direction;

    /// <summary>
    /// Responsible for creating a custom DevExpress Diagram Control Context Menu Applying actions on a <see cref="IGrapherOrgChartBehavior"/>
    /// </summary>
    public class DiagramControlContextMenuViewModel : ReactiveObject, IHaveContextMenu
    {
        /// <summary>
        /// Backing field for <see cref="CanExportDiagram"/>
        /// </summary>
        private bool canExportDiagram = true;

        /// <summary>
        /// Backing field for <see cref="CanExitIsolation"/>
        /// </summary>
        private bool canExitIsolation;
        
        /// <summary>
        /// Backing field for the <see cref="HoveredElement"/> property
        /// </summary>
        private GraphElementViewModel hoveredElement;

        /// <summary>
        /// Gets or sets the under the mouse element <see cref="GraphElementViewModel"/>
        /// </summary>
        public GraphElementViewModel HoveredElement
        {
            get => this.hoveredElement;
            set => this.RaiseAndSetIfChanged(ref this.hoveredElement, value);
        }
        
        /// <summary>
        /// Gets or sets the attached behavior
        /// </summary>
        public IGrapherOrgChartBehavior Behavior { get; set; }

        /// <summary>
        /// Holds the <see cref="BarButtonItem"/> and <see cref="BarSubItem"/> representing an overridable diagram Context Menu
        /// </summary>
        public List<IBarManagerControllerAction> ContextMenu { get; set; } = new List<IBarManagerControllerAction>();
        
        /// <summary>
        /// Gets a value indicating whether the diagram can be exported
        /// </summary>
        public bool CanExportDiagram
        {
            get => this.canExportDiagram;
            private set => this.RaiseAndSetIfChanged(ref this.canExportDiagram, value);
        }
        
        /// <summary>
        /// Gets a value indicating whether the user can exit Isolation
        /// </summary>
        public bool CanExitIsolation
        {
            get => this.canExitIsolation;
            private set => this.RaiseAndSetIfChanged(ref this.canExitIsolation, value);
        }
        
        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to Isolate the <see cref="HoveredElement"/>
        /// </summary>
        public ReactiveCommand<object> IsolateCommand { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to exit isolation
        /// </summary>
        public ReactiveCommand<object> ExitIsolationCommand { get; private set; }
        
        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to export the generated diagram as png
        /// </summary>
        public ReactiveCommand<object> ExportGraphAsJpg { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> to export the generated diagram as pdf
        /// </summary>
        public ReactiveCommand<object> ExportGraphAsPdf { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the circular layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyCircularLayout { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Fugiyama Right to Left layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyFugiyamaLayoutRightToLeft { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Fugiyama Left to Right layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyFugiyamaLayoutLeftToRight { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Fugiyama Top to Bottom layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyFugiyamaLayoutTopToBottom { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Fugiyama Bottom to Top layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyFugiyamaLayoutBottomToTop { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Tree view Right to Left layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyTreeViewLayoutRightToLeft { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Tree view Left to Right layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyTreeViewLayoutLeftToRight { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Tree view Top to Bottom layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyTreeViewLayoutTopToBottom { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Tree view Bottom to Top layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyTreeViewLayoutBottomToTop { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Tip over Left to Right layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyTipOverLayoutLeftToRight { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Tip over Right to Left layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyTipOverLayoutRightToLeft { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Mind map layout from bottom to top to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyMindMapLayoutBottomToTop { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Mind map layout from left to right to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyMindMapLayoutLeftToRight { get; private set; }

        /// <summary>
        /// Gets the <see cref="ReactiveCommand"/> that apply the Org chart layout to the diagram elements
        /// </summary>
        public ReactiveCommand<object> ApplyOrganisationalChartLayout { get; private set; }

        /// <summary>
        /// Instiate a new <see cref="DiagramControlContextMenuViewModel"/>
        /// </summary>
        public DiagramControlContextMenuViewModel()
        {
            this.InitializeCommands();
            this.CreateContextMenu();
        }

        /// <summary>
        /// Initialize the <see cref="ReactiveCommand"/>
        /// </summary>
        private void InitializeCommands()   
        {
            this.IsolateCommand = ReactiveCommand.Create(
                this.WhenAnyValue(x => x.HoveredElement)
                    .Select(x => x != null).ObserveOn(RxApp.MainThreadScheduler));

            this.IsolateCommand.Subscribe(_ => this.ExecuteIsolate());

            this.ExitIsolationCommand = ReactiveCommand.Create(
                this.WhenAnyValue(x => x.CanExitIsolation)
                    .ObserveOn(RxApp.MainThreadScheduler));

            this.ExitIsolationCommand.Subscribe(_ => this.ExecuteExitIsolation());

            this.ExportGraphAsJpg = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanExportDiagram).ObserveOn(RxApp.MainThreadScheduler));
            this.ExportGraphAsJpg.Subscribe(_ => this.ExecuteExportGraphAsPng());
            this.ExportGraphAsPdf = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanExportDiagram).ObserveOn(RxApp.MainThreadScheduler));
            this.ExportGraphAsPdf.Subscribe(_ => this.ExecuteExportGraphAsPdf());

            this.ApplyFugiyamaLayoutLeftToRight = ReactiveCommand.Create();
            this.ApplyFugiyamaLayoutLeftToRight.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.Fugiyama, Direction.Right));
            this.ApplyFugiyamaLayoutRightToLeft = ReactiveCommand.Create();
            this.ApplyFugiyamaLayoutRightToLeft.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.Fugiyama, Direction.Left));
            this.ApplyFugiyamaLayoutTopToBottom = ReactiveCommand.Create();
            this.ApplyFugiyamaLayoutTopToBottom.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.Fugiyama, Direction.Down));
            this.ApplyFugiyamaLayoutBottomToTop = ReactiveCommand.Create();
            this.ApplyFugiyamaLayoutBottomToTop.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.Fugiyama, Direction.Up));

            this.ApplyTreeViewLayoutLeftToRight = ReactiveCommand.Create();
            this.ApplyTreeViewLayoutLeftToRight.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.TreeView, LayoutDirection.LeftToRight));
            this.ApplyTreeViewLayoutRightToLeft = ReactiveCommand.Create();
            this.ApplyTreeViewLayoutRightToLeft.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.TreeView, LayoutDirection.RightToLeft));
            this.ApplyTreeViewLayoutTopToBottom = ReactiveCommand.Create();
            this.ApplyTreeViewLayoutTopToBottom.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.TreeView, LayoutDirection.TopToBottom));
            this.ApplyTreeViewLayoutBottomToTop = ReactiveCommand.Create();
            this.ApplyTreeViewLayoutBottomToTop.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.TreeView, LayoutDirection.BottomToTop));

            this.ApplyMindMapLayoutBottomToTop = ReactiveCommand.Create();
            this.ApplyMindMapLayoutBottomToTop.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.MindMap, OrientationKind.Vertical));
            this.ApplyMindMapLayoutLeftToRight = ReactiveCommand.Create();
            this.ApplyMindMapLayoutLeftToRight.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.MindMap, OrientationKind.Horizontal));

            this.ApplyTipOverLayoutLeftToRight = ReactiveCommand.Create();
            this.ApplyTipOverLayoutLeftToRight.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.TipOver, TipOverDirection.LeftToRight));
            this.ApplyTipOverLayoutRightToLeft = ReactiveCommand.Create();
            this.ApplyTipOverLayoutRightToLeft.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.TipOver, TipOverDirection.RightToLeft));

            this.ApplyOrganisationalChartLayout = ReactiveCommand.Create();
            this.ApplyOrganisationalChartLayout.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.OrganisationalChart));

            this.ApplyCircularLayout = ReactiveCommand.Create();
            this.ApplyCircularLayout.Subscribe(_ => this.Behavior.ApplySpecifiedLayout(LayoutEnumeration.Circular));
        }
        
        /// <summary>
        /// Fills up the <see cref="ContextMenu"/> with <see cref="BarButtonItem"/> and <see cref="BarSubItem"/>
        /// </summary>
        private void CreateContextMenu()
        {
            this.ContextMenu.Add(this.GenerateContextMenuItem<BarButtonItem>("Isolate", this.IsolateCommand, "Snap/SeparatorListNone.svg"));
            this.ContextMenu.Add(this.GenerateContextMenuItem<BarButtonItem>("Exit Isolation", this.ExitIsolationCommand, "Icon Builder/Actions_RemoveCircled.svg"));
            this.ContextMenu.Add(this.GenerateContextMenuItem<BarButtonItem>("Export Graph as JPG", this.ExportGraphAsJpg, "XAF/Action_Export_ToImage.svg"));
            this.ContextMenu.Add(this.GenerateContextMenuItem<BarButtonItem>("Export Graph as PDF", this.ExportGraphAsPdf, "Export/ExportToPDF.svg"));
            var layoutSubItem = this.GenerateContextMenuItem<BarSubItem>("Apply layouts", null, "DiagramIcons/Direction/re-layout.svg");

            var treeSubItem = this.GenerateContextMenuItem<BarSubItem>("Tree view", null, "DiagramIcons/Direction/re-layout.svg");
            treeSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Top to Bottom", this.ApplyTreeViewLayoutTopToBottom, "DiagramIcons/Direction/direction1.svg"));
            treeSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Bottom to Top", this.ApplyTreeViewLayoutBottomToTop, "DiagramIcons/Direction/direction2.svg"));
            treeSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Left to Right", this.ApplyTreeViewLayoutLeftToRight, "DiagramIcons/Direction/direction4.svg"));
            treeSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Right to Left", this.ApplyTreeViewLayoutRightToLeft, "DiagramIcons/Direction/direction3.svg"));
            layoutSubItem.Items.Add(treeSubItem);

            var fugiyamaSubItem = this.GenerateContextMenuItem<BarSubItem>("Fugiyama", null, "DiagramIcons/Direction/re-layout.svg");
            fugiyamaSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Top to Bottom", this.ApplyFugiyamaLayoutTopToBottom, "DiagramIcons/Direction/direction1.svg"));
            fugiyamaSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Bottom to Top", this.ApplyFugiyamaLayoutBottomToTop, "DiagramIcons/Direction/direction2.svg"));
            fugiyamaSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Left to Right", this.ApplyFugiyamaLayoutLeftToRight, "DiagramIcons/Direction/direction4.svg"));
            fugiyamaSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Right to Left", this.ApplyFugiyamaLayoutRightToLeft, "DiagramIcons/Direction/direction3.svg"));
            layoutSubItem.Items.Add(fugiyamaSubItem);

            var tipOverSubItem = this.GenerateContextMenuItem<BarSubItem>("Tip Over", null, "DiagramIcons/Direction/re-layout.svg");
            tipOverSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Left to Right", this.ApplyTipOverLayoutLeftToRight, "DiagramIcons/Direction/direction4.svg"));
            tipOverSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Right to Left", this.ApplyTipOverLayoutRightToLeft, "DiagramIcons/Direction/direction3.svg"));
            layoutSubItem.Items.Add(tipOverSubItem);

            var mindMapSubItem = this.GenerateContextMenuItem<BarSubItem>("Mind Map", null, "DiagramIcons/Direction/re-layout.svg");
            mindMapSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Left to Right", this.ApplyMindMapLayoutLeftToRight, "DiagramIcons/Direction/direction4.svg"));
            mindMapSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Bottom to Top", this.ApplyMindMapLayoutBottomToTop, "DiagramIcons/Direction/direction3.svg"));
            layoutSubItem.Items.Add(mindMapSubItem);

            layoutSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Organisational Chart", this.ApplyOrganisationalChartLayout, "DiagramIcons/Direction/re-layout.svg"));
            layoutSubItem.Items.Add(this.GenerateContextMenuItem<BarButtonItem>("Circular", this.ApplyCircularLayout, "DiagramIcons/Direction/re-layout.svg"));
            
            this.ContextMenu.Add(layoutSubItem);
        }

        /// <summary>
        /// Instanciate a new <see cref="BarItem"/> 
        /// </summary>
        /// <typeparam name="T">A <see cref="BarItem"/> type to return</typeparam>
        /// <param name="text">the text that will be displayed next to the <see cref="icon"/></param>
        /// <param name="command">the <see cref="ICommand"/> to be executed on click</param>
        /// <param name="icon">the path or partial path to an image resource</param>
        /// <returns>A BarItem that is use to create a custom context menu</returns>
        private T GenerateContextMenuItem<T>(string text, ICommand command, string icon) where T : BarItem, new()
        {
            return new T
            {
                DataContext = text,
                Description = text,
                ToolTip = text,
                Hint = text,
                Content = text,
                Command = command,
                Glyph = SvgHelper.ToImageSource(icon)
            };
        }
        
        /// <summary>
        /// Executes the <see cref="ExportGraphAsJpg"/> 
        /// </summary>
        private void ExecuteExportGraphAsPdf()
        {
            this.canExportDiagram = false;
            this.Behavior.ExportGraph(DiagramExportFormat.PDF);
            this.canExportDiagram = true;
        }

        /// <summary>
        /// Executes the <see cref="ExportGraphAsJpg"/> 
        /// </summary>
        private void ExecuteExportGraphAsPng()
        {
            this.canExportDiagram = false;
            this.Behavior.ExportGraph(DiagramExportFormat.JPEG);
            this.canExportDiagram = true;
        }

        /// <summary>
        /// Executes the <see cref="IsolateCommand"/>
        /// </summary>
        private void ExecuteIsolate()
        {
            this.CanExitIsolation = this.Behavior.Isolate();
        }

        /// <summary>
        /// Executes the <see cref="ExitIsolationCommand"/>
        /// </summary>
        private void ExecuteExitIsolation()
        {
            this.Behavior.ExitIsolation();
            this.CanExitIsolation = false;
        }
    }
}
