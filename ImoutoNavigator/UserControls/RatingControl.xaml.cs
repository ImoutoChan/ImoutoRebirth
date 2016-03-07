using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro;

namespace Imouto.Navigator.UserControls
{
    /// <summary>
    ///     Interaction logic for RatingControl.xaml
    /// </summary>
    public partial class RatingControl : UserControl
    {
        int _ratingUnderMouse = 0;

        public RatingControl()
        {
            InitializeComponent();
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
            GetRating(sender as Border);
            SetImage(_ratingUnderMouse, imgYellowVisibility, imgGrayVisibility);
        }

        private void SetImage(int value, Visibility imgYellowVisibility, Visibility imgGrayVisibility)
        {
            var plusImages = pnlPlus.Children.OfType<Border>().ToList();
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
            GetRating(sender as Border);
            Value = _ratingUnderMouse;
        }

        private void GetRating(Border img)
        {
            string strImgName = img.Name;
            _ratingUnderMouse = Convert.ToInt32(strImgName.Substring(strImgName.Length - 1, 1));
        }
    }
}
