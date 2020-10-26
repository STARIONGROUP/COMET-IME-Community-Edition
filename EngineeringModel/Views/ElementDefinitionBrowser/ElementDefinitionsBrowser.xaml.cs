// -------------------------------------------------------------------------------------------------
// <copyright file="ElementDefinitionsBrowser.xaml.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2020 RHEA System S.A.
// </copyright>
// -------------------------------------------------------------------------------------------------

namespace CDP4EngineeringModel.Views
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Windows;

    using CDP4Common.SiteDirectoryData;

    using CDP4Composition;
    using CDP4Composition.Attributes;

    using DevExpress.Data.Filtering;
    using DevExpress.Xpf.Core.FilteringUI;
    using DevExpress.Xpf.Editors.Settings;

    /// <summary>
    /// Interaction logic for ElementDefinitions view
    /// </summary>
    [PanelViewExport(RegionNames.EditorPanel)]
    public partial class ElementDefinitionsBrowser : IPanelView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionsBrowser"/> class
        /// </summary>
        public ElementDefinitionsBrowser()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementDefinitionsBrowser"/> class.
        /// </summary>
        /// <param name="initializeComponent">
        /// a value indicating whether the contained Components shall be loaded
        /// </param>
        /// <remarks>
        /// This constructor is called by the navigation service
        /// </remarks>
        public ElementDefinitionsBrowser(bool initializeComponent)
        {
            if (initializeComponent)
            {
                this.InitializeComponent();
            }
        }

        private void OnQueryOperators(object sender, FilterEditorQueryOperatorsEventArgs e)
        {
            //if (e.FieldName == "Category")
            //{
            //    e.Operators.Clear();

            //    var customFunctionEditSettings = new BaseEditSettings[] 
            //    {
            //        new ListBoxEditSettings
            //        {
            //            ItemsSource = new List<Category>
            //            {
            //                new Category
            //                {
            //                    Iid = Guid.NewGuid(),
            //                    ShortName= "CAT1"
            //                }, 
            //                new Category
            //                {
            //                    Iid = Guid.NewGuid(),
            //                    ShortName = "CAT2"
            //                }
            //            }, 
            //            DisplayMember = nameof(Category.ShortName)
            //        }
            //    };

            //    e.Operators.Add(new FilterEditorOperatorItem(CustomFunctionName, customFunctionEditSettings) { Caption = "Member of Category" });

            //    //var template = (DataTemplate)this.FindResource("categorySelectionTemplate");
            //    //e.Operators[0].OperandTemplate = template;
            //}
        }
    }
}