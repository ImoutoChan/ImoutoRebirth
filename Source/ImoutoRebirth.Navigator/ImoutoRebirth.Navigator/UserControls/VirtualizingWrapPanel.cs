using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ImoutoRebirth.Navigator.UserControls;

public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
{
    #region ItemSize

    #region ItemWidth

    /// <summary>
    /// <see cref="ItemWidth"/> 依存関係プロパティの識別子。
    /// </summary>
    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(
            nameof(ItemWidth),
            typeof(double),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(
                double.NaN,
                FrameworkPropertyMetadataOptions.AffectsMeasure
            ),
            IsWidthHeightValid
        );

    /// <summary>
    /// VirtualizingWrapPanel 内に含まれているすべての項目の幅を
    /// 指定する値を取得、または設定する。
    /// </summary>
    [TypeConverter(typeof(LengthConverter)), Category("共通")]
    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    #endregion

    #region ItemHeight

    /// <summary>
    /// <see cref="ItemHeight"/> 依存関係プロパティの識別子。
    /// </summary>
    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(
            nameof(ItemHeight),
            typeof(double),
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(
                double.NaN,
                FrameworkPropertyMetadataOptions.AffectsMeasure
            ),
            IsWidthHeightValid
        );

    /// <summary>
    /// VirtualizingWrapPanel 内に含まれているすべての項目の高さを
    /// 指定する値を取得、または設定する。
    /// </summary>
    [TypeConverter(typeof(LengthConverter)), Category("共通")]
    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    #endregion

    #region IsWidthHeightValid
    /// <summary>
    /// <see cref="ItemWidth"/>, <see cref="ItemHeight"/> に設定された値が
    /// 有効かどうかを検証するコールバック。
    /// </summary>
    /// <param name="value">プロパティに設定された値。</param>
    /// <returns>値が有効な場合は true、無効な場合は false。</returns>
    private static bool IsWidthHeightValid(object value)
    {
        var d = (double)value;
        return double.IsNaN(d) || ((d >= 0) && !double.IsPositiveInfinity(d));
    }
    #endregion

    #endregion

    #region Orientation

    /// <summary>
    /// <see cref="Orientation"/> 依存関係プロパティの識別子。
    /// </summary>
    public static readonly DependencyProperty OrientationProperty =
        WrapPanel.OrientationProperty.AddOwner(
            typeof(VirtualizingWrapPanel),
            new FrameworkPropertyMetadata(
                Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnOrientationChanged
            )
        );

    /// <summary>
    /// 子コンテンツが配置される方向を指定する値を取得、または設定する。
    /// </summary>
    [Category("共通")]
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// <see cref="Orientation"/> 依存関係プロパティが変更されたときに呼び出されるコールバック。
    /// </summary>
    /// <param name="d">プロパティの値が変更された <see cref="System.Windows.DependencyObject"/>。</param>
    /// <param name="e">このプロパティの有効値に対する変更を追跡するイベントによって発行されるイベントデータ。</param>
    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (VirtualizingWrapPanel)d;
        panel._offset = default;
        panel.InvalidateMeasure();
    }

    #endregion

    #region MeasureOverride, ArrangeOverride

    /// <summary>
    /// 指定したインデックスのアイテムの位置およびサイズを記憶するディクショナリ。
    /// </summary>
    private Dictionary<int, Rect> _containerLayouts = new Dictionary<int, Rect>();

    /// <summary>
    /// 子要素に必要なレイアウトのサイズを測定し、パネルのサイズを決定する。
    /// </summary>
    /// <param name="availableSize">子要素に与えることができる使用可能なサイズ。</param>
    /// <returns>レイアウト時にこのパネルが必要とするサイズ。</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var maxSize = default(Size);
        try
        {
            _containerLayouts.Clear();

            var isAutoWidth = double.IsNaN(ItemWidth);
            var isAutoHeight = double.IsNaN(ItemHeight);
            var childAvailable = new Size(isAutoWidth
                ? double.PositiveInfinity
                : ItemWidth, isAutoHeight
                ? double.PositiveInfinity
                : ItemHeight);
            var isHorizontal = Orientation == Orientation.Horizontal;

            var childrenCount = InternalChildren.Count;

            var itemsControl = ItemsControl.GetItemsOwner(this);
            if (itemsControl != null)
                childrenCount = itemsControl.Items.Count;

            var generator = new ChildGenerator(this);

            var x = 0.0;
            var y = 0.0;
            var lineSize = default(Size);

            for (int i = 0; i < childrenCount; i++)
            {
                var childSize = ContainerSizeForIndex(i);

                // ビューポートとの交差判定用に仮サイズで x, y を調整
                var isWrapped = isHorizontal
                    ? lineSize.Width + childSize.Width > availableSize.Width
                    : lineSize.Height + childSize.Height > availableSize.Height;
                if (isWrapped)
                {
                    x = isHorizontal
                        ? 0
                        : x + lineSize.Width;
                    y = isHorizontal
                        ? y + lineSize.Height
                        : 0;
                }

                // 子要素がビューポート内であれば子要素を生成しサイズを再計測
                var itemRect = new Rect(x, y, childSize.Width, childSize.Height);
                var viewportRect = new Rect(_offset, availableSize);
                if (itemRect.IntersectsWith(viewportRect))
                {
                    var child = generator.GetOrCreateChild(i);
                    child?.Measure(childAvailable);
                    childSize = ContainerSizeForIndex(i);
                }

                // 確定したサイズを記憶
                _containerLayouts[i] = new Rect(x, y, childSize.Width, childSize.Height);

                // lineSize, maxSize を計算
                isWrapped = isHorizontal
                    ? lineSize.Width + childSize.Width > availableSize.Width
                    : lineSize.Height + childSize.Height > availableSize.Height;
                if (isWrapped)
                {
                    maxSize.Width = isHorizontal
                        ? Math.Max(lineSize.Width, maxSize.Width)
                        : maxSize.Width + lineSize.Width;
                    maxSize.Height = isHorizontal
                        ? maxSize.Height + lineSize.Height
                        : Math.Max(lineSize.Height, maxSize.Height);
                    lineSize = childSize;

                    isWrapped = isHorizontal
                        ? childSize.Width > availableSize.Width
                        : childSize.Height > availableSize.Height;
                    if (isWrapped)
                    {
                        maxSize.Width = isHorizontal
                            ? Math.Max(childSize.Width, maxSize.Width)
                            : maxSize.Width + childSize.Width;
                        maxSize.Height = isHorizontal
                            ? maxSize.Height + childSize.Height
                            : Math.Max(childSize.Height, maxSize.Height);
                        lineSize = default(Size);
                    }
                }
                else
                {
                    lineSize.Width = isHorizontal
                        ? lineSize.Width + childSize.Width
                        : Math.Max(childSize.Width, lineSize.Width);
                    lineSize.Height = isHorizontal
                        ? Math.Max(childSize.Height, lineSize.Height)
                        : lineSize.Height + childSize.Height;
                }

                x = isHorizontal
                    ? lineSize.Width
                    : maxSize.Width;
                y = isHorizontal
                    ? maxSize.Height
                    : lineSize.Height;
            }

            maxSize.Width = isHorizontal
                ? Math.Max(lineSize.Width, maxSize.Width)
                : maxSize.Width + lineSize.Width;
            maxSize.Height = isHorizontal
                ? maxSize.Height + lineSize.Height
                : Math.Max(lineSize.Height, maxSize.Height);

            _extent = maxSize;
            _viewport = availableSize;

            generator.CleanupChildren();
            generator?.Dispose();

            ScrollOwner?.InvalidateScrollInfo();
        }
        catch
        {
            // ignore
        }

        return maxSize;
    }

    #region ChildGenerator
    /// <summary>
    /// <see cref="VirtualizingWrapPanel"/> のアイテムを管理する。
    /// </summary>
    private class ChildGenerator : IDisposable
    {
        #region fields

        /// <summary>
        /// アイテムを生成する対象の <see cref="VirtualizingWrapPanel"/>。
        /// </summary>
        private VirtualizingWrapPanel _owner;

        /// <summary>
        /// <see cref="_owner"/> の <see cref="System.Windows.Controls.ItemContainerGenerator"/>。
        /// </summary>
        private IItemContainerGenerator? _generator;

        /// <summary>
        /// <see cref="_generator"/> の生成プロセスの有効期間を追跡するオブジェクト。
        /// </summary>
        private IDisposable? _generatorTracker;

        /// <summary>
        /// 表示範囲内にある最初の要素のインデックス。
        /// </summary>
        private int _firstGeneratedIndex;

        /// <summary>
        /// 表示範囲内にある最後の要素のインデックス。
        /// </summary>
        private int _lastGeneratedIndex;

        /// <summary>
        /// 次に生成される要素の <see cref="System.Windows.Controls.Panel.InternalChildren"/> 内のインデックス。
        /// </summary>
        private int _currentGenerateIndex;

        #endregion

        #region _ctor

        /// <summary>
        /// <see cref="ChildGenerator"/> の新しいインスタンスを生成する。
        /// </summary>
        /// <param name="owner">アイテムを生成する対象の <see cref="VirtualizingWrapPanel"/>。</param>
        public ChildGenerator(VirtualizingWrapPanel owner)
        {
            _owner = owner;

            // ItemContainerGenerator 取得前に InternalChildren にアクセスしないと null になる
            _ = owner.InternalChildren.Count;
            _generator = owner.ItemContainerGenerator;
        }

        /// <summary>
        /// <see cref="ChildGenerator"/> のインスタンスを破棄する。
        /// </summary>
        ~ChildGenerator()
        {
            Dispose();
        }

        /// <summary>
        /// アイテムの生成を終了する。
        /// </summary>
        public void Dispose()
        {
            if (_generatorTracker != null)
                _generatorTracker.Dispose();
        }

        #endregion

        #region GetOrCreateChild

        /// <summary>
        /// アイテムの生成を開始する。
        /// </summary>
        /// <param name="index">アイテムのインデックス。</param>
        private void BeginGenerate(int index)
        {
            _firstGeneratedIndex = index;
            var startPos = _generator!.GeneratorPositionFromIndex(index);
            _currentGenerateIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
            _generatorTracker = _generator.StartAt(startPos, GeneratorDirection.Forward, true);
        }

        /// <summary>
        /// 必要に応じてアイテムを生成し、指定したインデックスのアイテムを取得する。
        /// </summary>
        /// <param name="index">取得するアイテムのインデックス。</param>
        /// <returns>指定したインデックスのアイテム。</returns>
        public UIElement? GetOrCreateChild(int index)
        {
            if (_generator == null)
                return _owner.InternalChildren[index];

            if (_generatorTracker == null)
                BeginGenerate(index);

            var child = _generator.GenerateNext(out var newlyRealized) as UIElement;
            if (newlyRealized && child != null)
            {
                if (_currentGenerateIndex >= _owner.InternalChildren.Count)
                    _owner.AddInternalChild(child);
                else
                    _owner.InsertInternalChild(_currentGenerateIndex, child);

                _generator.PrepareItemContainer(child);
            }

            _lastGeneratedIndex = index;
            _currentGenerateIndex++;

            return child;
        }

        #endregion

        #region CleanupChildren
        /// <summary>
        /// 表示範囲外のアイテムを削除する。
        /// </summary>
        public void CleanupChildren()
        {
            if (_generator == null)
                return;

            var children = _owner.InternalChildren;

            for (int i = children.Count - 1; i >= 0; i--)
            {
                var childPos = new GeneratorPosition(i, 0);
                var index = _generator.IndexFromGeneratorPosition(childPos);
                if (index < _firstGeneratedIndex || index > _lastGeneratedIndex)
                {
                    _generator.Remove(childPos, 1);
                    _owner.RemoveInternalChildRange(i, 1);
                }
            }
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// 子要素を配置し、パネルのサイズを決定する。
    /// </summary>
    /// <param name="finalSize">パネル自体と子要素を配置するために使用する親の末尾の領域。</param>
    /// <returns>使用する実際のサイズ。</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (UIElement child in InternalChildren)
        {
            var gen = ItemContainerGenerator as ItemContainerGenerator;
            var index = (gen != null) ? gen.IndexFromContainer(child) : InternalChildren.IndexOf(child);
            if (_containerLayouts.ContainsKey(index))
            {
                var layout = _containerLayouts[index];
                layout.Offset(_offset.X * -1, _offset.Y * -1);
                child.Arrange(layout);
            }
        }

        return finalSize;
    }

    #endregion

    #region ContainerSizeForIndex

    /// <summary>
    /// 直前にレイアウトした要素のサイズ。
    /// </summary>
    /// <remarks>
    /// <see cref="System.Windows.DataTemplate"/> 使用時、全要素のサイズが一致することを前提に、
    /// 要素のサイズの推定に使用する。
    /// </remarks>
    private Size _prevSize = new Size(16, 16);

    /// <summary>
    /// 指定したインデックスに対するアイテムのサイズを、実際にアイテムを生成せずに推定する。
    /// </summary>
    /// <param name="index">アイテムのインデックス。</param>
    /// <returns>指定したインデックスに対するアイテムの推定サイズ。</returns>
    private Size ContainerSizeForIndex(int index)
    {
        var getSize = new Func<int, Size>(idx =>
        {
            UIElement? item = null;
            var itemsOwner = ItemsControl.GetItemsOwner(this);
            var generator = ItemContainerGenerator as ItemContainerGenerator;

            if (itemsOwner == null || generator == null)
            {
                // VirtualizingWrapPanel 単体で使用されている場合、自身のアイテムを返す
                if (InternalChildren.Count > idx)
                    item = InternalChildren[idx];
            }
            else
            {
                // generator がアイテムを未生成の場合、Items が使えればそちらを使う
                if (generator.ContainerFromIndex(idx) != null)
                    item = generator.ContainerFromIndex(idx) as UIElement;
                else if (itemsOwner.Items.Count > idx)
                    item = itemsOwner.Items[idx] as UIElement;
            }

            if (item != null)
            {
                // アイテムのサイズが測定済みであればそのサイズを返す
                if (item.IsMeasureValid)
                    return item.DesiredSize;

                // アイテムのサイズが未測定の場合、推奨値を使う
                var i = item as FrameworkElement;
                if (i != null)
                    return new Size(i.Width, i.Height);
            }

            // 前回の測定値があればそちらを使う
            if (_containerLayouts.ContainsKey(idx))
                return _containerLayouts[idx].Size;

            // 有効なサイズが取得できなかった場合、直前のアイテムのサイズを返す
            return _prevSize;
        });

        var size = getSize(index);

        // ItemWidth, ItemHeight が指定されていれば調整する
        if (!double.IsNaN(ItemWidth))
            size.Width = ItemWidth;
        if (!double.IsNaN(ItemHeight))
            size.Height = ItemHeight;

        return _prevSize = size;
    }

    #endregion

    #region OnItemsChanged
    /// <summary>
    /// このパネルの <see cref="System.Windows.Controls.ItemsControl"/> に関連付けられている
    /// <see cref="System.Windows.Controls.ItemsControl.Items"/> コレクションが変更されたときに
    /// 呼び出されるコールバック。
    /// </summary>
    /// <param name="sender">イベントを発生させた <see cref="System.Object"/></param>
    /// <param name="args">イベントデータ。</param>
    /// <remarks>
    /// <see cref="System.Windows.Controls.ItemsControl.Items"/> が変更された際
    /// <see cref="System.Windows.Controls.Panel.InternalChildren"/> にも反映する。
    /// </remarks>
    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                break;
        }
    }
    #endregion

    #region IScrollInfo Members

    #region Extent

    /// <summary>
    /// エクステントのサイズ。
    /// </summary>
    private Size _extent = default(Size);

    /// <summary>
    /// エクステントの縦幅を取得する。
    /// </summary>
    public double ExtentHeight => _extent.Height;

    /// <summary>
    /// エクステントの横幅を取得する。
    /// </summary>
    public double ExtentWidth => _extent.Width;

    #endregion Extent

    #region Viewport

    /// <summary>
    /// ビューポートのサイズ。
    /// </summary>
    private Size _viewport = default(Size);

    /// <summary>
    /// このコンテンツに対するビューポートの縦幅を取得する。
    /// </summary>
    public double ViewportHeight => _viewport.Height;

    /// <summary>
    /// このコンテンツに対するビューポートの横幅を取得する。
    /// </summary>
    public double ViewportWidth => _viewport.Width;

    #endregion

    #region Offset

    /// <summary>
    /// スクロールしたコンテンツのオフセット。
    /// </summary>
    private Point _offset;

    /// <summary>
    /// スクロールしたコンテンツの水平オフセットを取得する。
    /// </summary>
    public double HorizontalOffset => _offset.X;

    /// <summary>
    /// スクロールしたコンテンツの垂直オフセットを取得する。
    /// </summary>
    public double VerticalOffset => _offset.Y;

    #endregion

    #region ScrollOwner
    /// <summary>
    /// スクロール動作を制御する <see cref="System.Windows.Controls.ScrollViewer"/> 要素を
    /// 取得、または設定する。
    /// </summary>
    public ScrollViewer? ScrollOwner { get; set; }
    #endregion

    #region CanHorizontallyScroll
    /// <summary>
    /// 水平軸のスクロールが可能かどうかを示す値を取得、または設定する。
    /// </summary>
    public bool CanHorizontallyScroll { get; set; }
    #endregion

    #region CanVerticallyScroll
    /// <summary>
    /// 垂直軸のスクロールが可能かどうかを示す値を取得、または設定する。
    /// </summary>
    public bool CanVerticallyScroll { get; set; }
    #endregion

    #region LineUp
    /// <summary>
    /// コンテンツ内を 1 論理単位ずつ上にスクロールする。
    /// </summary>
    public void LineUp()
    {
        //this.SetVerticalOffset(this.VerticalOffset - SystemParameters.ScrollHeight);

        var currentTopLine = VerticalOffset / ItemHeight - 1;
        SetVerticalOffset(Math.Ceiling(currentTopLine) * ItemHeight);
    }
    #endregion

    #region LineDown
    /// <summary>
    /// コンテンツ内を 1 論理単位ずつ下にスクロールする。
    /// </summary>
    public void LineDown()
    {
        //this.SetVerticalOffset(this.VerticalOffset + SystemParameters.ScrollHeight);

        var currentBottomLine = (VerticalOffset + ViewportHeight) / ItemHeight + 1;
        SetVerticalOffset(Math.Floor(currentBottomLine) * ItemHeight - ViewportHeight);
    }
    #endregion

    #region LineLeft
    /// <summary>
    /// コンテンツ内を 1 論理単位ずつ左にスクロールする。
    /// </summary>
    public void LineLeft()
    {
        SetHorizontalOffset(HorizontalOffset - SystemParameters.ScrollWidth);
    }
    #endregion

    #region LineRight
    /// <summary>
    /// コンテンツ内を 1 論理単位ずつ右にスクロールする。
    /// </summary>
    public void LineRight()
    {
        SetHorizontalOffset(HorizontalOffset + SystemParameters.ScrollWidth);
    }
    #endregion

    #region PageUp
    /// <summary>
    /// コンテンツ内を 1 ページずつ上にスクロールする。
    /// </summary>
    public void PageUp()
    {
        SetVerticalOffset(VerticalOffset - _viewport.Height);
    }
    #endregion

    #region PageDown
    /// <summary>
    /// コンテンツ内を 1 ページずつ下にスクロールする。
    /// </summary>
    public void PageDown()
    {
        SetVerticalOffset(VerticalOffset + _viewport.Height);
    }
    #endregion

    #region PageLeft
    /// <summary>
    /// コンテンツ内を 1 ページずつ左にスクロールする。
    /// </summary>
    public void PageLeft()
    {
        SetHorizontalOffset(HorizontalOffset - _viewport.Width);
    }
    #endregion

    #region PageRight
    /// <summary>
    /// コンテンツ内を 1 ページずつ右にスクロールする。
    /// </summary>
    public void PageRight()
    {
        SetHorizontalOffset(HorizontalOffset + _viewport.Width);
    }
    #endregion

    #region MouseWheelUp
    /// <summary>
    /// ユーザがマウスのホイールボタンをクリックした後に、コンテンツ内を上にスクロールする。
    /// </summary>
    public void MouseWheelUp()
    {
        SetVerticalOffset(VerticalOffset - SystemParameters.ScrollHeight * SystemParameters.WheelScrollLines);
    }
    #endregion

    #region MouseWheelDown
    /// <summary>
    /// ユーザがマウスのホイールボタンをクリックした後に、コンテンツ内を下にスクロールする。
    /// </summary>
    public void MouseWheelDown()
    {
        SetVerticalOffset(VerticalOffset + SystemParameters.ScrollHeight * SystemParameters.WheelScrollLines);
    }
    #endregion

    #region MouseWheelLeft
    /// <summary>
    /// ユーザがマウスのホイールボタンをクリックした後に、コンテンツ内を左にスクロールする。
    /// </summary>
    public void MouseWheelLeft()
    {
        SetHorizontalOffset(HorizontalOffset - SystemParameters.ScrollWidth * SystemParameters.WheelScrollLines);
    }
    #endregion

    #region MouseWheelRight
    /// <summary>
    /// ユーザがマウスのホイールボタンをクリックした後に、コンテンツ内を右にスクロールする。
    /// </summary>
    public void MouseWheelRight()
    {
        SetHorizontalOffset(HorizontalOffset + SystemParameters.ScrollWidth * SystemParameters.WheelScrollLines);
    }
    #endregion

    #region MakeVisible
    /// <summary>
    /// <see cref="System.Windows.Media.Visual"/> オブジェクトの座標空間が表示されるまで、
    /// コンテンツを強制的にスクロールする。
    /// </summary>
    /// <param name="visual">表示可能になる <see cref="System.Windows.Media.Visual"/>。</param>
    /// <param name="rectangle">表示する座標空間を識別する外接する四角形。</param>
    /// <returns>表示される <see cref="System.Windows.Rect"/>。</returns>
    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        var idx = InternalChildren.IndexOf(visual as UIElement);

        if (ItemContainerGenerator is { } generator)
        {
            var pos = new GeneratorPosition(idx, 0);
            idx = generator.IndexFromGeneratorPosition(pos);
        }

        if (idx < 0)
            return Rect.Empty;

        if (!_containerLayouts.ContainsKey(idx))
            return Rect.Empty;

        var layout = _containerLayouts[idx];

        if (HorizontalOffset + ViewportWidth < layout.X + layout.Width)
            SetHorizontalOffset(layout.X + layout.Width - ViewportWidth);
        if (layout.X < HorizontalOffset)
            SetHorizontalOffset(layout.X);

        if (VerticalOffset + ViewportHeight < layout.Y + layout.Height)
            SetVerticalOffset(layout.Y + layout.Height - ViewportHeight);
        if (layout.Y < VerticalOffset)
            SetVerticalOffset(layout.Y);

        layout.Width = Math.Min(ViewportWidth, layout.Width);
        layout.Height = Math.Min(ViewportHeight, layout.Height);

        return layout;
    }
    #endregion

    #region SetHorizontalOffset
    /// <summary>
    /// 水平オフセットの値を設定する。
    /// </summary>
    /// <param name="offset">包含するビューポートからのコンテンツの水平方向オフセットの程度。</param>
    public void SetHorizontalOffset(double offset)
    {
        if (offset < 0 || ViewportWidth >= ExtentWidth)
        {
            offset = 0;
        }
        else
        {
            if (offset + ViewportWidth >= ExtentWidth)
                offset = ExtentWidth - ViewportWidth;
        }

        _offset.X = offset;

        if (ScrollOwner != null)
            ScrollOwner.InvalidateScrollInfo();

        InvalidateMeasure();
    }
    #endregion

    #region SetVerticalOffset
    /// <summary>
    /// 垂直オフセットの値を設定する。
    /// </summary>
    /// <param name="offset">包含するビューポートからの垂直方向オフセットの程度。</param>
    public void SetVerticalOffset(double offset)
    {
        if (offset < 0 || ViewportHeight >= ExtentHeight)
        {
            offset = 0;
        }
        else
        {
            if (offset + ViewportHeight >= ExtentHeight)
                offset = ExtentHeight - ViewportHeight;
        }

        _offset.Y = offset;

        if (ScrollOwner != null)
            ScrollOwner.InvalidateScrollInfo();

        InvalidateMeasure();
    }
    #endregion

    #endregion
}
