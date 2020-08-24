// -------------------------------------------------------------------------------------------------
// <copyright file="ViewUtils.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4CommonView
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Utility class for views
    /// </summary>
    public static class ViewUtils
    {
        /// <summary>
        /// Create a <see cref="Binding"/> with the specified input
        /// </summary>
        /// <param name="dataContext">The data-context</param>
        /// <param name="path">The path of the property in the data-context</param>
        /// <param name="mode">The <see cref="BindingMode"/>. Default is <see cref="BindingMode.TwoWay"/></param>
        /// <param name="trigger">The <see cref="UpdateSourceTrigger"/>. Default is <see cref="UpdateSourceTrigger.PropertyChanged"/></param>
        /// <returns>The <see cref="Binding"/></returns>
        /// <exception cref="ArgumentNullException">The <paramref name="dataContext"/> cannot be null</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="path"/> cannot be null</exception>
        public static Binding CreateBinding(object dataContext, string path, BindingMode mode = BindingMode.TwoWay, UpdateSourceTrigger trigger = UpdateSourceTrigger.PropertyChanged)
        {
            if (dataContext == null)
            {
                throw new ArgumentNullException(nameof(dataContext), "The data-context cannot be null");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path), "The path cannot be null or empty");
            }

            var myBinding = new Binding
            {
                Source = dataContext, 
                Path = new PropertyPath(path, new object[0]), 
                Mode = mode, 
                UpdateSourceTrigger = trigger
            };

            return myBinding;
        }
    }
}