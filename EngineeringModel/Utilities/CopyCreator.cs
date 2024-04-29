// -------------------------------------------------------------------------------------------------
// <copyright file="CopyCreator.cs" company="Starion Group S.A.">
//   Copyright (c) 2017 Starion Group S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Utilities
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;    
    using CDP4Dal.Operations;
    using CDP4CommonView.ViewModels;
    using CDP4Composition.Navigation;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using NLog;
    using CDP4Composition.DragDrop;

    /// <summary>
    /// The class responsible for copy operations
    /// </summary>
    internal class CopyCreator
    {
        /// <summary>
        /// The NLog logger
        /// </summary>
        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="ISession"/> in which the copy is performed
        /// </summary>
        private readonly ISession session;

        /// <summary>
        /// The <see cref="IDialogNavigationService"/> used to navigate to <see cref="IDialogViewModel"/>
        /// </summary>
        private readonly IDialogNavigationService dialogNavigationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CopyCreator"/> class
        /// </summary>
        /// <param name="session">The associated <see cref="ISession"/></param>
        /// <param name="dialogNavigationService">The <see cref="IDialogNavigationService"/></param>
        public CopyCreator(ISession session, IDialogNavigationService dialogNavigationService)
        {
            this.session = session;
            this.dialogNavigationService = dialogNavigationService;
        }

        /// <summary>
        /// Perform the copy operation of an <see cref="ElementDefinition"/>
        /// </summary>
        /// <param name="elementDefinition">The <see cref="ElementDefinition"/> to copy</param>
        /// <param name="targetIteration">The target container</param>
        /// <param name="keyStates">The <see cref="DragDropKeyStates"/> used in the drag-and-drop operation</param>
        public async Task Copy(ElementDefinition elementDefinition, Iteration targetIteration, DragDropKeyStates keyStates)
        {
            // copy the payload to this iteration
            var ownedIsChanged = keyStates ==  Constants.DryCopy || keyStates == Constants.CtrlCopy;
            var copyOperationHelper = new CopyPermissionHelper(this.session, ownedIsChanged);
            var copyPermissionResult = copyOperationHelper.ComputeCopyPermission(elementDefinition, targetIteration);

            var operationKind = keyStates.GetCopyOperationKind();
            
            if (copyPermissionResult.ErrorList.Any())
            {
                // show permission information
                var copyConfirmationDialog = new CopyConfirmationDialogViewModel(copyPermissionResult.CopyableThings, copyPermissionResult.ErrorList);

                var dialogResult = this.dialogNavigationService.NavigateModal(copyConfirmationDialog);
                if (dialogResult != null && dialogResult.Result == true)
                {
                    await this.WriteCopyOperation(elementDefinition, targetIteration, operationKind.Value);
                }
            }
            else if(copyPermissionResult.CopyableThings.Any())
            {
                await this.WriteCopyOperation(elementDefinition, targetIteration, operationKind.Value);
            }
        }

        /// <summary>
        /// Create and write the copy operation
        /// </summary>
        /// <param name="thingToCopy">The <see cref="Thing"/> to copy</param>
        /// <param name="targetContainer">The target container</param>
        /// <param name="keyStates">The <see cref="DragDropKeyStates"/> used in the drag-and-drop operation</param>
        private async Task WriteCopyOperation(Thing thingToCopy, Thing targetContainer, OperationKind operationKind)
        {
            var clone = thingToCopy.Clone(false);
            var containerClone = targetContainer.Clone(false);
            
            var transactionContext = TransactionContextResolver.ResolveContext(targetContainer);
            var transaction = new ThingTransaction(transactionContext, containerClone);
            transaction.Copy(clone, containerClone, operationKind);

            await this.session.Write(transaction.FinalizeTransaction());
        }
    }
}