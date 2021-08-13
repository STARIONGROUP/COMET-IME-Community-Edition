// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MenuItemKind.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    /// <summary>
    /// An enumeration data type used to identify the kind of command associated to a menu-item
    /// </summary>
    public enum MenuItemKind
    {
        /// <summary>
        /// Assertion that the associated menu item is not any of the other known enumeration value definitions.
        /// </summary>
        /// <remarks>
        /// None is the default enumeration value.
        /// </remarks>
        None = 0,

        /// <summary>
        /// Assertion that the associated menu item is used to create.
        /// </summary>
        Create = 1,

        /// <summary>
        /// Assertion that the associated menu item is used to edit.
        /// </summary>
        Edit = 2,

        /// <summary>
        /// Assertion that the associated menu item is used to inspect.
        /// </summary>
        Inspect = 3,

        /// <summary>
        /// Assertion that the associated menu item is used to Delete.
        /// </summary>
        Delete = 4,

        /// <summary>
        /// Assertion that the associated menu item is used to deprecate.
        /// </summary>
        Deprecate = 5,

        /// <summary>
        /// Assertion that the associated menu item is used to refresh the data.
        /// </summary>
        Refresh = 6,

        /// <summary>
        /// Assertion that the associated menu item is used to export the data.
        /// </summary>
        Export = 7,

        /// <summary>
        /// Assertion that the associated menu item is used to invoke the help.
        /// </summary>
        Help = 8,

        /// <summary>
        /// Assertion that the associated menu item is used to highlight.
        /// </summary>
        Highlight = 9,

        /// <summary>
        /// Assertion that the associated menu item is used to copy.
        /// </summary>
        Copy = 10,

        /// <summary>
        /// Assertion that the associated menu item is used to save.
        /// </summary>
        Save = 11,

        /// <summary>
        /// Assertion that the associated menu item is used to navigate to a Thing
        /// </summary>
        Navigate = 12,

        /// <summary>
        /// Assertion that the associated menu item is used to save a Thing to favorites
        /// </summary>
        Favorite = 13,

        /// <summary>
        /// Assertion that the associated menu item is used to open a Thing to a Editor
        /// </summary>
        Open = 14,

        /// <summary>
        /// Assertion that the associated menu item is used to hide a Thing to a Editor
        /// </summary>
        Hide = 15,

        /// <summary>
        /// Assertion that the associated menu item is used to publish a Thing to a Editor
        /// </summary>
        Publish = 16,

        /// <summary>
        /// Assertion that the associated menu item is used to ready a Thing for review to a Editor
        /// </summary>
        Review = 17
    }
}
