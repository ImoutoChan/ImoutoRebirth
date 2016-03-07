using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Imouto.Navigator.UserControls
{
    /// <summary>
    ///     Interaction logic for RatingControl.xaml
    /// </summary>
    public partial class RatingControl : UserControl
    {
        int _ratingUnderMouse = 0;
        //int intCount = 1;
        //int Rate = 0;

        public RatingControl()
        {
            InitializeComponent();
            LoadImages();
        }
        
        /// <summary>
        ///     Value Dependency Property
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof (int), typeof (RatingControl), new PropertyMetadata(0, OnValueChanged));

        /// <summary>
        ///     Gets or sets the Value property.
        /// </summary>
        public int Value
        {
            get
            {
                try
                {
                    return (int) GetValue(ValueProperty);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                SetValue(ValueProperty, value);
            }
        }

        /// <summary>
        ///     Handles changes to the Value property.
        /// </summary>
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rating = (int) e.NewValue;
            var control = (RatingControl) d;

            control.SetImage(rating, Visibility.Visible, Visibility.Hidden);
        }
        
        private void LoadImages()
        {
            for (int i = 1; i <= 5; i++)
            {
                Image img = new Image();
                img.Name = "imgRate" + i;
                img.Stretch = Stretch.UniformToFill;
                img.Height = 25;
                img.Width = 25;
                img.Source = new BitmapImage(new Uri(@"..\Resources\Icon\MinusRate.png", UriKind.Relative));
                img.MouseEnter += imgRateMinus_MouseEnter;
                pnlMinus.Children.Add(img);

                Image img1 = new Image();
                img1.Name = "imgRate" + i + i;
                img1.Stretch = Stretch.UniformToFill;
                img1.Height = 25;
                img1.Width = 25;
                img1.Visibility = Visibility.Hidden;
                img1.Source = new BitmapImage(new Uri(@"..\Resources\Icon\PlusRate.png", UriKind.Relative));
                img1.MouseEnter += imgRatePlus_MouseEnter;
                img1.MouseLeave += imgRatePlus_MouseLeave;
                img1.MouseLeftButtonUp += imgRatePlus_MouseLeftButtonUp;
                pnlPlus.Children.Add(img1);
            }
        }

        private void imgRateMinus_MouseEnter(object sender, MouseEventArgs e)
        {
            GetData(sender, Visibility.Visible, Visibility.Hidden);
        }

        private void imgRatePlus_MouseEnter(object sender, MouseEventArgs e)
        {
            GetData(sender, Visibility.Visible, Visibility.Hidden);
        }

        private void imgRatePlus_MouseLeave(object sender, MouseEventArgs e)
        {
            GetData(sender, Visibility.Hidden, Visibility.Visible);
        }

        private void gdRating_MouseLeave(object sender, MouseEventArgs e)
        {
            SetImage(Value, Visibility.Visible, Visibility.Hidden);
        }

        private void GetData(object sender, Visibility imgYellowVisibility, Visibility imgGrayVisibility)
        {
            GetRating(sender as Image);
            SetImage(_ratingUnderMouse, imgYellowVisibility, imgGrayVisibility);
        }

        private void SetImage(int value, Visibility imgYellowVisibility, Visibility imgGrayVisibility)
        {
            var plusImages = pnlPlus.Children.OfType<Image>().ToList();
            for (int i = 0; i < plusImages.Count(); i++)
            {
                var image = plusImages[i];

                image.Visibility = i + 1 <= value
                                   ? imgYellowVisibility
                                   : imgGrayVisibility;
            }
        }

        private void imgRatePlus_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GetRating(sender as Image);
            Value = _ratingUnderMouse;
        }

        private void GetRating(Image img)
        {
            string strImgName = img.Name;
            _ratingUnderMouse = Convert.ToInt32(strImgName.Substring(strImgName.Length - 1, 1));
        }
    }
}
