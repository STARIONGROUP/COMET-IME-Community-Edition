// -------------------------------------------------------------------------------------------------
// <copyright file="ReqIfMappingDialogViewModelBase.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Requirements.ViewModels
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Dal.Operations;
    using CDP4Common.Types;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using CDP4Dal.Permission;
    using ReactiveUI;
    using ReqIFSharp;

    /// <summary>
    /// The base dialog view-model class for the ReqIF mapping dialogs
    /// </summary>
    public abstract class ReqIfMappingDialogViewModelBase : DialogViewModelBase
    {
        /// <summary>
        /// The Language code
        /// </summary>
        protected readonly string Lang; 

        /// <summary>
        /// The <see cref="ISession"/> where to write
        /// </summary>
        protected readonly ISession Session;

        /// <summary>
        /// The <see cref="IPermissionService"/>
        /// </summary>
        protected readonly IPermissionService PermissionService;

        /// <summary>
        /// The <see cref="IThingDialogNavigationService"/>
        /// </summary>
        protected readonly IThingDialogNavigationService ThingDialogNavigationService;
        
        /// <summary>
        /// The unique clone of the current <see cref="Iteration"/> to update
        /// </summary>
        protected readonly Iteration IterationClone;

        /// <summary>
        /// Backing field for <see cref="SelectedRow"/>
        /// </summary>
        private IMappingRowViewModelBase<Identifiable> selectedRow;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfMappingDialogViewModelBase"/> class
        /// Used by MEF
        /// </summary>
        protected ReqIfMappingDialogViewModelBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReqIfMappingDialogViewModelBase"/> class
        /// </summary>
        /// <param name="iteration">The <see cref="Iteration"/></param>
        /// <param name="session">The <see cref="ISession"/></param>
        /// <param name="thingDialogNavigationService">The <see cref="IThingDialogNavigationService"/></param>
        /// <param name="lang">The current language code</param>
        protected ReqIfMappingDialogViewModelBase(Iteration iteration, ISession session, IThingDialogNavigationService thingDialogNavigationService, string lang)
        {
            this.Lang = lang; 
            this.IterationClone = iteration;
            this.Session = session;
            this.PermissionService = this.Session.PermissionService;
            this.ThingDialogNavigationService = thingDialogNavigationService;

            this.CancelCommand = ReactiveCommand.Create();
            this.CancelCommand.Subscribe(_ => this.ExecuteCancelCommand());
        }

        /// <summary>
        /// Gets or sets the selected <see cref="MappingRowViewModelBase"/>
        /// </summary>
        public IMappingRowViewModelBase<Identifiable> SelectedRow
        {
            get { return this.selectedRow; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRow, value); }
        }

        public ReactiveCommand<object> CreateCommand { get; protected set; } 

        /// <summary>
        /// Gets the edit <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> EditCommand { get; protected set; }

        /// <summary>
        /// Gets the cancel <see cref="ICommand"/>
        /// </summary>
        public ReactiveCommand<object> CancelCommand { get; protected set; }

        /// <summary>
        /// Executes the cancel <see cref="ICommand"/>
        /// </summary>
        protected abstract void ExecuteCancelCommand();

        /// <summary>
        /// Add the contained <see cref="Thing"/> in a <see cref="IThingTransaction"/> object
        /// </summary>
        /// <param name="transaction">The <see cref="IThingTransaction"/></param>
        /// <param name="thing">The <see cref="Thing"/></param>
        protected void AddContainedThingToTransaction(IThingTransaction transaction, Thing thing)
        {
            var cloneType = thing.GetType();
            var containersInfo = cloneType.GetProperties().Where(x =>
                    x.PropertyType.IsGenericType &&
                    (x.PropertyType.GetGenericTypeDefinition() == typeof(ContainerList<>) ||
                    x.PropertyType.GetGenericTypeDefinition() == typeof(OrderedItemList<>)) &&
                     typeof(Thing).IsAssignableFrom(x.PropertyType.GetGenericArguments().Single())).ToList();

            foreach (var containerInfo in containersInfo)
            {
                var container = (IEnumerable)containerInfo.GetValue(thing);
                foreach (Thing containedThing in container)
                {
                    transaction.Create(containedThing);
                    this.AddContainedThingToTransaction(transaction, containedThing);
                }
            }
        }
    }
}