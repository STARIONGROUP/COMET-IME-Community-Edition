// -------------------------------------------------------------------------------------------------
// <copyright file="ScriptingEngineRibbonPageGroup.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Views
{
    using System.ComponentModel.Composition;
    using CDP4Composition.Navigation;
    using CDP4Composition.Ribbon;
    using Interfaces;
    using Microsoft.Practices.Prism.Mvvm;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ScriptingEngineRibbonPageGroup.xaml
    /// </summary>
    [Export(typeof(ScriptingEngineRibbonPageGroup))]
    public partial class ScriptingEngineRibbonPageGroup : ExtendedRibbonPageGroup, IView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingEngineRibbonPageGroup"/> class
        /// </summary>       
        [ImportingConstructor]
        public ScriptingEngineRibbonPageGroup(IPanelNavigationService panelNavigationService, IOpenSaveFileDialogService fileDialogService, IScriptingProxy scriptingProxy)
        {
            this.InitializeComponent();
            this.DataContext = new ScriptingEngineRibbonPageGroupViewModel(panelNavigationService, fileDialogService, scriptingProxy);
        }
    }
}
