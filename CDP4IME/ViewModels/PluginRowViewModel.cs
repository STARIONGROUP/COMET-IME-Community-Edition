// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2018 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4IME.ViewModels
{
    using System;
    using System.Reflection;
    using CDP4Composition.Attributes;
    using Microsoft.Practices.Prism.Modularity;
    using ReactiveUI;

    /// <summary>
    /// The plugin row view model exposes some literal information on a module.
    /// </summary>
    public class PluginRowViewModel : ReactiveObject
    {
        /// <summary>
        /// The backing field for the <see cref="AssemblyName"/> property
        /// </summary>
        private string assemblyName;

        /// <summary>
        /// The backing field for the <see cref="Name"/> property
        /// </summary>
        private string name;

        /// <summary>
        /// The backing field for the <see cref="Description"/> property
        /// </summary>
        private string description;

        /// <summary>
        /// The backing field for the <see cref="Version"/> property
        /// </summary>
        private string version;

        /// <summary>
        /// The backing field for the <see cref="Company"/> property
        /// </summary>
        private string company;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginRowViewModel"/> class.
        /// </summary>
        /// <param name="module">
        /// The <see cref="IModule"/> to extract information from.
        /// </param>
        public PluginRowViewModel(IModule module)
        {
            this.AssemblyName =
                ((AssemblyTitleAttribute)module.GetType().Assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute)))
                    .Title;

            var nameAttribute = (ModuleExportNameAttribute)Attribute.GetCustomAttribute(module.GetType(), typeof(ModuleExportNameAttribute));

            this.Name = nameAttribute == null ? string.Empty : nameAttribute.Name;
            this.Description =
                ((AssemblyDescriptionAttribute)
                    module.GetType().Assembly.GetCustomAttribute(typeof(AssemblyDescriptionAttribute))).Description;
            this.Version = ((AssemblyFileVersionAttribute)module.GetType().Assembly.GetCustomAttribute(typeof(AssemblyFileVersionAttribute))).Version;
            this.Company = ((AssemblyCompanyAttribute)module.GetType().Assembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute))).Company;
        }

        /// <summary>
        /// Gets or sets the assembly name of the Plugin
        /// </summary>
        public string AssemblyName
        {
            get
            {
                return this.assemblyName;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.assemblyName, value);
            }
        }

        /// <summary>
        /// Gets or sets the name of the Plugin
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.name, value);
            }
        }

        /// <summary>
        /// Gets or sets the description of the Plugin
        /// </summary>
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.description, value);
            }
        }

        /// <summary>
        /// Gets or sets the version of the Plugin
        /// </summary>
        public string Version
        {
            get
            {
                return this.version;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.version, value);
            }
        }

        /// <summary>
        /// Gets or sets the company
        /// </summary>
        public string Company
        {
            get
            {
                return this.company;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.company, value);
            }
        }
    }
}