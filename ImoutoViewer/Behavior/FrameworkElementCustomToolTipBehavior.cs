using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using ImoutoViewer.UserControls;
using Utils;
using Utils.WPFHelpers;

namespace ImoutoViewer.Behavior
{
    class FrameworkElementCustomToolTipBehavior : Behavior<FrameworkElement>
    {
        private const int MinToolTipSize = 150;
        ObservableCollection<Inline> _inlineCollection = new ObservableCollection<Inline>();
        

        public ObservableCollection<Inline> InlineList
        {
            get { return (ObservableCollection<Inline>)GetValue(InlineListProperty); }
            set { SetValue(InlineListProperty, value); }
        }

        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register("InlineList", typeof(ObservableCollection<Inline>), typeof(FrameworkElementCustomToolTipBehavior), new UIPropertyMetadata(null, OnInlineListPropertyChanged));

        private static void OnInlineListPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElementCustomToolTipBehavior beh = sender as FrameworkElementCustomToolTipBehavior;
            ObservableCollection<Inline> list = e.NewValue as ObservableCollection<Inline>;
            if (list != null)
            {
                beh._inlineCollection.Clear();
                list.ForEach(x => beh._inlineCollection.Add(x));
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
        }

        private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {

                var target = sender as UIElement;
                var mainWindow = UIHelper.FindVisualParent<Window>(target);
                var area = (UIElement) mainWindow.FindName("GridParent");
                var toolTipBorder = (FrameworkElement) mainWindow.FindName("ToolTipBorder");
                var toolTipTextBlock = (TextBlock) mainWindow.FindName("ToolTipTextBlock");

                var targetPosition = target.TransformToAncestor(area).Transform(new Point(0, 0));
                var targetSize = target.RenderSize;
                var areaSize = area.RenderSize;

                var bottomSpace = areaSize.Height - targetPosition.Y - targetSize.Height;
                var topSpace = targetPosition.Y;
                var leftSpace = targetPosition.X;
                var rightSpace = areaSize.Width - targetPosition.X - targetSize.Width;

                var toolTipWidth = (targetSize.Width < MinToolTipSize) ? MinToolTipSize : targetSize.Width;
                var desiredTooltipSize = GetTextBlockSize(toolTipWidth);

                //Select placement

                HorisontalPosition horisontalPosition;
                VerticalPosition verticalPosition;

                GetToolTipPosition(desiredTooltipSize, bottomSpace, rightSpace, targetSize, leftSpace, topSpace,
                    out horisontalPosition, out verticalPosition);

                if (horisontalPosition == HorisontalPosition.Nowhere || verticalPosition == VerticalPosition.Nowhere)
                {
                    return;
                }

                var tooltipPosition = new Point();
                HorizontalAlignment horisontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment verticalAlignment = VerticalAlignment.Top;

                switch (verticalPosition)
                {
                    case VerticalPosition.Bottom:
                        tooltipPosition.Y = targetPosition.Y + targetSize.Height;
                        break;
                    case VerticalPosition.Top:
                        tooltipPosition.Y = targetPosition.Y - desiredTooltipSize.Height;
                        break;
                    case VerticalPosition.ToBottom:
                        tooltipPosition.Y = targetPosition.Y;
                        break;
                    case VerticalPosition.ToTop:
                        tooltipPosition.Y = targetPosition.Y + targetSize.Height - desiredTooltipSize.Height;
                        break;
                    case VerticalPosition.Anywhere:
                        tooltipPosition.Y = areaSize.Height - desiredTooltipSize.Height;
                        break;
                }

                switch (horisontalPosition)
                {
                    case HorisontalPosition.Rigth:
                        tooltipPosition.X = targetPosition.X + targetSize.Width;
                        break;
                    case HorisontalPosition.Left:
                        tooltipPosition.X = targetPosition.X - desiredTooltipSize.Width;
                        break;
                    case HorisontalPosition.ToRight:
                        tooltipPosition.X = targetPosition.X;
                        break;
                    case HorisontalPosition.ToLeft:
                        tooltipPosition.X = targetPosition.X + targetSize.Width - desiredTooltipSize.Width;
                        break;
                    case HorisontalPosition.Anywhere:
                        tooltipPosition.X = areaSize.Width - desiredTooltipSize.Width;
                        break;
                }

                toolTipBorder.Visibility = Visibility.Collapsed;

                toolTipBorder.Width = toolTipWidth;
                toolTipBorder.HorizontalAlignment = horisontalAlignment;
                toolTipBorder.VerticalAlignment = verticalAlignment;

                toolTipBorder.Margin = new Thickness(tooltipPosition.X, tooltipPosition.Y, 5, 5);
                toolTipTextBlock.Inlines.Clear();
                toolTipTextBlock.Inlines.AddRange(InlineList);

                toolTipBorder.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
            }
        }

        private static void GetToolTipPosition(Size desiredTooltipSize, double bottomSpace,
            double rightSpace, Size targetSize, double leftSpace, double topSpace, out HorisontalPosition horisontalPosition, out VerticalPosition verticalPosition)
        {
            if (desiredTooltipSize.Height <= bottomSpace)
            {
                verticalPosition = VerticalPosition.Bottom;

                if (rightSpace + targetSize.Width >= desiredTooltipSize.Width)
                {
                    horisontalPosition = HorisontalPosition.ToRight;
                }
                else if (leftSpace + targetSize.Width >= desiredTooltipSize.Width)
                {
                    horisontalPosition = HorisontalPosition.ToLeft;
                }
                else if (rightSpace + leftSpace + targetSize.Width >= desiredTooltipSize.Width)
                {
                    horisontalPosition = HorisontalPosition.Anywhere;
                }
                else
                {
                    horisontalPosition = HorisontalPosition.Nowhere;
                }
            }
            else if (desiredTooltipSize.Height <= topSpace)
            {
                verticalPosition = VerticalPosition.Top;

                if (rightSpace + targetSize.Width >= desiredTooltipSize.Width)
                {
                    horisontalPosition = HorisontalPosition.ToRight;
                }
                else if (leftSpace + targetSize.Width >= desiredTooltipSize.Width)
                {
                    horisontalPosition = HorisontalPosition.ToLeft;
                }
                else if (rightSpace + leftSpace + targetSize.Width >= desiredTooltipSize.Width)
                {
                    horisontalPosition = HorisontalPosition.Anywhere;
                }
                else
                {
                    horisontalPosition = HorisontalPosition.Nowhere;
                }
            }
            else if (desiredTooltipSize.Width <= rightSpace)
            {
                horisontalPosition = HorisontalPosition.Rigth;

                if (bottomSpace + targetSize.Height >= desiredTooltipSize.Height)
                {
                    verticalPosition = VerticalPosition.ToBottom;
                }
                else if (topSpace + targetSize.Height >= desiredTooltipSize.Height)
                {
                    verticalPosition = VerticalPosition.ToTop;
                }
                else if (topSpace + bottomSpace + targetSize.Height >= desiredTooltipSize.Height)
                {
                    verticalPosition = VerticalPosition.Anywhere;
                }
                else
                {
                    verticalPosition = VerticalPosition.Nowhere;
                }
            }
            else if (desiredTooltipSize.Width <= leftSpace)
            {
                horisontalPosition = HorisontalPosition.Left;

                if (bottomSpace + targetSize.Height >= desiredTooltipSize.Height)
                {
                    verticalPosition = VerticalPosition.ToBottom;
                }
                else if (topSpace + targetSize.Height >= desiredTooltipSize.Height)
                {
                    verticalPosition = VerticalPosition.ToTop;
                }
                else if (topSpace + bottomSpace + targetSize.Height >= desiredTooltipSize.Height)
                {
                    verticalPosition = VerticalPosition.Anywhere;
                }
                else
                {
                    verticalPosition = VerticalPosition.Nowhere;
                }
            }
            else
            {
                horisontalPosition = HorisontalPosition.Nowhere;
                verticalPosition = VerticalPosition.Nowhere;
            }
        }

        private Size GetTextBlockSize(double toolTipWidth)
        {
            var textBlock = new TextBlock();
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange(InlineList.ToList());
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Margin = new Thickness(5);

            textBlock.Measure(new Size(toolTipWidth, Double.PositiveInfinity));
            textBlock.Arrange(new Rect(textBlock.DesiredSize));

            return new Size(textBlock.ActualWidth, textBlock.ActualHeight);
        }

        void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
            AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
        }

        private enum VerticalPosition
        {
            ToTop,
            Top,
            Bottom,
            ToBottom,
            Anywhere,
            Nowhere
        }

        private enum HorisontalPosition
        {
            ToLeft,
            Left,
            Rigth,
            ToRight,
            Anywhere,
            Nowhere
        }
    }
}
