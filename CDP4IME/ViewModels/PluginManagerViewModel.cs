// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginManagerViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// <summary>
//   The purpose of the  is to allow a user to select an  implementation
//   and provide Credentials to login to a data source.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using System;
    using CDP4Composition.Navigation;
    using Microsoft.Practices.Prism.Modularity;
    using Microsoft.Practices.ServiceLocation;
    using ReactiveUI;

    /// <summary>
    /// The purpose of the <see cref="PluginManagerViewModel"/> is to allow a user to inspect
    /// and interact with MEF loaded modules
    /// </summary>
    public class PluginManagerViewModel : DialogViewModelBase
    {
        /// <summary>
        /// The <see cref="ReactiveList{T}"/> of <see cref="PluginRowViewModel"/>s contained withing this view.
        /// </summary>
        private ReactiveList<PluginRowViewModel> plugins;

        /// <summary>
        /// The selected <see cref="PluginRowViewModel"/>
        /// </summary>
        private PluginRowViewModel selectedPlugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginManagerViewModel"/> class.
        /// </summary>
        public PluginManagerViewModel()
        {
            this.Plugins = new ReactiveList<PluginRowViewModel>();

            this.CloseCommand = ReactiveCommand.Create();
            this.CloseCommand.Subscribe(_ => this.ExecuteClose());

            this.PopulateModuleList();
        }
        
        /// <summary>
        /// Gets the Close Command
        /// </summary>
        public ReactiveCommand<object> CloseCommand { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ReactiveList{T}"/> of <see cref="PluginRowViewModel"/>s contained withing this view.
        /// </summary>
        public ReactiveList<PluginRowViewModel> Plugins
        {
            get
            {
                return this.plugins;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.plugins, value);
            }
        }

        /// <summary>
        /// Gets or sets the selected <see cref="PluginRowViewModel"/>
        /// </summary>
        public PluginRowViewModel SelectedPlugin
        {
            get
            {
                return this.selectedPlugin;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.selectedPlugin, value);
            }
        }

        /// <summary>
        /// Executes the Close Command
        /// </summary>
        private void ExecuteClose()
        {
            this.DialogResult = new BaseDialogResult(false);
        }

        /// <summary>
        /// Populates the list of modules
        /// </summary>
        private void PopulateModuleList()
        {
            var modules = ServiceLocator.Current.GetAllInstances(typeof(IModule));
            this.Plugins.Clear();
            foreach (var module in modules)
            {
                this.Plugins.Add(new PluginRowViewModel((IModule)module));
            }
        }
    }
}