// ------------------------------------------------------------------------------------------------
// <copyright file="DiagramItemViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// ------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System.Windows;
    using CDP4Common.CommonData;
    using CDP4Dal;
    using DevExpress.Xpf.Diagram;

    /// <summary>
    /// The <see cref="DiagramItemViewModel{T}"/> handles the diagram items for <see cref="DiagramControl"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DiagramItemViewModel<T> : ViewModelBase<T> where T : Thing
    {
        public int ParentId { get; set; }
        public int Id { get; set; }

        public Point Position { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagramItemViewModel{T}"/> class. 
        /// </summary>
        /// <param name="thing">The <see cref="Thing"/> represented by the row</param>
        /// <param name="session">The session</param>
        public DiagramItemViewModel(T thing, ISession session): base(thing, session)
        {
        }
        
        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        public string Name
        {
            get { return ((INamedThing)this.Thing).Name; }
        }
    }
}
