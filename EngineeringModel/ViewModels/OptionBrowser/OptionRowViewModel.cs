// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OptionRowViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.ViewModels
{
    using System.Threading.Tasks;
    using System.Windows;
    using CDP4Common.CommonData;    
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.DragDrop;
    using CDP4Composition.Mvvm;
    using CDP4Composition.Services;
    using CDP4Dal;
    using ReactiveUI;

    /// <summary>
    /// The extended row class representing an <see cref="Option"/>
    /// </summary>
    public class OptionRowViewModel : CDP4CommonView.OptionRowViewModel, IDropTarget
    {
        /// <summary>
        /// Backing field for <see cref="IsDefaultOption"/>
        /// </summary>
        private bool isDefaultOption;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionRowViewModel"/> class
        /// </summary>
        /// <param name="option">The <see cref="Option"/> associated with this row</param>
        /// <param name="session">The session</param>
        /// <param name="containerViewModel">The container <see cref="IViewModelBase{T}"/></param>
        public OptionRowViewModel(Option option, ISession session, IViewModelBase<Thing> containerViewModel)
            : base(option, session, containerViewModel)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is the default <see cref="Option"/>
        /// </summary>
        public bool IsDefaultOption
        {
            get { return this.isDefaultOption; }
            set { this.RaiseAndSetIfChanged(ref this.isDefaultOption, value); }
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">
        ///  Information about the drag operation.
        /// </param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see cref="DropInfo.Effects"/> property on 
        /// <paramref name="dropInfo"/> should be set to a value other than <see cref="DragDropEffects.None"/>
        /// and <see cref="DropInfo.Payload"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Payload is Category category)
            {
                dropInfo.Effects = CategoryApplicationValidationService.ValidateDragDrop(this.Session.PermissionService, this.Thing, category, logger);
                return;
            }

            dropInfo.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Performs the drop operation
        /// </summary>
        /// <param name="dropInfo">
        /// Information about the drop operation.
        /// </param>
        public async Task Drop(IDropInfo dropInfo)
        {
            var category = dropInfo.Payload as Category;
            if (category != null)
            {
                await this.Drop(category);
                return;
            }
        }

        // <summary>
        /// Handles the drop action of a <see cref="Category"/>
        /// </summary>
        /// <param name="category">The dropped <see cref="Category"/></param>
        private async Task Drop(Category category)
        {
            var clone = this.Thing.Clone(false);
            clone.Category.Add(category);
            await this.DalWrite(clone);
        }
    }
}