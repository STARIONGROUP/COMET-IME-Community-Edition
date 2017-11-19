// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentationAttribute.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Attributes
{
    using System;
    using CDP4Composition.Attributes;
    using Helpers;

    /// <summary>
    /// The purporse of this class is to decorate the commands possible in the scripting engine. 
    /// The command <see cref="ScriptingProxy.Help"/> browses all the commands decorated and displays them to the output.
    /// </summary>
    public class DocumentationAttribute : Attribute, IDescribeMetaData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of a command.</param>
        /// <param name="description">The description of a command.</param>
        public DocumentationAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }

        /// <summary>
        /// Gets the name of the command.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        public string Description { get; private set; }
    }
}