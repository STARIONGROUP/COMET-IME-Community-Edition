// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CategoryApplicationValidationService.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Services
{
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal.Permission;
    using NLog;

    /// <summary>
    /// The purpose of the <see cref="CategoryApplicationValidationService"/> is to provide helper methods to
    /// determine whether a <see cref="Category"/> can be applied to a <see cref="ICategorizableThing"/>
    /// </summary>
    public static class CategoryApplicationValidationService
    {
        /// <summary>
        /// Validates whether the <see cref="Category"/> can be applied to the target <see cref="ICategorizableThing"/>
        /// as part of a frag-n-drop operation
        /// </summary>
        /// <param name="permissionService">
        /// The <see cref="IPermissionService"/> used to determine whether the active <see cref="Participant"/> has the appropriate
        /// permissions to update the target <see cref="ICategorizableThing"/>
        /// </param>
        /// <param name="target">
        /// The target <see cref="ICategorizableThing"/> to which the <see cref="Category"/> may or may not be applied.
        /// </param>
        /// <param name="category">
        /// The <see cref="Category"/> for which the <see cref="ICategorizableThing"/> is being validated
        /// </param>
        /// <param name="logger">
        /// The <see cref="ILogger"/> used for logging
        /// </param>
        /// <returns>
        /// <see cref="DragDropEffects.Copy"/> when the <see cref="Category"/> can applied, <see cref="DragDropEffects.None"/> otherwise
        /// </returns>
        public static DragDropEffects ValidateDragDrop(IPermissionService permissionService, ICategorizableThing target, Category category, ILogger logger)
        {
            if (!(target is Thing thing))
            {
                logger.Warn("The target {0} is not a Thing", target);
                return DragDropEffects.None;
            }

            if (!permissionService.CanWrite(thing))
            {
                logger.Info("Permission denied to apply the Category.");
                return DragDropEffects.None;
            }

            if (!category.PermissibleClass.Contains(thing.ClassKind))
            {
                logger.Info("The Category {0} can not be applied to this kind of Thing: {1}", category.ShortName, thing.ClassKind);
                return DragDropEffects.None;
            }

            if (!target.IsCategoryInChainOfRdls(category))
            {
                logger.Info("The Category {0} is not in the same chain of rdls as the target {1}", category.ShortName, thing.ClassKind);
                return DragDropEffects.None;
            }

            if (target.Category.Contains(category))
            {
                logger.Info("The Category {0} has already been applied to this {1}", category.ShortName, thing.ClassKind);
                return DragDropEffects.None;
            }

            return DragDropEffects.Copy;
        }
    }
}