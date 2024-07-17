﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UriRowViewModel.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4ShellDialogs.ViewModels
{
    using System;
    using CDP4Composition.Utilities;
    using CDP4Dal.Composition;
    using ReactiveUI;

    /// <summary>
    /// The uri row view model exposes the literal configuration uri information.
    /// </summary>
    public class UriRowViewModel : ReactiveObject
    {
        /// <summary>
        /// The backing field for the <see cref="Alias"/> property
        /// </summary>
        private string alias;

        /// <summary>
        /// The backing field for the <see cref="Uri"/> property
        /// </summary>
        private string uri;
        
        /// <summary>
        /// The backing field for the <see cref="DalType"/> property
        /// </summary>
        private DalType dalType;
        
        /// <summary>
        /// Gets or sets the <see cref="UriConfig"/> of the Row
        /// </summary>        
        public UriConfig UriConfig
        {
            get
            {
                var uriConfig = new UriConfig
                {
                    Alias = this.alias,
                    Uri = this.uri,
                    DalType = this.dalType.ToString()
                };
                return uriConfig;
            }

            set
            {
                this.Alias = value.Alias;
                this.Uri = value.Uri;
                this.DalType = (DalType) Enum.Parse(typeof(DalType), value.DalType, true);
            }
        }

        /// <summary>
        /// Gets or sets the name of the uri
        /// </summary>
        public string Name
        {
            get
            {
                return !string.IsNullOrEmpty(this.Alias) ? this.Alias : this.Uri;
            }
        }

        /// <summary>
        /// Gets or sets the name of the uri
        /// </summary>
        public string Alias
        {
            get
            {
                return this.alias;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.alias, value);
            }
        }

        /// <summary>
        /// Gets or sets the value of the uri
        /// </summary>
        public string Uri
        {
            get
            {
                return this.uri;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.uri, value);
            }
        }
        
        /// <summary>
        /// Gets or sets the <see cref="DalType"/> of the uri
        /// </summary>
        public DalType DalType
        {
            get
            {
                return this.dalType;
            }

            set
            {
                this.RaiseAndSetIfChanged(ref this.dalType, value);
            }
        }
    }
}