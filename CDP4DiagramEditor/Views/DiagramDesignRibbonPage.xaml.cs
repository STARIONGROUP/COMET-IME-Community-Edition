using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CDP4DiagramEditor.Views
{
    using System.ComponentModel.Composition;

    using Microsoft.Practices.Prism.Mvvm;

    /// <summary>
    /// Interaction logic for DiagramToolsRibbonPage.xaml
    /// </summary>
    [Export(typeof(DiagramDesignRibbonPage))]
    public partial class DiagramDesignRibbonPage : IView
    {
        public DiagramDesignRibbonPage()
        {
            InitializeComponent();
        }
    }
}
