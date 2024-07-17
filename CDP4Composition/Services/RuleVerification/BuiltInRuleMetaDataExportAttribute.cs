// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BuiltInRuleMetaDataExportAttribute.cs" company="Starion Group S.A.">
//   Copyright (c) 2015 Starion Group S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System;
    using System.ComponentModel.Composition;
    using CDP4Common.EngineeringModelData;

    /// <summary>
    /// Export attribute that is used to decorate classes that implement <see cref="IBuiltInRuleMetaData"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false), MetadataAttribute]
    public class BuiltInRuleMetaDataExportAttribute : ExportAttribute, IBuiltInRuleMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltInRuleMetaDataExportAttribute"/> class.
        /// </summary>
        /// <param name="author">
        /// The author of the <see cref="BuiltInRuleVerification"/>.
        /// </param>
        /// <param name="name">
        /// The name of the <see cref="BuiltInRuleVerification"/>.
        /// </param>
        /// <param name="description">
        /// The description of the <see cref="BuiltInRuleVerification"/>.
        /// </param>
        public BuiltInRuleMetaDataExportAttribute(string author, string name, string description)
            : base(typeof(IBuiltInRule))
        {
            this.Author = author;
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets the human readable name of the author of the exported <see cref="BuiltInRuleVerification"/>
        /// </summary>
        public string Author { get; private set; }

        /// <summary>
        /// Gets the human readable name of the exported <see cref="BuiltInRuleVerification"/>
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the human readable description of the exported <see cref="BuiltInRuleVerification"/>
        /// </summary>
        public string Description { get; private set; }
    }
}
