// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfImportResult.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using CDP4Common.EngineeringModelData;
    using CDP4Composition.Navigation;
    using ReqIFSharp;

    /// <summary>
    /// The result of the ReqIF import dialog
    /// </summary>
    public class ReqIfImportResult : BaseDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfImportResult"/> class
        /// </summary>
        /// <param name="reqIfObject">The <see cref="ReqIF"/> object</param>
        /// <param name="iteration">The selected <see cref="Iteration"/></param>
        /// <param name="res">A value indicating whether the task in the </param>
        public ReqIfImportResult(ReqIF reqIfObject, Iteration iteration, bool? res) : base(res)
        {
            this.ReqIfObject = reqIfObject;
            this.Iteration = iteration;
        }

        /// <summary>
        /// Gets the built <see cref="ReqIF"/> 
        /// </summary>
        public ReqIF ReqIfObject { get; private set; }

        /// <summary>
        /// Gets the <see cref="Iteration"/> in which the requirements shall be contained
        /// </summary>
        public Iteration Iteration { get; private set; }
    }
}