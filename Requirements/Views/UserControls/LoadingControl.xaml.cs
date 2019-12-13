namespace CDP4Requirements.Views.UserControls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for LoadingControl.xaml
    /// </summary>
    public partial class LoadingControl : UserControl
    {
        public LoadingControl()
        {
            this.InitializeComponent();
        }

        public int Diameter
        {
            get => (int)this.GetValue(DiameterProperty);
            set
            {
                if (value < 10)
                {
                    value = 10;
                }

                this.SetValue(DiameterProperty, value);
            }
        }

        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register("Diameter", typeof(int), typeof(LoadingControl), new PropertyMetadata(20, OnDiameterPropertyChanged));

        private static void OnDiameterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var vm = (LoadingControl)d;
            d.CoerceValue(CenterProperty);
            d.CoerceValue(RadiusProperty);
            d.CoerceValue(InnerRadiusProperty);
        }

        public int Radius
        {
            get => (int)this.GetValue(RadiusProperty);
            set => this.SetValue(RadiusProperty, value);
        }

        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(int), typeof(LoadingControl), new PropertyMetadata(15, null, OnCoerceRadius));

        private static object OnCoerceRadius(DependencyObject d, object baseValue)
        {
            var control = (LoadingControl)d;
            var newRadius = (int)control.GetValue(DiameterProperty) / 2;

            return newRadius;
        }

        public int InnerRadius
        {
            get => (int)this.GetValue(InnerRadiusProperty);
            set => this.SetValue(InnerRadiusProperty, value);
        }

        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadius", typeof(int), typeof(LoadingControl), new PropertyMetadata(2, null, OnCoerceInnerRadius));

        private static object OnCoerceInnerRadius(DependencyObject d, object baseValue)
        {
            var control = (LoadingControl)d;
            var newInnerRadius = (int)control.GetValue(DiameterProperty) / 4;

            return newInnerRadius;
        }

        public Point Center
        {
            get => (Point)this.GetValue(CenterProperty);
            set => this.SetValue(CenterProperty, value);
        }

        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(Point), typeof(LoadingControl), new PropertyMetadata(new Point(15, 15), null, OnCoerceCenter));

        private static object OnCoerceCenter(DependencyObject d, object baseValue)
        {
            var control = (LoadingControl)d;
            var newCenter = (int)control.GetValue(DiameterProperty) / 2;

            return new Point(newCenter, newCenter);
        }

        public Color Color1
        {
            get => (Color)this.GetValue(Color1Property);
            set => this.SetValue(Color1Property, value);
        }

        public static readonly DependencyProperty Color1Property =
            DependencyProperty.Register("Color1", typeof(Color), typeof(LoadingControl), new PropertyMetadata(Colors.Green));

        public Color Color2
        {
            get => (Color)this.GetValue(Color2Property);
            set => this.SetValue(Color2Property, value);
        }

        public static readonly DependencyProperty Color2Property =
            DependencyProperty.Register("Color2", typeof(Color), typeof(LoadingControl), new PropertyMetadata(Colors.Transparent));
    }
}
