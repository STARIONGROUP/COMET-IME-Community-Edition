// -------------------------------------------------------------------------------------------------
// <copyright file="ItemsSourceHelper.cs" company="Starion Group S.A.">
//   Copyright (c) 2015-2018 Starion Group S.A.
//   Source @ https://github.com/DevExpress-Examples/how-to-implement-the-itemssource-property-for-the-layoutgroup-layoutcontrol-control-e5081/tree/13.2.7%2B/CS/LayoutControlItemsSource
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm.Behaviours
{
    using System.Collections;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using DevExpress.Mvvm.UI.Interactivity;
    using DevExpress.Xpf.LayoutControl;

    /// <summary>
    /// A behaviour to add the "ItemsSource" functionality to a <see cref="LayoutGroup"/>. That allows dynamic declaration of <see cref="LayoutItem"/> for a <see cref="LayoutGroup"/>
    /// </summary>
    public class ItemsSourceHelper : Behavior<LayoutGroup>
    {
        /// <summary>
        /// The "ItemTemplateSelector" <see cref="DependencyProperty"/> for the dynamic <see cref="LayoutItem"/>
        /// </summary>
        public static readonly DependencyProperty ItemTemplateSelectorProperty =
           DependencyProperty.Register("ItemTemplateSelector", typeof(DataTemplateSelector), typeof(ItemsSourceHelper), new PropertyMetadata());

        /// <summary>
        /// The "ItemTemplate" <see cref="DependencyProperty"/> to use for the dynamic <see cref="LayoutItem"/>
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty =
          DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(ItemsSourceHelper), new PropertyMetadata());

        /// <summary>
        /// The <see cref="DependencyProperty"/> to use to bind a collection of view-model to the current <see cref="LayoutGroup"/>
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
           DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ItemsSourceHelper), new PropertyMetadata((d, e) => ((ItemsSourceHelper)d).OnItemsSourceChanged(e)));

        /// <summary>
        /// Gets or sets the <see cref="DataTemplateSelector"/>
        /// </summary>
        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)this.GetValue(ItemTemplateSelectorProperty); }
            set { this.SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/>
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
            set { this.SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// Gets the collection of view-models
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)this.GetValue(ItemsSourceProperty); }
            set { this.SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Gets the current <see cref="LayoutGroup"/>
        /// </summary>
        protected LayoutGroup Group => this.AssociatedObject;

        /// <summary>
        /// Gets the children of this <see cref="LayoutGroup"/>
        /// </summary>
        protected UIElementCollection Children => this.Group.Children;

        /// <summary>
        /// Initializes the behaviour
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.RearrangeChildren();
        }

        /// <summary>
        /// Handles the <see cref="DependencyPropertyChangedEventArgs"/> evebt
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/></param>
        protected virtual void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)e.OldValue).CollectionChanged -= this.OnItemsSourceCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)e.NewValue).CollectionChanged += this.OnItemsSourceCollectionChanged;
            }

            if (this.Group != null)
            {
                this.RearrangeChildren();
            }
        }

        /// <summary>
        /// Handles the <see cref="NotifyCollectionChangedEventArgs"/> event
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The event</param>
        protected virtual void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.Group == null)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.RearrangeChildren();
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    this.AddItem(item);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    this.RemoveItem(item);
                }
            }
        }

        /// <summary>
        /// Rearrange the children of this <see cref="LayoutGroup"/>
        /// </summary>
        protected virtual void RearrangeChildren()
        {
            this.Children.Clear();
            if (this.ItemsSource != null)
            {
                foreach (var item in this.ItemsSource)
                {
                    this.AddItem(item);
                }
            }
        }

        /// <summary>
        /// Removes a <see cref="LayoutItem"/> from this <see cref="LayoutGroup"/>
        /// </summary>
        /// <param name="item">the associated data-context</param>
        protected virtual void RemoveItem(object item)
        {
            var layoutItem = this.Children.OfType<LayoutItem>().FirstOrDefault(x => x.DataContext.Equals(item));
            if (layoutItem != null)
            {
                this.Children.Remove(layoutItem);
            }
        }

        /// <summary>
        /// Add a new <see cref="LayoutItem"/> to this <see cref="LayoutGroup"/>
        /// </summary>
        /// <param name="item">The associated data-context</param>
        protected virtual void AddItem(object item)
        {
            var layoutItem = new LayoutItem { DataContext = item };
            var content = new ContentControl { Content = item };
            content.SetBinding(ContentControl.ContentTemplateProperty, new Binding("ItemTemplate") { Mode = BindingMode.TwoWay, Source = this });
            content.SetBinding(ContentControl.ContentTemplateSelectorProperty, new Binding("ItemTemplateSelector") { Mode = BindingMode.TwoWay, Source = this });
            layoutItem.Content = content;
            this.Children.Add(layoutItem);
        }
    }
}
