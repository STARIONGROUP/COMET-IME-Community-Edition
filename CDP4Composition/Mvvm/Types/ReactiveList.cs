// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReactiveList.cs" company="Starion Group S.A.">
//    Copyright (c) 2015-2022 Starion Group S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Alexander van Delft, Nathanael Smiechowski, Antoine Théate, Omar Elebiary
//
//    This file is part of COMET-IME Community Edition.
//    The COMET-IME Community Edition is the Starion Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The COMET-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The COMET-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program. If not, see http://www.gnu.org/licenses/.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    using DynamicData;
    using DynamicData.Binding;

    /// <summary>
    /// Wrapper class for the legacy <see cref="ReactiveList{T}"/> class
    /// </summary>
    /// <typeparam name="T">The <see cref="System.Type"/></typeparam>
    public class ReactiveList<T> : IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        /// The <see cref="SourceList{T}"/> that provides an observable list
        /// </summary>
        protected readonly SourceList<T> SourceList = new();

        /// <summary>
        /// An observable collection that is connected to the <see cref="SourceList"/>
        /// </summary>
        private ObservableCollectionExtended<T> observableCollection = new();

        /// <summary>
        /// Backing field for the <see cref="Changed"/> property
        /// </summary>
        private IObservable<NotifyCollectionChangedEventArgs> changed;

        /// <summary>
        /// Backing field for the <see cref="CountChanged"/> property
        /// </summary>
        private IObservable<int> countChanged;

        /// <summary>
        /// Backing field for the <see cref="ItemsRemoved"/> property
        /// </summary>
        private Subject<T> itemsRemoved;

        /// <summary>
        /// Backing field for the <see cref="ItemsAdded"/> property
        /// </summary>
        private Subject<T> itemsAdded;

        /// <summary>
        /// Backing field for the <see cref="IsEmptyChanged"/> property
        /// </summary>
        private IObservable<bool> isEmptyChanged;

        /// <summary>
        /// Gets a value indicating that this <see cref="IList"/> is of a fixed size.
        /// </summary>
        public bool IsFixedSize => false;

        /// <summary>
        /// Gets a value indicating that this <see cref="IList"/> is readonly.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Is this collection synchronized (i.e., thread-safe)?  If you want a
        /// thread-safe collection, you can use SyncRoot as an object to
        /// synchronize your collection with.  If you're using one of the
        /// collections in System.Collections, you could call the static
        /// Synchronized method to get a thread-safe wrapper around the
        /// underlying collection.
        /// </summary>
        public bool IsSynchronized => false;

        /// <summary>
        /// SyncRoot will return an Object to use for synchronization
        /// (thread safety).  You can use this object in your code to take a
        /// lock on the collection, even if this collection is a wrapper around
        /// another collection.  The intent is to tunnel through to a real
        /// implementation of a collection, and use one of the internal objects
        /// found in that code.
        ///
        /// In the absence of a static Synchronized method on a collection,
        /// the expected usage for SyncRoot would look like this:
        ///
        /// ICollection col = ...
        /// lock (col.SyncRoot) {
        ///     // Some operation on the collection, which is now thread safe.
        ///     // This may include multiple operations.
        /// }
        ///
        ///
        /// The system-provided collections have a static method called
        /// Synchronized which will create a thread-safe wrapper around the
        /// collection.  All access to the collection that you want to be
        /// thread-safe should go through that wrapper collection.  However, if
        /// you need to do multiple calls on that collection (such as retrieving
        /// two items, or checking the count then doing something), you should
        /// NOT use our thread-safe wrapper since it only takes a lock for the
        /// duration of a single method call.  Instead, use Monitor.Enter/Exit
        /// or your language's equivalent to the C# lock keyword as mentioned
        /// above.
        ///
        /// For collections with no publicly available underlying store, the
        /// expected implementation is to simply return the this pointer.  Note
        /// that the this pointer may not be sufficient for collections that
        /// wrap other collections;  those should return the underlying
        /// collection's SyncRoot property.
        /// </summary>
        public object SyncRoot => this;

        /// <summary>
        /// Gets the number of items in the <see cref="SourceList"/>
        /// </summary>
        public int Count => this.SourceList.Count;

        /// <summary>
        /// Returns true if the <see cref="ReactiveList{T}"/> is empty.
        /// </summary>
        public bool IsEmpty => this.SourceList.Count == 0;

        /// <summary>
        /// Observable.FromEvent implementation that wraps the <see cref="observableCollection"/>'s NotifyCollectionChanged event
        /// </summary>
        public IObservable<NotifyCollectionChangedEventArgs> Changed
        {
            get
            {
                return this.changed ??= Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    handler =>
                    {
                        void FsHandler(object sender, NotifyCollectionChangedEventArgs e)
                        {
                            handler(e);
                        }

                        return FsHandler;
                    },
                    fsHandler => this.CollectionChanged += fsHandler,
                    fsHandler => this.CollectionChanged -= fsHandler);
            }
        }

        /// <summary>
        /// Gets an <see cref="IObservable{T}"/> of type <see cref="int"/> that executes when the <see cref="SourceList"/>s item count changed. 
        /// </summary>
        public IObservable<int> CountChanged
        {
            get { return this.countChanged ??= this.SourceList.CountChanged; }
        }

        /// <summary>
        /// Observable.FromEvent implementation that wraps the <see cref="observableCollection"/>'s NotifyCollectionChanged event
        /// and executes the handler for every removed item
        /// </summary>
        public IObservable<T> ItemsRemoved
        {
            get
            {
                return this.itemsRemoved ??= new Subject<T>();
            }
        }

        /// <summary>
        /// Observable.FromEvent implementation that wraps the <see cref="observableCollection"/>'s NotifyCollectionChanged event
        /// and executes the handler for every added item
        /// </summary>
        public IObservable<T> ItemsAdded
        {
            get
            {
                return this.itemsAdded ??= new Subject<T>();
            }
        }

        /// <summary>
        /// Gets an <see cref="IObservable{T}"/> of type <see cref="bool"/> that executes when the first items gets added to an empty list,
        /// or when the last item is removed from the list.
        /// </summary>
        public IObservable<bool> IsEmptyChanged
        {
            get { return this.isEmptyChanged ??= this.Changed.Select(_ => this.SourceList.Count == 0).DistinctUntilChanged(); }
        }
        
        /// <summary>
        /// Gets the item at a specific position in the <see cref="observableCollection"/>
        /// </summary>
        /// <param name="index">The position in the <see cref="observableCollection"/></param>
        /// <returns>The item at the specific position</returns>
        /// <exception cref="NotImplementedException"></exception>
        public T this[int index]
        {
            get => this.observableCollection[index];
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the item at a specific position in the <see cref="observableCollection"/>
        /// </summary>
        /// <param name="index">The position in the <see cref="observableCollection"/></param>
        /// <returns>The item at the specific position</returns>
        /// <exception cref="NotImplementedException"></exception>
        object IList.this[int index]
        {
            get => this.observableCollection[index];
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Accessible constructor
        /// </summary>
        public ReactiveList()
        {
            this.Initialize();
        }

        /// <summary>
        /// Accessible constructor
        /// </summary>
        public ReactiveList(IEnumerable<T> def)
        {
            this.Initialize();
            this.SourceList.AddRange(def);
        }

        /// <summary>
        /// Initializes this instance of <see cref="ReactiveList{T}"/>
        /// </summary>
        private void Initialize()
        {
            this.SourceList.Connect().Bind(this.observableCollection).Subscribe();
            this.SourceList.CountChanged.Subscribe(x => this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.observableCollection.Count))));

            this.observableCollection.CollectionChanged += (sender, args) => this.CollectionChanged?.Invoke(this, args);

            this.observableCollection
                .WhenAnyPropertyChanged("Item[]")
                .Subscribe(x => this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]")));
        }

        /// <summary>
        /// Sort the <see cref="SourceList"/> using a <see cref="Comparison{T}"/>
        /// </summary>
        /// <param name="comparison">The <see cref="Comparison{T}"/></param>
        public void Sort(Comparison<T> comparison)
        {
            this.Sort(Comparer<T>.Create(comparison));
        }

        /// <summary>
        /// Sort the <see cref="SourceList"/> using a <see cref="IComparer{T}"/>
        /// </summary>
        /// <param name="comparer">The <see cref="IComparer{T}"/></param>
        public void Sort(IComparer<T> comparer)
        {
            using (this.observableCollection.SuspendNotifications())
            {
                var items = this.SourceList.Items.ToList();

                foreach (var item in items)
                {
                    this.SortedMove(item, comparer);
                }
            }
        }

        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        /// <param name="oldIndex">The index FROM which the item should be copied.</param>
        /// <param name="newIndex">The index TO which the item should be copied.</param>
        public void Move(int oldIndex, int newIndex)
        {
            this.SourceList.Move(oldIndex, newIndex);
        }

        /// <summary>
        /// Remove the item at index <see cref="index"/>
        /// </summary>
        /// <param name="index">The index of the item to be removed</param>
        public void RemoveAt(int index)
        {
            var obj = this.SourceList.Items.ToArray()[index];
            this.SourceList.RemoveAt(index);
            this.itemsRemoved?.OnNext(obj);
        }

        /// <summary>
        /// Removes a range of <see cref="count"/> items starting from index <see cref="index"/> 
        /// </summary>
        /// <param name="index">The starting index</param>
        /// <param name="count">the amount of items</param>
        public void RemoveRange(int index, int count)
        {
            var items = new List<T>();
            var sourceArray = this.SourceList.Items.ToArray();

            for (var i = index; i < count; i++)
            {
                items.Add(sourceArray[index]);
            }

            this.SourceList.RemoveRange(index, count);

            foreach (var item in items)
            {
                this.itemsRemoved?.OnNext(item);
            }
        }

        /// <summary>
        /// Removes all items in a given <see cref="IEnumerable{T}"/> from the <see cref="SourceList"/>
        /// </summary>
        /// <param name="remove">The <see cref="IEnumerable{T}"/></param>
        public void RemoveAll(IEnumerable<T> remove)
        {
            var removeList = remove.ToList();
            this.SourceList.RemoveMany(removeList);

            foreach (var removable in removeList)
            {
                this.itemsRemoved?.OnNext(removable);
            }
        }

        /// <summary>
        /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            this.PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        [field: NonSerialized]
        protected virtual event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => this.PropertyChanged += value;
            remove => this.PropertyChanged -= value;
        }

        /// <summary>
        /// Occurs when the collection changes, either by adding or removing an item.
        /// </summary>
        /// <remarks>
        /// see <seealso cref="INotifyCollectionChanged"/>
        /// </remarks>
        [field: NonSerialized]
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
        /// </summary>
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add => this.CollectionChanged += value;
            remove => this.CollectionChanged -= value;
        }

        /// <summary>
        /// Returns the <see cref="observableCollection"/>'s GetEnumerator.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{T}"/></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return this.observableCollection.GetEnumerator();
        }

        /// <summary>
        /// Returns the <see cref="observableCollection"/>'s GetEnumerator.
        /// </summary>
        /// <returns>The <see cref="IEnumerator"/></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the <see cref="SourceList"/>
        /// </summary>
        /// <param name="item">The item</param>
        public void Add(T item)
        {
            this.SourceList.Add(item);
            this.itemsAdded?.OnNext(item);
        }

        /// <summary>
        /// Adds an item to the <see cref="SourceList"/>
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>An <see cref="int"/> value that represents the index of the newly added item</returns>
        /// <exception cref="NotImplementedException">Throws if item is not of type <see cref="T"/></exception>
        public int Add(object item)
        {
            if (item is T obj)
            {
                this.Add(obj);
                return this.observableCollection.IndexOf(obj);
            }

            return -1;
        }

        /// <summary>
        /// Clears the <see cref="SourceList"/>
        /// </summary>
        public void Clear()
        {
            var items = this.SourceList.Items.ToArray();
            this.RemoveAll(items);
        }

        /// <summary>
        /// Checks if a specific item is present in the <see cref="observableCollection"/>
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>true if the item is present in the <see cref="observableCollection"/>, otherwise false</returns>
        /// <exception cref="NotImplementedException">Throws if item is not of type <see cref="T"/></exception>
        public bool Contains(object item)
        {
            if (item is T obj)
            {
                return this.observableCollection.Contains(obj);
            }

            return false;
        }

        /// <summary>
        /// Returns the index of a specific item in the <see cref="observableCollection"/>
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The index of the item if found, otherwise -1</returns>
        /// <exception cref="NotImplementedException">Throws if item is not of type <see cref="T"/></exception>
        public int IndexOf(object item)
        {
            if (item is T obj)
            {
                return this.observableCollection.IndexOf(obj);
            }

            return -1;
        }

        /// <summary>
        /// Inserts an item to <see cref="SourceList"/> at a specific index
        /// </summary>
        /// <param name="index">The index where to insert the item</param>
        /// <param name="item">The item</param>
        /// <exception cref="NotImplementedException">Throws if item is not of type <see cref="T"/></exception>
        public void Insert(int index, object item)
        {
            if (item is T obj)
            {
                this.Insert(index, obj);
                return;
            }
        }

        /// <summary>
        /// Removes an item from the <see cref="SourceList"/>
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="NotImplementedException">Throws if item is not of type <see cref="T"/></exception>
        void IList.Remove(object item)
        {
            if (item is T obj)
            {
                this.Remove(obj);
                return;
            }
        }

        /// <summary>
        /// Removes an item at a specific index from the <see cref="SourceList"/> 
        /// </summary>
        /// <param name="index">The index</param>
        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }
        
        /// <summary>
        /// Checks if a specific item is present in the <see cref="observableCollection"/>
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>true if the item is present in the <see cref="observableCollection"/>, otherwise false</returns>
        public bool Contains(T item)
        {
            return this.observableCollection.Contains(item);
        }

        /// <summary>
        /// Copies the content of <see cref="observableCollection"/> to an array, starting from a specific position into the array
        /// </summary>
        /// <param name="array">The array</param>
        /// <param name="arrayIndex">The starting position into the array </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.observableCollection.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a specific item from the <see cref="SourceList"/>
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>true if the itemn was removed, otherwise false</returns>
        public bool Remove(T item)
        {
            if (this.SourceList.Remove(item))
            {
                this.itemsRemoved?.OnNext(item);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the content of <see cref="observableCollection"/> to an array, starting from a specific position into the array
        /// </summary>
        /// <param name="array">The array</param>
        /// <param name="index">The starting position into the array </param>
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)this.observableCollection).CopyTo(array, index);
        }

        /// <summary>
        /// Adds a range of items to the <see cref="SourceList"/>
        /// </summary>
        /// <param name="items">The items to add</param>
        public void AddRange(IEnumerable<T> items)
        {
            var itemList = items.ToList();

            this.SourceList.AddRange(itemList);

            foreach (var item in itemList)
            {
                this.itemsAdded?.OnNext(item);
            }
        }

        /// <summary>
        /// Returns the index of a specific item in the <see cref="observableCollection"/>
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The index of the item if found, otherwise -1</returns>
        public int IndexOf(T item)
        {
            return this.observableCollection.IndexOf(item);
        }

        /// <summary>
        /// Insert an item into the <see cref="SourceList"/> at a specific position
        /// </summary>
        /// <param name="index">The position within the <see cref="SourceList"/> where to add the item</param>
        /// <param name="item">The item</param>
        public void Insert(int index, T item)
        {
            this.SourceList.Insert(index, item);
            this.itemsAdded?.OnNext(item);
        }

        /// <summary>
        /// Removes an item at a specific index from the <see cref="SourceList"/> 
        /// </summary>
        /// <param name="index">The index</param>
        void IList<T>.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        /// <summary>
        /// Insert a <see cref="T"/> into the list given a <see cref="IComparer{T}"/>
        /// </summary>
        /// <param name="obj">The <see cref="T"/> to add</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> used to perform the sorting</param>
        public void SortedInsert(T obj, IComparer<T> comparer)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), $"The {nameof(obj)} may not be null");
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer), $"The {nameof(comparer)} may not be null");
            }

            // item is found using the comparer : returns the index of the item found
            // item not found : returns a negative number that is the bitwise complement 
            // of the index of the next element that is larger or count if none
            var index = this.BinarySearch(obj, comparer);

            if (index < 0)
            {
                this.Insert(~index, obj);
            }
            else
            {
                this.Insert(index, obj);
            }
        }

        /// <summary>
        /// Sort a <see cref="T"/> in the list given a <see cref="IComparer{T}"/>
        /// </summary>
        /// <param name="obj">The <see cref="T"/> to Move</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> used to perform the sorting</param>
        private void SortedMove(T obj, IComparer<T> comparer)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), $"The {nameof(obj)} may not be null");
            }

            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer), $"The {nameof(comparer)} may not be null");
            }

            // item is found using the comparer : returns the index of the item found
            // item not found : returns a negative number that is the bitwise complement 
            // of the index of the next element that is larger or count if none
            var index = this.BinarySearch(obj, comparer);

            if (index < 0)
            {
                this.Move(this.IndexOf(obj), Math.Min(~index, this.Count - 1));
            }
            else
            {
                this.Move(this.IndexOf(obj), index);
            }
        }
    }
}
