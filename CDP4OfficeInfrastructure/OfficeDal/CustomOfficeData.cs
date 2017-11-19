// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomOfficeData.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4OfficeInfrastructure.OfficeDal
{
    using System;
    using CDP4Common.MetaInfo;
    using CDP4JsonSerializer;
    using Microsoft.Practices.Prism.Modularity;

    /// <summary>
    /// an abstract super class that shall be derived to store data in office documents
    /// as custom XML parts.
    /// </summary>
    public abstract class CustomOfficeData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomOfficeData"/> class
        /// </summary>
        protected CustomOfficeData()
        {
            this.MetaDataProvider = StaticMetadataProvider.GetMetaDataProvider;
            if (this.MetaDataProvider == null)
            {
                throw new ModuleTypeLoaderNotFoundException("The MetaDataProvider could not be found.");
            }

            this.Serializer = new Cdp4JsonSerializer(this.MetaDataProvider, new Version(1, 0, 0));
        }

        /// <summary>
        /// Gets the <see cref="IMetaDataProvider"/>
        /// </summary>
        protected IMetaDataProvider MetaDataProvider { get; private set; }

        /// <summary>
        /// Gets the <see cref="Cdp4JsonSerializer"/>
        /// </summary>
        protected Cdp4JsonSerializer Serializer { get; private set; }
    }
}
