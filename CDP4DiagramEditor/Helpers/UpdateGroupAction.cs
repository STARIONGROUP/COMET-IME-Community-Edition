


namespace CDP4DiagramEditor.Helpers
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    using DevExpress.Xpf.Bars.Native;
    using DevExpress.Xpf.Core.Native;
    using DevExpress.Xpf.Utils;
    using DevExpress.Xpf.Bars;

    [ContentProperty("Value")]
    public class UpdateGroupAction : BarManagerControllerActionBase
    {
        public static readonly DevExpress.Xpf.Core.Internal.ReflectionHelper rHelper = new DevExpress.Xpf.Core.Internal.ReflectionHelper();
        /// <summary>
        ///   <para>Identifies the <see cref="P:DevExpress.Xpf.Bars.UpdateAction.Value" /> dependency property.</para>
        /// </summary>
        /// <value>A dependency property identifier.</value>
        public static readonly DependencyProperty ValueProperty = DependencyPropertyManager.Register(nameof(Value), typeof(object), typeof(UpdateAction), (PropertyMetadata)new FrameworkPropertyMetadata((PropertyChangedCallback)null));
        /// <summary>
        ///   <para>Identifies the <see cref="P:DevExpress.Xpf.Bars.UpdateAction.Property" /> dependency property.</para>
        /// </summary>
        /// <value>A dependency property identifier.</value>
        public static readonly DependencyProperty PropertyProperty = DependencyPropertyManager.Register(nameof(Property), typeof(DependencyProperty), typeof(UpdateAction), (PropertyMetadata)new FrameworkPropertyMetadata((PropertyChangedCallback)null));
        /// <summary>
        ///   <para>Identifies the <see cref="P:DevExpress.Xpf.Bars.UpdateAction.PropertyName" /> dependency property.</para>
        /// </summary>
        /// <value>A dependency property identifier.</value>
        public static readonly DependencyProperty PropertyNameProperty = DependencyPropertyManager.Register(nameof(PropertyName), typeof(string), typeof(UpdateAction), (PropertyMetadata)new FrameworkPropertyMetadata((PropertyChangedCallback)null));
        /// <summary>
        ///   <para>Identifies the <see cref="P:DevExpress.Xpf.Bars.UpdateAction.ElementName" /> dependency property.</para>
        /// </summary>
        /// <value>A dependency property identifier.</value>
        public static readonly DependencyProperty ElementNameProperty = CollectionAction.ElementNameProperty.AddOwner(typeof(UpdateAction));
        /// <summary>
        ///   <para>Identifies the <see cref="P:DevExpress.Xpf.Bars.UpdateAction.Element" /> dependency property.</para>
        /// </summary>
        /// <value>A dependency property identifier.</value>
        public static readonly DependencyProperty ElementProperty = CollectionAction.ElementProperty.AddOwner(typeof(UpdateAction));

        /// <summary>
        ///   <para>Get or sets a dependency property whose value should be updated. This is a dependency property.</para>
        /// </summary>
        /// <value>A <see cref="T:System.Windows.DependencyProperty" /> whose value should be updated.</value>
        public DependencyProperty Property
        {
            get
            {
                return (DependencyProperty)this.GetValue(UpdateGroupAction.PropertyProperty);
            }
            set
            {
                this.SetValue(UpdateGroupAction.PropertyProperty, (object)value);
            }
        }

        /// <summary>
        ///   <para>Gets or sets a name of the property whose value should be updated. This is a dependency property.</para>
        /// </summary>
        /// <value>A name of the property whose value should be updated.</value>
        public string PropertyName
        {
            get
            {
                return (string)this.GetValue(UpdateGroupAction.PropertyNameProperty);
            }
            set
            {
                this.SetValue(UpdateGroupAction.PropertyNameProperty, (object)value);
            }
        }

        /// <summary>
        ///   <para>Gets or sets a new value of an updated property. This is a dependency property.</para>
        /// </summary>
        /// <value>A new value of an updated property.</value>
        public object Value
        {
            get
            {
                return this.GetValue(UpdateGroupAction.ValueProperty);
            }
            set
            {
                this.SetValue(UpdateGroupAction.ValueProperty, value);
            }
        }

        /// <summary>
        ///   <para>Gets or sets the data binding for the updated property.</para>
        /// </summary>
        /// <value>A <see cref="T:System.Windows.Data.BindingBase" /> object instance that specifies the data binding for the updated property.</value>
        public BindingBase ValueBinding { get; set; }

        /// <summary>
        ///   <para>Gets or sets an element whose properties should be updated. This is a dependency property.</para>
        /// </summary>
        /// <value>An element whose property should be updated.</value>
        public object Element
        {
            get
            {
                return this.GetValue(UpdateGroupAction.ElementProperty);
            }
            set
            {
                this.SetValue(UpdateGroupAction.ElementProperty, value);
            }
        }

        /// <summary>
        ///   <para>Gets or sets a name of an element whose property value should be updated. This is a dependency property.</para>
        /// </summary>
        /// <value>A name of an element whose property value should be updated</value>
        public string ElementName
        {
            get
            {
                return (string)this.GetValue(UpdateGroupAction.ElementNameProperty);
            }
            set
            {
                this.SetValue(UpdateGroupAction.ElementNameProperty, (object)value);
            }
        }

        /// <summary>
        ///   <para>Returns the object being manipulated by the current action.</para>
        /// </summary>
        /// <returns>An object that is manipulated by the current action.</returns>
        public override object GetObjectCore()
        {
            if (this.Element != null)
                return this.Element;
            DependencyObject context = CollectionAction.GetContext((DependencyObject)this);
            if (this.ElementName == null && context != null)
                return (object)context;
            return CollectionActionHelper.Instance.FindElementByName(CollectionAction.GetContext((DependencyObject)this), this.ElementName, this.Container, ScopeSearchSettings.Local | ScopeSearchSettings.Descendants, (Func<IFrameworkInputElement, bool>)null);
        }

        protected override void ExecuteCore(DependencyObject associatedObject)
        {
            object objectCore = this.GetObjectCore();
            if (objectCore == null)
                return;
            DependencyObject target = objectCore as DependencyObject;
            if (this.ValueBinding != null && this.Property != null && target != null)
            {
                BindingOperations.SetBinding(target, this.Property, this.ValueBinding);
            }
            else
            {
                Action<object, object> setter = this.GetSetter(objectCore);
                if (setter == null)
                    return;
                setter(objectCore, this.GetConvertedValue(objectCore));
            }
        }

        protected virtual object GetConvertedValue(object obj)
        {
            Type propertyType = this.GetPropertyType(obj);
            if (this.Value == null || propertyType.IsAssignableFrom(this.Value.GetType()))
                return this.Value;
            return RenderTriggerHelper.GetConvertedValue(propertyType, this.Value);
        }

        private Type GetPropertyType(object obj)
        {
            if (this.Property != null)
                return this.Property.PropertyType;
            if (this.PropertyName != null)
                return UpdateGroupAction.rHelper.GetPropertyType(obj, this.PropertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return typeof(object);
        }

        protected virtual Action<object, object> GetSetter(object element)
        {
            if (this.Property != null)
                return (Action<object, object>)((obj, val) => ((DependencyObject)obj).SetValue(this.Property, val));
            if (this.PropertyName != null)
                return UpdateGroupAction.rHelper.GetInstanceMethodHandler<Action<object, object>>(element, "set_" + this.PropertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public, element.GetType(), new int?(), (Type[])null, true);
            return (Action<object, object>)null;
        }
    }
}
