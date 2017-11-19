// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IdentifiableCustomTaskPane.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Addin
{
    using System;
    using NetOffice.OfficeApi;

    /// <summary>
    /// The purpose of the <see cref="IdentifiableCustomTaskPane"/> is to Decorate the <see cref="_CustomTaskPane"/>
    /// with extra properties
    /// </summary>
    public class IdentifiableCustomTaskPane : IDisposable
    {
        public IdentifiableCustomTaskPane(Guid identifier, _CustomTaskPane customTaskPane)
        {
            this.Identifier = identifier;
            this.CustomTaskPane = customTaskPane;
        }

        /// <summary>
        /// Gets the unique identifier of the <see cref="IdentifiableCustomTaskPane"/>
        /// </summary>
        public Guid Identifier { get; private set; }

        /// <summary>
        /// Gets the <see cref="_CustomTaskPane"/> that is decorated by the <see cref="IdentifiableCustomTaskPane"/>
        /// </summary>
        public _CustomTaskPane CustomTaskPane { get; private set; }

        /// <summary>
        /// Dispose of the decorated <see cref="_CustomTaskPane"/>
        /// </summary>
        public void Dispose()
        {
            this.CustomTaskPane.Dispose();
        }
    }
}
