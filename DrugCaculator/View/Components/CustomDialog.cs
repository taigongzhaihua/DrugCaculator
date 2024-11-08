using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace DrugCaculator.View.Components;

public class CustomDialog : Window
{
    // 依赖属性定义
    public static readonly DependencyProperty DialogWidthProperty =
        DependencyProperty.Register(nameof(DialogWidth), typeof(double), typeof(CustomDialog), new PropertyMetadata(400.0));

    public static readonly DependencyProperty DialogHeightProperty =
        DependencyProperty.Register(nameof(DialogHeight), typeof(double), typeof(CustomDialog), new PropertyMetadata(200.0));

    public static readonly DependencyProperty DialogMinWidthProperty =
        DependencyProperty.Register(nameof(DialogMinWidth), typeof(double), typeof(CustomDialog), new PropertyMetadata(60.0));

    public static readonly DependencyProperty DialogMinHeightProperty =
        DependencyProperty.Register(nameof(DialogMinHeight), typeof(double), typeof(CustomDialog), new PropertyMetadata(60.0));

    public static readonly DependencyProperty DialogMaxWidthProperty =
        DependencyProperty.Register(nameof(DialogMaxWidth), typeof(double), typeof(CustomDialog), new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DialogMaxHeightProperty =
        DependencyProperty.Register(nameof(DialogMaxHeight), typeof(double), typeof(CustomDialog), new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty DialogBackgroundColorProperty =
        DependencyProperty.Register(nameof(DialogBackgroundColor), typeof(Brush), typeof(CustomDialog), new PropertyMetadata(Brushes.White));

    public static readonly DependencyProperty TitleBarBackgroundProperty =
        DependencyProperty.Register(nameof(TitleBarBackground), typeof(Brush), typeof(CustomDialog), new PropertyMetadata(Brushes.GhostWhite));

    public static readonly DependencyProperty TitleColorProperty =
        DependencyProperty.Register(nameof(TitleColor), typeof(Brush), typeof(CustomDialog), new PropertyMetadata(Brushes.IndianRed));

    public static readonly DependencyProperty TitleAlignmentProperty =
        DependencyProperty.Register(nameof(TitleAlignment), typeof(HorizontalAlignment), typeof(CustomDialog), new PropertyMetadata(HorizontalAlignment.Center));

    // 属性包装器
    public double DialogWidth
    {
        get => (double)GetValue(DialogWidthProperty);
        set => SetValue(DialogWidthProperty, value);
    }

    public double DialogHeight
    {
        get => (double)GetValue(DialogHeightProperty);
        set => SetValue(DialogHeightProperty, value);
    }

    public double DialogMinWidth
    {
        get => (double)GetValue(DialogMinWidthProperty);
        set => SetValue(DialogMinWidthProperty, value);
    }

    public double DialogMinHeight
    {
        get => (double)GetValue(DialogMinHeightProperty);
        set => SetValue(DialogMinHeightProperty, value);
    }

    public double DialogMaxWidth
    {
        get => (double)GetValue(DialogMaxWidthProperty);
        set => SetValue(DialogMaxWidthProperty, value);
    }

    public double DialogMaxHeight
    {
        get => (double)GetValue(DialogMaxHeightProperty);
        set => SetValue(DialogMaxHeightProperty, value);
    }

    public Brush DialogBackgroundColor
    {
        get => (Brush)GetValue(DialogBackgroundColorProperty);
        set => SetValue(DialogBackgroundColorProperty, value);
    }

    public Brush TitleBarBackground
    {
        get => (Brush)GetValue(TitleBarBackgroundProperty);
        set => SetValue(TitleBarBackgroundProperty, value);
    }

    public Brush TitleColor
    {
        get => (Brush)GetValue(TitleColorProperty);
        set => SetValue(TitleColorProperty, value);
    }

    public HorizontalAlignment TitleAlignment
    {
        get => (HorizontalAlignment)GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    public CustomDialog()
    {
        DataContext = this;
        // 设置通用窗口样式和行为
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        ResizeMode = ResizeMode.NoResize; // 禁止调整窗口大小

        // 初始化界面元素
        InitializeTemplate();
        MouseLeftButtonDown += BaseWindow_MouseLeftButtonDown;

        // 绑定 DialogWidth 和 DialogHeight 到 Window 的 Width 和 Height
        SetBinding(WidthProperty, new Binding("DialogWidth") { Source = this, Converter = new Add40Converter() });
        SetBinding(HeightProperty, new Binding("DialogHeight") { Source = this, Converter = new Add40Converter() });
        SetBinding(MinWidthProperty, new Binding("DialogMinWidth") { Source = this, Converter = new Add40Converter() });
        SetBinding(MinHeightProperty, new Binding("DialogMinHeight") { Source = this, Converter = new Add40Converter() });
        SetBinding(MaxWidthProperty, new Binding("DialogMaxWidth") { Source = this, Converter = new Add40Converter() });
        SetBinding(MaxHeightProperty, new Binding("DialogMaxHeight") { Source = this, Converter = new Add40Converter() });
    }

    private void InitializeTemplate()
    {
        // 创建控件模板
        var template = new ControlTemplate(typeof(Window));

        // 主 Grid
        var mainGridFactory = new FrameworkElementFactory(typeof(Grid));
        mainGridFactory.SetValue(EffectProperty, new DropShadowEffect
        {
            Color = Colors.SteelBlue,
            BlurRadius = 15,
            ShadowDepth = 5,
            Opacity = 0.5
        });

        // 边框
        var borderFactory = new FrameworkElementFactory(typeof(Border));
        borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(DialogBackgroundColorProperty));
        borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(15));
        borderFactory.SetValue(Border.PaddingProperty, new Thickness(0, 0, 0, 5));
        borderFactory.SetValue(ClipToBoundsProperty, true);
        borderFactory.AddHandler(SizeChangedEvent, new SizeChangedEventHandler(Window_SizeChanged));

        borderFactory.SetValue(MaxHeightProperty, new TemplateBindingExtension(DialogMaxHeightProperty));
        borderFactory.SetValue(MinHeightProperty, new TemplateBindingExtension(DialogMinHeightProperty));
        borderFactory.SetValue(HeightProperty, new TemplateBindingExtension(DialogHeightProperty));
        borderFactory.SetValue(MaxWidthProperty, new TemplateBindingExtension(DialogMaxWidthProperty));
        borderFactory.SetValue(MinWidthProperty, new TemplateBindingExtension(DialogMinWidthProperty));
        borderFactory.SetValue(WidthProperty, new TemplateBindingExtension(DialogWidthProperty));

        mainGridFactory.AppendChild(borderFactory);

        // 内部 Grid
        var innerGridFactory = new FrameworkElementFactory(typeof(Grid));

        // 添加行定义
        innerGridFactory.AppendChild(CreateRowDefinition(GridLength.Auto));
        innerGridFactory.AppendChild(CreateRowDefinition(GridLength.Auto));

        // 标题栏
        var titleGridFactory = CreateTitleBar();
        innerGridFactory.AppendChild(titleGridFactory);

        // 内容呈现器
        var contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenterFactory.SetValue(Grid.RowProperty, 1);
        innerGridFactory.AppendChild(contentPresenterFactory);

        // 将内部 Grid 设置为 Border 的子元素
        borderFactory.AppendChild(innerGridFactory);

        // 设置模板
        template.VisualTree = mainGridFactory;
        Template = template;

        // 应用模板
        ApplyTemplate();
    }

    private static FrameworkElementFactory CreateRowDefinition(GridLength height)
    {
        var rowDefinition = new FrameworkElementFactory(typeof(RowDefinition));
        rowDefinition.SetValue(RowDefinition.HeightProperty, height);
        return rowDefinition;
    }

    private FrameworkElementFactory CreateTitleBar()
    {
        // 标题栏 Grid
        var titleGridFactory = new FrameworkElementFactory(typeof(Grid));
        titleGridFactory.SetValue(Panel.BackgroundProperty, new TemplateBindingExtension(TitleBarBackgroundProperty));
        titleGridFactory.SetValue(HeightProperty, 30.0);
        titleGridFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);

        // 定义两列
        FrameworkElementFactory column1 = new(typeof(ColumnDefinition));
        column1.SetValue(ColumnDefinition.WidthProperty, new GridLength(1, GridUnitType.Star));
        titleGridFactory.AppendChild(column1);

        FrameworkElementFactory column2 = new(typeof(ColumnDefinition));
        column2.SetValue(ColumnDefinition.WidthProperty, GridLength.Auto);
        titleGridFactory.AppendChild(column2);

        // 标题文本
        FrameworkElementFactory titleTextFactory = new(typeof(TextBlock));
        titleTextFactory.SetValue(TextBlock.TextProperty, new TemplateBindingExtension(TitleProperty));
        titleTextFactory.SetValue(MarginProperty, new Thickness(40, 0, 0, 0));
        titleTextFactory.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
        titleTextFactory.SetValue(HorizontalAlignmentProperty, new TemplateBindingExtension(TitleAlignmentProperty));
        titleTextFactory.SetValue(TextBlock.FontSizeProperty, 14.0);
        titleTextFactory.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
        titleTextFactory.SetValue(TextBlock.ForegroundProperty, new TemplateBindingExtension(TitleColorProperty));
        titleGridFactory.AppendChild(titleTextFactory);

        // 关闭按钮
        FrameworkElementFactory closeButtonFactory = new(typeof(Button));
        closeButtonFactory.SetValue(ContentProperty, "\uE955");
        closeButtonFactory.SetValue(WidthProperty, 40.0);
        closeButtonFactory.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Right);
        closeButtonFactory.SetValue(Grid.ColumnProperty, 1); // 将按钮放到第二列
        closeButtonFactory.SetValue(StyleProperty, new DynamicResourceExtension("CloseButton")); // 应用样式
        closeButtonFactory.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(CloseButton_Click));
        titleGridFactory.AppendChild(closeButtonFactory);

        return titleGridFactory;
    }

    // 实现窗口拖动
    private void BaseWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    // 窗口大小变化时更新裁剪区域
    private static void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is Border border) UpdateClipRegion(border); // 更新裁剪区域
    }

    // 更新 Border 的裁剪区域，以匹配控件的当前大小和圆角
    private static void UpdateClipRegion(Border border)
    {
        border.Clip = new RectangleGeometry
        {
            Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
            RadiusX = border.CornerRadius.TopLeft,
            RadiusY = border.CornerRadius.TopLeft
        };
    }

    // 关闭按钮点击事件处理
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close(); // 关闭窗口
    }
}

// 转换器：将值加上 40
public class Add40Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue + 40;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue - 40;
        }
        return value;
    }
}